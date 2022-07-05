using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database.Streaming;
using Newtonsoft.Json.Linq;
using Pump.Database.Table;
using Pump.IrrigationController;

namespace Pump.SocketController.Firebase
{
    internal class InitializeFirebase
    {
        private readonly Dictionary<IrrigationConfiguration, ObservableIrrigation> _observableDict;
        private IDisposable _subscribeFirebase;
        private readonly FirebaseManager _firebaseManager;
        private bool _alreadySubscribed;
        
        public InitializeFirebase(FirebaseManager firebaseManager, Dictionary<IrrigationConfiguration, ObservableIrrigation> observableDict)
        {
            _observableDict = observableDict;
            _firebaseManager = firebaseManager;
        }

        public void SubscribeFirebase()
        {
            
            if (_observableDict.Keys.Any(x => x.ConnectionType == 1) && _alreadySubscribed == false)
            {
                _alreadySubscribed = true;
                _subscribeFirebase.Dispose();
            }
            
            _subscribeFirebase = _firebaseManager.FirebaseQuery
                .AsObservable<JObject>().Subscribe(OnNext);
        }

        private void OnNext(FirebaseEvent<JObject> obj)
        {
            try
            {
                if(obj.Key == "Config")
                    return;

                var configuration = _observableDict.Keys.FirstOrDefault(y => y.Path == obj.Key);
                if (configuration == null)
                    throw new Exception("Configuration does not exist for :" + obj.Key);
                        
                foreach (var elementPair in obj.Object)
                {
                    if(!elementPair.Value.Any())
                        continue;
                            
                    var typeAndDynamicValueList = ManageObservableIrrigationData.GetDynamicValueListFromJObject(elementPair.Key, JObject.Parse(elementPair.Value.ToString()));
                                
                    if(typeAndDynamicValueList.type == null)
                        continue;
                                
                    ManageObservableIrrigationData.AddUpdateOrRemove(typeAndDynamicValueList.type, typeAndDynamicValueList.dynamicList, _observableDict[configuration]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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