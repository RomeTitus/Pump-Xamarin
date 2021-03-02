using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.EventArgs;
using Pump.Class;
using Pump.Database;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.SocketController.BT;
using Xamarin.Forms;

namespace Pump.SocketController
{
    public class SocketPicker
    {
        private readonly InitializeFirebase _initializeFirebase;
        private readonly InitializeBlueTooth _initializeBlueTooth;
        private readonly NotificationEvent _notificationEvent;

        public SocketPicker(ObservableIrrigation observableIrrigation, NotificationEvent notificationEvent)
        {
            var observableIrrigation1 = observableIrrigation;
            _notificationEvent = notificationEvent;
            _initializeFirebase = new InitializeFirebase(observableIrrigation1);
            _initializeBlueTooth = new InitializeBlueTooth(observableIrrigation1);
            //Subscribe();


        }

        private void AdapterBLEOnDeviceConnected(object sender, DeviceEventArgs e)
        {
            _notificationEvent.Notification("Bluetooth status", "Status: " + e.Device.State + "\nRssi: " + e.Device.Rssi, "Understood");
        }

        public async void ConnectionPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            if (picker.SelectedIndex == -1)
                return;
            var controllerList = new DatabaseController().GetControllerConnectionList();
            var selectedConnection = controllerList[picker.SelectedIndex];
            await Disposable();
            new DatabaseController().SetSelectedController(selectedConnection);
            await Subscribe();
        }

        private async Task Disposable()
        {
            var pumpConnection = new DatabaseController().GetControllerConnectionSelection();

            if(pumpConnection.ConnectionType == 0)
                _initializeFirebase.Disposable();
            if (pumpConnection.ConnectionType == 2)
            {
                _initializeBlueTooth.BlueToothManager.AdapterBle.DeviceConnected -= AdapterBLEOnDeviceConnected;
                _initializeBlueTooth.BlueToothManager.AdapterBle.DeviceConnectionLost -= AdapterBLEOnDeviceConnected;
                _initializeBlueTooth.BlueToothManager.AdapterBle.DeviceDisconnected -= AdapterBLEOnDeviceConnected;
                _initializeBlueTooth.Disposable();
            }
                
        }

        private async Task Subscribe()
        {
            var pumpConnection = new DatabaseController().GetControllerConnectionSelection();
            if (pumpConnection.ConnectionType == 0)
                _initializeFirebase.SubscribeFirebase();
            if (pumpConnection.ConnectionType == 2)
            {
                _initializeBlueTooth.BlueToothManager.AdapterBle.DeviceConnected += AdapterBLEOnDeviceConnected;
                _initializeBlueTooth.BlueToothManager.AdapterBle.DeviceConnectionLost += AdapterBLEOnDeviceConnected;
                _initializeBlueTooth.BlueToothManager.AdapterBle.DeviceDisconnected += AdapterBLEOnDeviceConnected;
                await _initializeBlueTooth.SubscribeBle();
            }
            
        }
    }
}
