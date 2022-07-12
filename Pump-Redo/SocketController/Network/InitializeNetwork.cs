using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Pump.Database.Table;
using Pump.IrrigationController;

namespace Pump.SocketController.Network
{
    internal class InitializeNetwork
    {
        private readonly Dictionary<IrrigationConfiguration, ObservableIrrigation> _observableDict;
        public readonly NetworkManager NetworkManager;
        public readonly Stopwatch RequestIrrigationTimer;
        private bool _alreadySubscribed;
        public bool RequestNow;

        public InitializeNetwork(Dictionary<IrrigationConfiguration, ObservableIrrigation> observableDict)
        {
            _observableDict = observableDict;
            RequestIrrigationTimer = new Stopwatch();
            NetworkManager = new NetworkManager();
        }


        public async Task SubscribeNetwork()
        {
            if (_observableDict.Keys.Any(x => x.ConnectionType == 1) && _alreadySubscribed == false)
            {
                _alreadySubscribed = true;
                await ConnectToDevice();
            }
        }

        public void Disposable()
        {
            _alreadySubscribed = false;
        }


        private async Task ConnectToDevice()
        {
            RequestIrrigationTimer.Start();
            var oldIrrigationTuple =
                new Tuple<List<CustomSchedule>, List<Schedule>, List<Equipment>, List<ManualSchedule>, List<Sensor>,
                    List<SubController>>
                (new List<CustomSchedule>(), new List<Schedule>(), new List<Equipment>(),
                    new List<ManualSchedule>(), new List<Sensor>(), new List<SubController>());
            RequestNow = true;
            while (_alreadySubscribed)
            {
                while (CanRequestIrrigationData()) await Task.Delay(500);

                try
                {
                    /*
                    var irrigationJObject = JObject.Parse(await GetIrrigationData());

                    var irrigationTuple = IrrigationConvert.IrrigationJObjectToList(irrigationJObject);

                    var irrigationTupleEditState =
                        IrrigationConvert.CheckUpdatedStatus(irrigationTuple, oldIrrigationTuple);


                    IrrigationConvert.UpdateObservableIrrigation(_observableIrrigation, irrigationTupleEditState);
                    oldIrrigationTuple = irrigationTuple;
                    */
                }
                catch (Exception)
                {
                    _alreadySubscribed = false;
                    RequestIrrigationTimer.Stop();
                    OnConnectionLost();
                    break;
                }

                RequestIrrigationTimer.Restart();
            }
        }

        private bool CanRequestIrrigationData()
        {
            if (!RequestNow) return RequestIrrigationTimer.Elapsed <= TimeSpan.FromSeconds(15);

            RequestNow = false;
            RequestIrrigationTimer.Restart();
            return false;
        }

        private void OnConnectionLost()
        {
            /*
            _observableIrrigation.EquipmentList.Clear();
            _observableIrrigation.SensorList.Clear();
            _observableIrrigation.ManualScheduleList.Clear();
            _observableIrrigation.ScheduleList.Clear();
            _observableIrrigation.CustomScheduleList.Clear();
            _observableIrrigation.SubControllerList.Clear();
            _observableIrrigation.AliveList.Clear();
            */
        }

        /*
        private async Task<string> GetIrrigationData()
        {
            return await NetworkManager.SendAndReceiveToNetwork(SocketCommands.AllTogether(), _irrigationConfiguration);
        }
        */
    }
}