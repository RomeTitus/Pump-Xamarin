using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pump.Database;
using Pump.Database.Table;
using Pump.IrrigationController;

namespace Pump.SocketController.BT
{
    internal class InitializeBlueTooth
    {
        private readonly Dictionary<IrrigationConfiguration, ObservableIrrigation> _observableDict;
        public readonly BluetoothManager BlueToothManager;
        public readonly Stopwatch RequestIrrigationTimer;
        private bool _isSubscribed;
        public bool RequestNow;

        public InitializeBlueTooth(Dictionary<IrrigationConfiguration, ObservableIrrigation> observableDict)
        {
            _observableDict = observableDict;
            RequestIrrigationTimer = new Stopwatch();
            BlueToothManager = new BluetoothManager();
        }

        public async Task SubscribeBle()
        {
            /*
            while (true)
            {
                _isSubscribed = true;
                await BlueToothManager.StartScanning();
                    do
                    {
                        var iDevice =
                            BlueToothManager.IrrigationDeviceBt.FirstOrDefault(x =>
                                x.NativeDevice.ToString() == _irrigationConfiguration.Mac);
                        if (iDevice == null)
                            await Task.Delay(500);
                        else
                            await ConnectToDevice(iDevice.Id);
                    } while (BlueToothManager.AdapterBle.IsScanning);

                    if (_isSubscribed) continue;
                    break;
            }
            */
        }

        public void Disposable()
        {
            _isSubscribed = false;
        }

        private async Task ConnectToDevice(Guid deviceId)
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
                    if (BlueToothManager.BleDevice == null)
                    {
                        //await BlueToothManager.ConnectToKnownDevice(deviceId);
                        RequestIrrigationTimer.Restart();
                        continue;
                    }

                    var irrigationJObject = JObject.Parse(await GetIrrigationData());

                    var irrigationTuple = IrrigationConvert.IrrigationJObjectToList(irrigationJObject);

                    var irrigationTupleEditState =
                        IrrigationConvert.CheckUpdatedStatus(irrigationTuple, oldIrrigationTuple);


                    //IrrigationConvert.UpdateObservableIrrigation(_observableIrrigation, irrigationTupleEditState);
                    oldIrrigationTuple = irrigationTuple;
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

        private async Task<string> GetIrrigationData()
        {
            return await BlueToothManager.SendAndReceiveToBleAsync(SocketCommands.AllTogether());
        }

        private bool CanRequestIrrigationData()
        {
            if (!RequestNow) return RequestIrrigationTimer.Elapsed <= TimeSpan.FromSeconds(15);

            RequestNow = false;
            RequestIrrigationTimer.Restart();
            return false;
        }
    }
}