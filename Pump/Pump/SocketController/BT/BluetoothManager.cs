using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Extensions;


namespace Pump.SocketController
{
    public class BluetoothManager
    {
        public const string Irrigation_Service_Code = "00000001-710e-4a5b-8d75-3e5b444bc3cf";
        #region Singleton
        private static readonly Lazy<BluetoothManager> lazyBluetoothManager = new Lazy<BluetoothManager>(() => new BluetoothManager());
        public static BluetoothManager Instance
        {
            get { return lazyBluetoothManager.Value; }

        }
        #endregion
        
        public IAdapter AdapterBLE { get; set; }
        public IDevice BLEDevice { get; set; }

        public ObservableCollection<IDevice> DeviceList { get; set; }

        private bool ledStatus;
        private readonly Guid serviceGuid = Guid.Parse("713D0000-503E-4C75-BA94-3148F18D941E");

        public BluetoothManager()
        {
            AdapterBLE = CrossBluetoothLE.Current.Adapter;
            DeviceList = new ObservableCollection<IDevice>();

            AdapterBLE.DeviceDiscovered += Adapter_DeviceDiscovered;
            AdapterBLE.DeviceConnected += Adapter_DeviceConnected;
            AdapterBLE.DeviceDisconnected += Adapter_DeviceDisconnected;
            AdapterBLE.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            AdapterBLE.ScanTimeout = 5000;
        }

        public async Task StartScanning()
        {
            await StartScanning(Guid.Empty);
        }

        async Task StartScanning(Guid forService)
        {
            if (AdapterBLE.IsScanning)
            {
                await AdapterBLE.StopScanningForDevicesAsync();
                Debug.WriteLine("adapter.StopScanningForDevices()");
            }
            else
            {
                DeviceList.Clear();
                AdapterBLE.ScanMode = ScanMode.LowPower;
                await DisconnectDevice();
                await AdapterBLE.StartScanningForDevicesAsync();
                
                Debug.WriteLine("adapter.StartScanningForDevices(" + forService + ")");
            }
        }

        public async Task ConnectToDevice(IDevice device)
        {
            if (BLEDevice == null)
            {
                await AdapterBLE.ConnectToDeviceAsync(device);
                BLEDevice = device;
            }
        }

        public async void StopScanning()
        {
            if (AdapterBLE.IsScanning)
            {
                Debug.WriteLine("Still scanning, stopping the scan");
                await AdapterBLE.StopScanningForDevicesAsync();
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

        void Adapter_DeviceDisconnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            //DeviceDisconnectedEvent?.Invoke(sender,e);
            Debug.WriteLine("Device already disconnected");
        }

        void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            AdapterBLE.StopScanningForDevicesAsync();
            Debug.WriteLine("Timeout", "Bluetooth scan timeout elapsed");
        }

        public async Task DisconnectDevice()
        {
            if (BLEDevice != null)
            {
                await AdapterBLE.DisconnectDeviceAsync(BLEDevice);
                BLEDevice.Dispose();
                BLEDevice = null;
            }
        }

        public async Task<bool> IsController()
        {
            var services = await BLEDevice.GetServicesAsync();
            return services.FirstOrDefault(x => x.Id == Guid.Parse(Irrigation_Service_Code)) != null;
        }

        public async Task<string> WriteToBle(string dataToSend, int? timeout = null)
        {
            var services = await BLEDevice.GetServicesAsync();
            if (services == null)
                return null;
            var characteristics = await services[0].GetCharacteristicsAsync();
            var characteristic = characteristics[0];
            if (characteristic == null || !characteristic.CanWrite) return null;
            var bytes = Encoding.ASCII.GetBytes(dataToSend);
            await characteristic.WriteAsync(bytes);
            if (timeout != null)
                Thread.Sleep(8000);
            var result =  await characteristic.ReadAsync();
            return Encoding.ASCII.GetString(result, 0, result.Length);
        }
    }
}
