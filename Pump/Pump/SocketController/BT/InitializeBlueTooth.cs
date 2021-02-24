using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Pump.Database;
using Pump.Droid.Database.Table;
using Pump.IrrigationController;
using Xamarin.Forms;

namespace Pump.SocketController.BT
{
    class InitializeBlueTooth
    {
        private readonly ObservableIrrigation _observableIrrigation;
        private readonly BluetoothManager _bluetoothManager;
        private readonly PumpConnection _pumpConnection;
        private bool isAlive;
        public InitializeBlueTooth(ObservableIrrigation observableIrrigation)
        {
            _observableIrrigation = observableIrrigation;
            _pumpConnection = new DatabaseController().GetControllerConnectionSelection();
            _bluetoothManager = new BluetoothManager();
            
        }

        private void AdapterBLE_DeviceConnected(object sender, DeviceEventArgs e)
        {
            //TODO We need some sort of notifiction here 
        }

        private void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            //TODO We need some sort of notifiction here 
        }

        private async void PopulateBluetoothDeviceEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            var controllerBle =
                _bluetoothManager.DeviceList.FirstOrDefault(x => x.NativeDevice.ToString() == _pumpConnection.Mac);
            if (controllerBle != null)
                await ConnectToDevice(controllerBle);
        }

        public async Task SubscribeFirebase()
        {
            isAlive = true;
            _bluetoothManager.DeviceList.CollectionChanged += PopulateBluetoothDeviceEvent;
            _bluetoothManager.AdapterBLE.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            _bluetoothManager.AdapterBLE.DeviceConnected += AdapterBLE_DeviceConnected;
            await _bluetoothManager.StartScanning();
        }

        public void Disposable()
        {
            isAlive = false;
            _bluetoothManager.DeviceList.CollectionChanged -= PopulateBluetoothDeviceEvent;
            _bluetoothManager.AdapterBLE.ScanTimeoutElapsed -= Adapter_ScanTimeoutElapsed;
            _bluetoothManager.AdapterBLE.DeviceConnected -= AdapterBLE_DeviceConnected;
        }

        private async Task ConnectToDevice(IDevice iDevice)
        {
            await _bluetoothManager.ConnectToDevice(iDevice);
            while (isAlive)
            {
                var test = await GetIrrigationData();
                Thread.Sleep(5000);
            }
            
        }

        private async Task<string> GetIrrigationData()
        {
            return await _bluetoothManager.WriteToBle(SocketCommands.AllTogether());
        }
    }
}
