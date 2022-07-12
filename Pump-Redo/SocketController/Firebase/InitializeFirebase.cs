using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database.Streaming;
using Newtonsoft.Json.Linq;
using Pump.Database;
using Pump.Database.Table;
using Pump.IrrigationController;

namespace Pump.SocketController.Firebase
{
    internal class InitializeFirebase
    {
        private readonly DatabaseController _databaseController;
        private readonly FirebaseManager _firebaseManager;
        private readonly Dictionary<IrrigationConfiguration, ObservableIrrigation> _observableDict;
        private bool _alreadySubscribed;
        private IDisposable _subscribeFirebase;

        public InitializeFirebase(FirebaseManager firebaseManager,
            Dictionary<IrrigationConfiguration, ObservableIrrigation> observableDict)
        {
            _observableDict = observableDict;
            _firebaseManager = firebaseManager;
            _databaseController = new DatabaseController();
        }

        public void SubscribeFirebase()
        {
            if (_observableDict.Keys.Any(x => x.ConnectionType == 0) && _alreadySubscribed == false)
            {
                _alreadySubscribed = true;
                _subscribeFirebase = _firebaseManager.FirebaseQuery
                    .AsObservable<JObject>().Subscribe(OnNext);
            }
        }

        private void OnNext(FirebaseEvent<JObject> obj)
        {
            try
            {
                if (obj.Key == "Config")
                {
                    UpdateConfiguration(_observableDict, obj.Object);
                    return;
                }

                var configuration = _observableDict.Keys.FirstOrDefault(y => y.Path == obj.Key);
                if (configuration == null)
                    throw new Exception("Configuration does not exist for :" + obj.Key);

                foreach (var elementPair in obj.Object)
                {
                    if (!elementPair.Value.Any())
                        continue;

                    var typeAndDynamicValueList =
                        ManageObservableIrrigationData.GetDynamicValueListFromJObject(elementPair.Key,
                            JObject.Parse(elementPair.Value.ToString()));

                    if (typeAndDynamicValueList.type == null)
                        continue;

                    ManageObservableIrrigationData.AddUpdateOrRemoveRecordFromController(typeAndDynamicValueList.type,
                        typeAndDynamicValueList.dynamicList, _observableDict[configuration]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void UpdateConfiguration(Dictionary<IrrigationConfiguration, ObservableIrrigation> observableDict,
            JObject configObject)
        {
            foreach (var elementPair in configObject)
            {
                if (!elementPair.Value.Any())
                    continue;
                var configLists = ManageObservableIrrigationData.GetConfigurationListFromJObject(configObject);
                ManageObservableIrrigationData.AddUpdateOrRemoveConfigFromController(observableDict, configLists,
                    _databaseController);
            }
        }

        public void Disposable(IrrigationConfiguration irrigationConfiguration)
        {
            if (_observableDict.Keys.Any(x => x.ConnectionType == 1) == false && _alreadySubscribed)
            {
                _alreadySubscribed = false;
                _subscribeFirebase.Dispose();
            }

            _observableDict[irrigationConfiguration].SensorList.Clear();
            _observableDict[irrigationConfiguration].EquipmentList.Clear();
            _observableDict[irrigationConfiguration].ManualScheduleList.Clear();
            _observableDict[irrigationConfiguration].ScheduleList.Clear();
            _observableDict[irrigationConfiguration].CustomScheduleList.Clear();
            _observableDict[irrigationConfiguration].SubControllerList.Clear();
            _observableDict[irrigationConfiguration].AliveList.Clear();
        }
    }
}