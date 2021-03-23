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
    class InitializeNetwork
    {
        private readonly ObservableIrrigation _observableIrrigation;
        public readonly NetworkManager _networkManager;
        private readonly PumpConnection _pumpConnection;
        private bool _isAlive;
        public InitializeNetwork(ObservableIrrigation observableIrrigation)
        {
            _observableIrrigation = observableIrrigation;
            _pumpConnection = new DatabaseController().GetControllerConnectionSelection();
            _networkManager = new NetworkManager();
        }


        public async Task SubscribeNetwork()
        {
            await ConnectToDevice();
        }

        public void Disposable()
        {
            _isAlive = false;
        }

        private async Task ConnectToDevice()
        {
            _isAlive = true;

            var oldIrrigationTuple =
                new Tuple<List<CustomSchedule>, List<Schedule>, List<Equipment>, List<ManualSchedule>, List<Sensor>, List<Site>, List<SubController>>
                    (new List<CustomSchedule>(), new List<Schedule>(), new List<Equipment>(), new List<ManualSchedule>(), new List<Sensor>(), new List<Site>(), new List<SubController>());
            
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
                catch (Exception ex)
                {
                    _isAlive = false;
                    OnConnectionLost();
                    break;
                }
                await Task.Delay(15000);
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
            return await _networkManager.SendAndReceiveToNetwork(SocketCommands.AllTogether(), _pumpConnection);
        }


    }
}
