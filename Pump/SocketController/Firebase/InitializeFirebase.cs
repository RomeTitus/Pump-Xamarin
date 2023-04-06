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
                var entitiesList = new List<string> { nameof(CustomSchedule), nameof(Equipment), nameof(ManualSchedule), nameof(Schedule), nameof(Sensor), nameof(SubController)};
                var configuration = _observableDict.Keys.FirstOrDefault(y => y.Path == obj.Key);
                
                if (obj.Key == "Config")
                {
                    UpdateConfiguration(_observableDict, obj.Object);
                    return;
                }

                if (configuration == null)
                    throw new Exception("Configuration does not exist for :" + obj.Key);

                
                if (obj.Object.ContainsKey("Equipment"))
                {
                   
                    var jProperty = obj.Object.Property("Equipment");
                    FirebaseToObservable(new KeyValuePair<string, JToken>(jProperty.Name, jProperty.Value),
                        configuration);
                    obj.Object.Remove("Equipment");
                    entitiesList.Remove(jProperty.Name);
                }

                foreach (var elementPair in obj.Object)
                {
                    FirebaseToObservable(elementPair, configuration);
                    entitiesList.Remove(elementPair.Key);
                }

                foreach (var entity in entitiesList)
                {
                    FirebaseToObservable(new KeyValuePair<string, JToken>(entity, "{}"),
                        configuration);
                }
                
                _observableDict[configuration].LoadedData = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void FirebaseToObservable(KeyValuePair<string,JToken> elementPair,  IrrigationConfiguration configuration)
        {
            if (elementPair.Value.ToString() != "{}" && !elementPair.Value.Any())
                return;

            var typeAndDynamicValueList =
                ManageObservableIrrigationData.GetDynamicValueListFromJObject(elementPair.Key,
                    JObject.Parse(elementPair.Value.ToString()));

            if (typeAndDynamicValueList.type == null)
                return;

            ManageObservableIrrigationData.AddUpdateOrRemoveRecordFromController(typeAndDynamicValueList.type,
                typeAndDynamicValueList.dynamicList, _observableDict[configuration]);

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