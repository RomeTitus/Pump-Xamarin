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
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using Pump.Database;

namespace Pump.SocketController.BT
{
    public class BluetoothManager
    {
        private const string IrrigationServiceCode = "7949B569-7FC4-465E-B35B-1B5B200AC8C3";
        private ICharacteristic _loadedCharacteristic;

        public BluetoothManager()
        {
            AdapterBle = CrossBluetoothLE.Current.Adapter;
            IrrigationDeviceBt = new ObservableCollection<IDevice>();

            AdapterBle.DeviceDiscovered += Adapter_DeviceDiscovered;
            AdapterBle.DeviceConnected += Adapter_DeviceConnected;
            AdapterBle.DeviceDisconnected += Adapter_DeviceDisconnected;
            AdapterBle.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            //30 Seconds spent Scanning
            AdapterBle.ScanTimeout = 30000;
        }

        public IAdapter AdapterBle { get; }
        public IDevice BleDevice { get; private set; }
        public ObservableCollection<IDevice> IrrigationDeviceBt { get; }

        public async Task StartScanning()
        {
            await StartScanning(Guid.Empty);
        }

        public async Task StartScanning(Guid forService)
        {
            if (AdapterBle.IsScanning)
            {
                await AdapterBle.StopScanningForDevicesAsync();
                Debug.WriteLine("adapter.StopScanningForDevices()");
            }

            if (IrrigationDeviceBt.Any())
                IrrigationDeviceBt.Clear();
            AdapterBle.ScanMode = ScanMode.Balanced;
            await AdapterBle.StartScanningForDevicesAsync();
        }

        public async Task<bool> ConnectToKnownDevice(Guid deviceId)
        {
            var cancellationToken = new CancellationTokenSource();
            cancellationToken.CancelAfter(10000);
            if (BleDevice == null)
                try
                {
                    var result =
                        await AdapterBle.ConnectToKnownDeviceAsync(deviceId,
                            cancellationToken: cancellationToken.Token);
                    BleDevice = result;
                    SaveIDevice(BleDevice);
                }
                catch (Exception)
                {
                    cancellationToken.Dispose();
                }

            return BleDevice != null;
        }

        public async Task<bool> ConnectToDevice(IDevice device, int retry = 0)
        {
            if (BleDevice != null) return BleDevice != null;

            var tries = -1;

            while (tries < retry)
            {
                var connected = true;
                var cancellationToken = new CancellationTokenSource();
                    cancellationToken.CancelAfter(23000);
                    
                    try
                    {
                        await AdapterBle.StopScanningForDevicesAsync();
                        await AdapterBle.ConnectToDeviceAsync(device, cancellationToken: cancellationToken.Token);
                    }
                    catch (DeviceConnectionException deviceConnectionException)
                    {
                        connected = false;
                        tries++;
                        if (tries == retry)
                            throw new ArgumentException("Failed to connect \n" + deviceConnectionException.Message);
                    }

                    catch (Exception e)
                    {
                        connected = false;
                        tries++;
                        if (tries == retry)
                            throw;
                    }
                    if (connected)
                        break;
            }

            BleDevice = device;
            SaveIDevice(BleDevice);
            return BleDevice != null;
        }

        private void SaveIDevice(IDevice iDevice)
        {
            if (iDevice == null)
                return;
            var currentController = new DatabaseController().GetControllerConnectionSelection();
            if (currentController != null && iDevice.NativeDevice.ToString() == currentController.Mac)
                if (currentController.IDeviceGuid != iDevice.Id)
                {
                    currentController.IDeviceGuid = iDevice.Id;
                    new DatabaseController().UpdateControllerConnection(currentController);
                }
        }

        private void Adapter_DeviceDiscovered(object sender, DeviceEventArgs e)
        {
            IrrigationDeviceBt.Add(e.Device);
        }

        private void Adapter_DeviceConnected(object sender, DeviceEventArgs e)
        {
            Debug.WriteLine("Device already connected");
        }

        private async void Adapter_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            await DisconnectDevice();
            _loadedCharacteristic = null;
            //DeviceDisconnectedEvent?.Invoke(sender,e);
            Debug.WriteLine("Device already disconnected");
        }

        private void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            AdapterBle.StopScanningForDevicesAsync();
            Debug.WriteLine("Timeout", "Bluetooth scan timeout elapsed");
        }

        private async Task DisconnectDevice()
        {
            if (BleDevice != null)
            {
                await AdapterBle.DisconnectDeviceAsync(BleDevice);
                BleDevice.Dispose();
                BleDevice = null;
            }
        }

        public async Task<bool> IsValidController()
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

        public async Task<string> SendAndReceiveToBleAsync(JObject dataToSend, int timeout = 0)
        {
            if (_loadedCharacteristic == null)
            {
                var services = await BleDevice.GetServicesAsync();
                if (services == null || !services.Any())
                    return null;
                var characteristics = await services[0].GetCharacteristicsAsync();
                _loadedCharacteristic = characteristics[0];
            }


            if (_loadedCharacteristic == null || !_loadedCharacteristic.CanWrite) return null;

            var fullData = false;
            var bleReplyBytes = new List<byte>();
            var key = Encoding.ASCII.GetBytes(SocketCommands.GenerateKey(4)).ToList();
            var partNumber = 0;
            while (fullData == false)
            {
                if (dataToSend.ContainsKey("Task"))
                    try
                    {
                        if(dataToSend["Task"].Type != JTokenType.String)
                            dataToSend["Task"]["Part"] = partNumber;
                    }
                    catch
                    {
                        // ignored
                    }

                var bytes = Encoding.ASCII.GetBytes(ConvertForIrrigation(dataToSend.ToString())).ToList();
                var finalBytesReceived = Array.Empty<byte>();

                for (var i = 0; i < bytes.Count; i += 508)
                {
                    var sendingBytes = bytes.Count > i + 508 ? bytes.GetRange(i, i + 508) : bytes;

                    sendingBytes.InsertRange(0, key);

                    finalBytesReceived = await WriteToBle(sendingBytes.ToArray(), timeout);
                }

                bleReplyBytes.AddRange(finalBytesReceived);
                if (finalBytesReceived.Length != 512)
                    fullData = true;
                partNumber++;
            }

            return ConvertForApplication(Encoding.ASCII.GetString(bleReplyBytes.ToArray(), 0, bleReplyBytes.Count));
        }

        private async Task<byte[]> WriteToBle(byte[] bytesToSend, int timeout = 0)
        {
            await _loadedCharacteristic.WriteAsync(bytesToSend);
            await Task.Delay(timeout);
            var result = await _loadedCharacteristic.ReadAsync();
            if (Encoding.ASCII.GetString(result, 0, result.Length) ==
                Encoding.ASCII.GetString(bytesToSend, 0, bytesToSend.Length))
                throw new Exception("Controller did not reply back using BlueTooth \n Reboot required");
            return result;
        }
    }
}