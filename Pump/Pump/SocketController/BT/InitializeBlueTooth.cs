using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
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
            BlueToothManager.AdapterBle.ScanTimeoutElapsed += AdapterBLE_ScanTimeoutElapsed;
            await BlueToothManager.StartScanning();
        }

        private async void AdapterBLE_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            if (!_isAlive)
            {
                await BlueToothManager.StartScanning();
            }
        }

        public void Disposable()
        {
            _isAlive = false;
            BlueToothManager.DeviceList.CollectionChanged -= PopulateBlueToothDeviceEvent;
            BlueToothManager.AdapterBle.ScanTimeoutElapsed -= AdapterBLE_ScanTimeoutElapsed;
        }

        private async Task ConnectToDevice(IDevice iDevice)
        {
            await BlueToothManager.StopScanning();
            _isAlive = true;

            var oldIrrigationTuple =
                new Tuple<List<CustomSchedule>, List<Schedule>, List<Equipment>, List<ManualSchedule>, List<Sensor>, List<Site>, List<SubController>>
                    (new List<CustomSchedule>(), new List<Schedule>(), new List<Equipment>(), new List<ManualSchedule>(), new List<Sensor>(), new List<Site>(), new List<SubController>());
            
            while (_isAlive)
            {
                try
                {
                    await BlueToothManager.ConnectToDevice(iDevice);
                    
                    var irrigationJObject = JObject.Parse(await GetIrrigationData());
                    
                    var irrigationTuple = IrrigationConvert.IrrigationJObjectToList(irrigationJObject);

                    var irrigationTupleEditState =
                        IrrigationConvert.CheckUpdatedStatus(irrigationTuple, oldIrrigationTuple);

                    
                    IrrigationConvert.UpdateObservableIrrigation(_observableIrrigation, irrigationTupleEditState);
                    oldIrrigationTuple = irrigationTuple;
                }
                catch (DeviceConnectionException ex)
                {
                    _isAlive = false;
                    OnConnectionLost();
                    await BlueToothManager.StartScanning();
                    break;
                }
                catch (Exception ex)
                {
                    _isAlive = false;
                    OnConnectionLost();
                    await BlueToothManager.StartScanning();
                    break;
                }
                //await Task.Delay(15000);
            }
        }

        private void OnConnectionLost()
        {
            _observableIrrigation.EquipmentList.Clear();
            _observableIrrigation.SensorList.Clear();
            _observableIrrigation.ManualScheduleList.Clear();
            _observableIrrigation.ScheduleList.Clear();
            _observableIrrigation.CustomScheduleList.Clear();
            _observableIrrigation.SiteList.Clear();
            _observableIrrigation.SubControllerList.Clear();
            _observableIrrigation.AliveList.Clear();

            _observableIrrigation.EquipmentList.Add(null);
            _observableIrrigation.SensorList.Add(null);
            _observableIrrigation.ManualScheduleList.Add(null);
            _observableIrrigation.ScheduleList.Add(null);
            _observableIrrigation.CustomScheduleList.Add(null);
            _observableIrrigation.SiteList.Add(null);
            _observableIrrigation.SubControllerList.Add(null);
            _observableIrrigation.AliveList.Add(null);
            
        }

        private async Task<string> GetIrrigationData()
        {
            return await BlueToothManager.SendAndReceiveToBle(SocketCommands.AllTogether());
        }


    }
}
