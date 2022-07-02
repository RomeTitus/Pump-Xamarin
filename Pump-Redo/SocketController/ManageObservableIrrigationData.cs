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
        public static void AddUpdateOrRemove<T>(T _, List<T> dynamicValueList, ObservableIrrigation observableIrrigation) where T : IEntity
        {
            var observableType = typeof(ObservableIrrigation);
            var propertyInfo = observableType.GetProperties().FirstOrDefault(x => x.PropertyType == typeof(ObservableCollection<T>));
            if(propertyInfo == null)
                return;
            var observableCollectionIrrigation = (ObservableCollection<T>) propertyInfo.GetValue(observableIrrigation, null);
            
            AddUpdateMissingRecord(dynamicValueList, observableCollectionIrrigation);
            
            RemoveMissingRecord(dynamicValueList, observableCollectionIrrigation);
            
            observableIrrigation.LoadedData = true;
        }

        private static void RemoveMissingRecord<T>(List<T> dynamicValueList, ObservableCollection<T> observableCollectionIrrigation)
            where T : IEntity
        {
            foreach (var existingEntity in observableCollectionIrrigation)
            {
                var existingRecord = dynamicValueList.FirstOrDefault(x => x.Id == existingEntity.Id);
                if(existingRecord == null)
                    observableCollectionIrrigation.Remove(existingEntity);
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
        
        public static (dynamic type, List<dynamic> dynamicList) GetDynamicValueListFromJObject(string className,  JObject jObject)
        {
            var dynamicList = new List<dynamic>();
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