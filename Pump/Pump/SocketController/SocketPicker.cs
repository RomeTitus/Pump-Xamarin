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
        private readonly InitializeNetwork _initializeNetwork;
        private readonly InitializeBlueTooth _initializeBlueTooth;
        private readonly NotificationEvent _notificationEvent;

        public SocketPicker(ObservableIrrigation observableIrrigation, NotificationEvent notificationEvent)
        {
            _notificationEvent = notificationEvent;
            _initializeFirebase = new InitializeFirebase(observableIrrigation);
            _initializeNetwork = new InitializeNetwork(observableIrrigation);
            _initializeBlueTooth = new InitializeBlueTooth(observableIrrigation);
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
            else if (pumpConnection.ConnectionType == 1)
                _initializeNetwork.Disposable();
            else if (pumpConnection.ConnectionType == 2)
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
            else if (pumpConnection.ConnectionType == 1)
                await _initializeNetwork.SubscribeNetwork();
            else if (pumpConnection.ConnectionType == 2)
            {
                _initializeBlueTooth.BlueToothManager.AdapterBle.DeviceConnected += AdapterBLEOnDeviceConnected;
                _initializeBlueTooth.BlueToothManager.AdapterBle.DeviceConnectionLost += AdapterBLEOnDeviceConnected;
                _initializeBlueTooth.BlueToothManager.AdapterBle.DeviceDisconnected += AdapterBLEOnDeviceConnected;
                await _initializeBlueTooth.SubscribeBle();
            }
        }
        public async Task<string> SendCommand(object sendObject)
        {
            var pumpConnection = new DatabaseController().GetControllerConnectionSelection();
            if (pumpConnection.ConnectionType == 0)
            {
                return await new Authentication().Descript(sendObject);
            }

            else if (pumpConnection.ConnectionType == 1)
            {
                return await _initializeNetwork._networkManager.SendAndReceiveToNetwork(SocketCommands.Descript(sendObject), pumpConnection);
            }
            else if (pumpConnection.ConnectionType == 2)
            {
                return await _initializeBlueTooth.BlueToothManager.SendAndReceiveToBle(SocketCommands.Descript(sendObject));
            }

            return "Unknown Operation/Could not Identify user operations";
        }
    }
}
