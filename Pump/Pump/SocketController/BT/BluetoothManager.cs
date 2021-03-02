using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;

namespace Pump.SocketController.BT
{
    public class BluetoothManager
    {
        private ICharacteristic _loadedCharacteristic = null;
        public const string IrrigationServiceCode = "00000001-710e-4a5b-8d75-3e5b444bc3cf";
        #region Singleton
        private static readonly Lazy<BluetoothManager> lazyBluetoothManager = new Lazy<BluetoothManager>(() => new BluetoothManager());
        public static BluetoothManager Instance
        {
            get { return lazyBluetoothManager.Value; }

        }
        #endregion
        
        public IAdapter AdapterBle { get; set; }
        public IDevice BleDevice { get; set; }

        public ObservableCollection<IDevice> DeviceList { get; set; }

        public BluetoothManager()
        {
            AdapterBle = CrossBluetoothLE.Current.Adapter;
            DeviceList = new ObservableCollection<IDevice>();

            AdapterBle.DeviceDiscovered += Adapter_DeviceDiscovered;
            AdapterBle.DeviceConnected += Adapter_DeviceConnected;
            AdapterBle.DeviceDisconnected += Adapter_DeviceDisconnected;
            AdapterBle.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            AdapterBle.ScanTimeout = 10000;
        }

        public async Task StartScanning()
        {
            await StartScanning(Guid.Empty);
        }

        async Task StartScanning(Guid forService)
        {
            if (AdapterBle.IsScanning)
            {
                await AdapterBle.StopScanningForDevicesAsync();
                Debug.WriteLine("adapter.StopScanningForDevices()");
            }
            else
            {
                DeviceList.Clear();
                AdapterBle.ScanMode = ScanMode.LowPower;
                await DisconnectDevice();
                await AdapterBle.StartScanningForDevicesAsync();
                
                Debug.WriteLine("adapter.StartScanningForDevices(" + forService + ")");
            }
        }

        public async Task ConnectToDevice(IDevice device)
        {
            if (BleDevice == null)
            {
                await AdapterBle.ConnectToDeviceAsync(device);
                BleDevice = device;
            }
        }

        public async Task StopScanning()
        {
            if (AdapterBle.IsScanning)
            {
                Debug.WriteLine("Still scanning, stopping the scan");
                await AdapterBle.StopScanningForDevicesAsync();
            }
        }

        void Adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            DeviceList.Add(e.Device);
        }

        void Adapter_DeviceConnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            Debug.WriteLine("Device already connected");
        }

        async void Adapter_DeviceDisconnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            await DisconnectDevice();
            _loadedCharacteristic = null;
            //DeviceDisconnectedEvent?.Invoke(sender,e);
            Debug.WriteLine("Device already disconnected");
        }

        void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            AdapterBle.StopScanningForDevicesAsync();
            Debug.WriteLine("Timeout", "Bluetooth scan timeout elapsed");
        }

        public async Task DisconnectDevice()
        {
            if (BleDevice != null)
            {
                await AdapterBle.DisconnectDeviceAsync(BleDevice);
                BleDevice.Dispose();
                BleDevice = null;
            }
        }

        public async Task<bool> IsController()
        {
            var services = await BleDevice.GetServicesAsync();
            return services.FirstOrDefault(x => x.Id == Guid.Parse(IrrigationServiceCode)) != null;
        }

        private string ConvertForIrrigation(string dataToBeConverted)
        {
            var updated = dataToBeConverted.Replace("true", "True");
            updated = updated.Replace("false", "False");
            return updated;
        }

        private string ConvertForApplication(string dataToBeConverted)
        {
            var updated = dataToBeConverted.Replace("True", "true");
            updated = updated.Replace("False", "false");
            return updated;
        }
        
        public async Task<string> SendAndReceiveToBle(JObject dataToSend, int timeout = 0)
        {
            if (_loadedCharacteristic == null)
            {
                var services = await BleDevice.GetServicesAsync();
                if (services == null)
                    return null;
                var characteristics = await services[0].GetCharacteristicsAsync();
                _loadedCharacteristic = characteristics[0];
            }


            if (_loadedCharacteristic == null || !_loadedCharacteristic.CanWrite) return null;

            var fullData = false;
            var bleReplyBytes = new List<byte>();
            var partitionNumber = 0;
            while (fullData == false)
            {
                if (dataToSend.ContainsKey("Task") && dataToSend["Task"]["PartitionNumber"] != null)
                    dataToSend["Task"]["PartitionNumber"] = partitionNumber;
                

                var receiveBytes = await WriteToBle(dataToSend, timeout);
                partitionNumber++;
                bleReplyBytes.AddRange(receiveBytes);
                if (receiveBytes.Length != 512)
                    fullData = true;
            }
            return ConvertForApplication(Encoding.ASCII.GetString(bleReplyBytes.ToArray(), 0, bleReplyBytes.Count));
        }

        private async Task<byte[]> WriteToBle(JObject dataToSend, int timeout = 0)
        {
            var bytes = Encoding.ASCII.GetBytes(ConvertForIrrigation(dataToSend.ToString()));
            await _loadedCharacteristic.WriteAsync(bytes);
            Thread.Sleep(timeout);
            var result = await _loadedCharacteristic.ReadAsync();
            if (result == bytes)
                throw new Exception("Controller did not reply back using BlueTooth");
            return result;
        }
    }
}
