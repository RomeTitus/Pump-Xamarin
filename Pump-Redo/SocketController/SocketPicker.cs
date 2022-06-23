using System.Linq;
using System.Threading.Tasks;
using Pump.Database;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.SocketController.BT;
using Pump.SocketController.Firebase;
using Pump.SocketController.Network;
using Xamarin.Forms;

namespace Pump.SocketController
{
    public class SocketPicker
    {
        private readonly InitializeBlueTooth _initializeBlueTooth;
        private readonly InitializeFirebase _initializeFirebase;
        private readonly InitializeNetwork _initializeNetwork;
        private readonly ObservableIrrigation _observableIrrigation;
        private readonly DatabaseController _database;
        private readonly FirebaseManager _manager;
        private IrrigationConfiguration _configuration;

        public SocketPicker(FirebaseManager manager, DatabaseController database, ObservableIrrigation observableIrrigation)
        {
            _database = database;
            _manager = manager;
            _observableIrrigation = observableIrrigation;
            _initializeFirebase = new InitializeFirebase(_manager, observableIrrigation);
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

        private async Task Subscribe()
        {
            _observableIrrigation.IsDisposable = false;
            foreach (var configuration in _database.GetControllerConfigurationList())
            {
                if (configuration.ConnectionType == 0)
                    _initializeFirebase.SubscribeFirebase();
                else if (configuration.ConnectionType == 1)
                    await _initializeNetwork.SubscribeNetwork();
                //else if (IrrigationConfiguration.ConnectionType == 2) 
                //    await _initializeBlueTooth.SubscribeBle();
            }
           
        }

        public async Task<string> SendCommand(object sendObject)
        {
            string result;
            switch (_configuration.ConnectionType)
            {
                case 0:
                    result = await _manager.Description(sendObject, _configuration.Path);
                    break;
                case 1:
                    _initializeNetwork.RequestIrrigationTimer.Restart();
                    _initializeNetwork.RequestNow = true;
                    result = await _initializeNetwork.NetworkManager.SendAndReceiveToNetwork(
                        SocketCommands.Descript(sendObject), _configuration);
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