using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Plugin.BLE.Abstractions.Contracts;
using Pump.Database;
using Pump.Droid.Database.Table;
using Pump.IrrigationController;

namespace Pump.SocketController.BT
{
    class InitializeBlueTooth
    {
        private readonly ObservableIrrigation _observableIrrigation;
        public readonly BluetoothManager BlueToothManager;
        private readonly PumpConnection _pumpConnection;
        private bool _isAlive;
        public InitializeBlueTooth(ObservableIrrigation observableIrrigation)
        {
            _observableIrrigation = observableIrrigation;
            _pumpConnection = new DatabaseController().GetControllerConnectionSelection();
            BlueToothManager = new BluetoothManager();
        }


        

        private async void PopulateBlueToothDeviceEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            var controllerBle =
                BlueToothManager.DeviceList.FirstOrDefault(x => x.NativeDevice.ToString() == _pumpConnection.Mac);
            if (controllerBle != null && _isAlive == false)
                await ConnectToDevice(controllerBle);
        }

        public async Task SubscribeBle()
        {
            BlueToothManager.DeviceList.CollectionChanged += PopulateBlueToothDeviceEvent;
            BlueToothManager.AdapterBLE.ScanTimeoutElapsed += AdapterBLE_ScanTimeoutElapsed;
            await BlueToothManager.StartScanning();
        }

        private void AdapterBLE_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void Disposable()
        {
            _isAlive = false;
            BlueToothManager.DeviceList.CollectionChanged -= PopulateBlueToothDeviceEvent;
            BlueToothManager.AdapterBLE.ScanTimeoutElapsed -= AdapterBLE_ScanTimeoutElapsed;

        }

        private async Task ConnectToDevice(IDevice iDevice)
        {
            await BlueToothManager.StopScanning();
            _isAlive = true;

            var oldIrrigationTuple =
                new Tuple<List<CustomSchedule>, List<Schedule>, List<Equipment>, List<ManualSchedule>, List<Sensor>, List<Site>, List<SubController>>
                    (new List<CustomSchedule>(), new List<Schedule>(), new List<Equipment>(), new List<ManualSchedule>(), new List<Sensor>(), new List<Site>(), new List<SubController>());

            await BlueToothManager.ConnectToDevice(iDevice);
            
            while (_isAlive)
            {
                try
                {
                    var irrigationJObject = JObject.Parse(await GetIrrigationData());
                    
                    var irrigationTuple = IrrigationConvert.IrrigationJObjectToList(irrigationJObject);

                    var irrigationTupleEditState =
                        IrrigationConvert.CheckUpdatedStatus(irrigationTuple, oldIrrigationTuple);

                    
                    IrrigationConvert.UpdateObservableIrrigation(_observableIrrigation, irrigationTupleEditState);
                    oldIrrigationTuple = irrigationTuple;
                }
                catch (Exception exception)
                {
                    _isAlive = false;
                }
                await Task.Delay(15000);
            }
        }

        private async Task<string> GetIrrigationData()
        {
            return await BlueToothManager.SendAndReceiveToBle(SocketCommands.AllTogether());
        }

    }
}
