using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pump.IrrigationController;
using Type = System.Type;

namespace Pump.SocketController
{
    public static class ManageObservableIrrigationData
    {
        public static void AddUpdateOrRemove<T>(T _, List<IEntity> dynamicValueList, ObservableIrrigation observableIrrigation) where T : IEntity
        {
            var observableType = typeof(ObservableIrrigation);
            var propertyInfo = observableType.GetProperties().FirstOrDefault(x => x.PropertyType == typeof(ObservableCollection<T>));
            if(propertyInfo == null)
                return;
            var observableCollectionIrrigation = (ObservableCollection<T>) propertyInfo.GetValue(observableIrrigation, null);
            var incomingRecordList = dynamicValueList.ConvertAll(x => (T) x);
            
            AddUpdateMissingRecord(incomingRecordList, observableCollectionIrrigation);
            
            RemoveMissingRecord(incomingRecordList, observableCollectionIrrigation);
            
            observableIrrigation.LoadedData = true;
        }

        private static void RemoveMissingRecord<T>(List<T> dynamicValueList, ObservableCollection<T> observableCollectionIrrigation)
            where T : IEntity
        {
            for (var i = 0; i < observableCollectionIrrigation.Count; i++)
            {
                var existingRecord = dynamicValueList.FirstOrDefault(x => x.Id == observableCollectionIrrigation[i].Id);
                if (existingRecord != null) continue;
                observableCollectionIrrigation.Remove(observableCollectionIrrigation[i]);
                i--;
            }
        }
        
        private static void AddUpdateMissingRecord<T>(List<T> dynamicValueList, ObservableCollection<T> observableCollectionIrrigation)
            where T : IEntity
        {
            foreach (var entity in dynamicValueList)
            {
                var existingRecord = observableCollectionIrrigation.FirstOrDefault(x => x.Id == entity.Id);
                if(existingRecord == null)
                    observableCollectionIrrigation.Add(entity);
                else
                {
                    if (JsonConvert.SerializeObject(existingRecord) == JsonConvert.SerializeObject(entity)) 
                        continue;
                    var index = observableCollectionIrrigation.IndexOf(existingRecord);
                    observableCollectionIrrigation[index] = entity;
                }    
            }
        }

        public static dynamic GetDynamicValueFromObject(string className,  KeyValuePair<string,JToken> keyValuePair)
        {
            var elementObject = keyValuePair.Value.ToString();
            var type = Type.GetType("Pump.IrrigationController."+ className);
            if (type == null) 
                return null;
            var irrigationObject = JsonConvert.DeserializeObject(elementObject, type);
            if (!(irrigationObject is IEntity entity)) return null;
            entity.Id = keyValuePair.Key;
            return irrigationObject;
        }
        
        public static (dynamic type, List<IEntity> dynamicList) GetDynamicValueListFromJObject(string className,  JObject jObject)
        {
            var dynamicList = new List<IEntity>();
            var type = Type.GetType("Pump.IrrigationController."+ className);
            if (type == null) 
                return (null, dynamicList);
            
            foreach (var keyValuePair in jObject)
            {
                var elementObject = keyValuePair.Value.ToString();
                var irrigationObject = JsonConvert.DeserializeObject(elementObject, type);
                if (!(irrigationObject is IEntity entity)) return (null, dynamicList);
                entity.Id = keyValuePair.Key;
                dynamicList.Add(entity);
            }
            var instance = Activator.CreateInstance(type);
            return (instance, dynamicList);
        }
    }
}