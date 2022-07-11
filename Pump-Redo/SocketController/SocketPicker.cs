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
        private readonly Dictionary<IrrigationConfiguration, ObservableIrrigation> _observableDict;
        private readonly FirebaseManager _firebaseManager;

        public SocketPicker(FirebaseManager firebaseManager, Dictionary<IrrigationConfiguration, ObservableIrrigation> observableDict)
        {
            _firebaseManager = firebaseManager;
            _observableDict = observableDict;
            _initializeFirebase = new InitializeFirebase(_firebaseManager, observableDict);
            _initializeNetwork = new InitializeNetwork(observableDict);
            _initializeBlueTooth = new InitializeBlueTooth(observableDict);
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
            //_observableIrrigation.IsDisposable = false;
            
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

        public async Task<string> SendCommand(object sendObject, IrrigationConfiguration targetedIrrigation)
        {
            string result;
            switch (targetedIrrigation.ConnectionType)
            {
                case 0:
                    result = await _firebaseManager.Description(sendObject, targetedIrrigation.Path);
                    break;
                case 1:
                    _initializeNetwork.RequestIrrigationTimer.Restart();
                    _initializeNetwork.RequestNow = true;
                    result = await _initializeNetwork.NetworkManager.SendAndReceiveToNetwork(
                        SocketCommands.Descript(sendObject), targetedIrrigation);
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

        public async Task<List<IrrigationConfiguration>> GetIrrigationConfigurations(User user)
        {
            _firebaseManager.InitializeFirebase(user);
            return await _firebaseManager.GetIrrigationConfigList();
        }
    }
}