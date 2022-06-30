﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pump.Database;
using Pump.IrrigationController;

namespace Pump.SocketController.Network
{
    internal class InitializeNetwork
    {
        private readonly ObservableIrrigation _observableIrrigation;
        public readonly NetworkManager NetworkManager;
        private readonly DatabaseController _database;
        public readonly Stopwatch RequestIrrigationTimer;
        private bool _isSubscribed;
        public bool RequestNow;

        public InitializeNetwork(ObservableIrrigation observableIrrigation)
        {
            _observableIrrigation = observableIrrigation;
            RequestIrrigationTimer = new Stopwatch();
            _database = new DatabaseController();
            NetworkManager = new NetworkManager();
        }


        public async Task SubscribeNetwork()
        {
            _isSubscribed = true;
            await ConnectToDevice();
        }

        public void Disposable()
        {
            _isSubscribed = false;
        }


        private async Task ConnectToDevice()
        {
            RequestIrrigationTimer.Start();
            var oldIrrigationTuple =
                new Tuple<List<CustomSchedule>, List<Schedule>, List<Equipment>, List<ManualSchedule>, List<Sensor>, List<SubController>>
                (new List<CustomSchedule>(), new List<Schedule>(), new List<Equipment>(),
                    new List<ManualSchedule>(), new List<Sensor>(), new List<SubController>());
            RequestNow = true;
            while (_isSubscribed)
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
                    _isSubscribed = false;
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
            _observableIrrigation.EquipmentList.Clear();
            _observableIrrigation.SensorList.Clear();
            _observableIrrigation.ManualScheduleList.Clear();
            _observableIrrigation.ScheduleList.Clear();
            _observableIrrigation.CustomScheduleList.Clear();
            _observableIrrigation.SubControllerList.Clear();
            _observableIrrigation.AliveList.Clear();

            _observableIrrigation.EquipmentList.Add(null);
            _observableIrrigation.SensorList.Add(null);
            _observableIrrigation.ManualScheduleList.Add(null);
            _observableIrrigation.ScheduleList.Add(null);
            _observableIrrigation.CustomScheduleList.Add(null);
            _observableIrrigation.SubControllerList.Add(null);
            _observableIrrigation.AliveList.Add(null);
        }

        /*
        private async Task<string> GetIrrigationData()
        {
            return await NetworkManager.SendAndReceiveToNetwork(SocketCommands.AllTogether(), _irrigationConfiguration);
        }
        */
    }
}