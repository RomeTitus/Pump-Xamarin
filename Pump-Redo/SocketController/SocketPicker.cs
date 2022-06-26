using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.SocketController.BT;
using Pump.SocketController.Firebase;
using Pump.SocketController.Network;

namespace Pump.SocketController
{
    public class SocketPicker
    {
        private readonly InitializeBlueTooth _initializeBlueTooth;
        private readonly InitializeFirebase _initializeFirebase;
        private readonly InitializeNetwork _initializeNetwork;
        private readonly ObservableIrrigation _observableIrrigation;
        private readonly FirebaseManager _firebaseManager;
        public IrrigationConfiguration TargetedIrrigation { get; set; }

        public SocketPicker(FirebaseManager firebaseManager, ObservableIrrigation observableIrrigation)
        {
            _firebaseManager = firebaseManager;
            _observableIrrigation = observableIrrigation;
            _initializeFirebase = new InitializeFirebase(_firebaseManager, observableIrrigation);
            _initializeNetwork = new InitializeNetwork(observableIrrigation);
            _initializeBlueTooth = new InitializeBlueTooth(observableIrrigation);
        }

        

        private void Disposable()
        {
            /*
            _observableIrrigation.IsDisposable = true;
            var irrigationConfiguration = new DatabaseController().GetControllerConnectionSelection();
            
            if (irrigationConfiguration.ConnectionType == 0)
                _initializeFirebase.Disposable();
            else if (irrigationConfiguration.ConnectionType == 1)
                _initializeNetwork.Disposable();
            else if (irrigationConfiguration.ConnectionType == 2) 
                _initializeBlueTooth.Disposable();
        */
        }

        public async Task Subscribe(List<IrrigationConfiguration> configurationList, User user = null)
        {
            _observableIrrigation.IsDisposable = false;
            
            foreach (var configuration in configurationList)
            {
                if (configuration.ConnectionType == 0)
                {
                    if(user!= null)
                        _firebaseManager.InitializeFirebase(user);
                    _initializeFirebase.SubscribeFirebase();
                }
                    
                else if (configuration.ConnectionType == 1)
                    await _initializeNetwork.SubscribeNetwork();
                //else if (IrrigationConfiguration.ConnectionType == 2) 
                //    await _initializeBlueTooth.SubscribeBle();
            }
           
        }

        public async Task<string> SendCommand(object sendObject)
        {
            string result;
            switch (TargetedIrrigation.ConnectionType)
            {
                case 0:
                    result = await _firebaseManager.Description(sendObject, TargetedIrrigation.Path);
                    break;
                case 1:
                    _initializeNetwork.RequestIrrigationTimer.Restart();
                    _initializeNetwork.RequestNow = true;
                    result = await _initializeNetwork.NetworkManager.SendAndReceiveToNetwork(
                        SocketCommands.Descript(sendObject), TargetedIrrigation);
                    break;
                case 2:
                    _initializeBlueTooth.RequestIrrigationTimer.Restart();
                    result = await _initializeBlueTooth.BlueToothManager.SendAndReceiveToBleAsync(
                        SocketCommands.Descript(sendObject));
                    _initializeBlueTooth.RequestNow = true;
                    break;
                default:
                    result = "Unknown Operation/Could not Identify user operations";
                    break;
            }
            return result;
        }

        public BluetoothManager BluetoothManager()
        {
            return _initializeBlueTooth.BlueToothManager;
        }
    }
}