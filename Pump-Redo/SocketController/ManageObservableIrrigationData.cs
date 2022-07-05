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
        
        
        public static void AddFilteredUpdateOrRemove<T>(T record, ObservableFilteredIrrigation filteredObservableIrrigation, List<string> subControllerIds) where T : IEntity
        {
            var observableType = typeof(ObservableIrrigation);
            var filterObservableType = typeof(ObservableFilteredIrrigation);

            var propertyInfo = observableType.GetProperties().FirstOrDefault(x => x.PropertyType == typeof(ObservableCollection<T>));
            var filterPropertyInfo = filterObservableType.GetProperties().FirstOrDefault(x => x.PropertyType == typeof(ObservableCollection<T>));
            if(propertyInfo == null || filterPropertyInfo == null)
                return;
            
            var observableFilteredCollectionIrrigation = (ObservableCollection<T>) filterPropertyInfo.GetValue(filteredObservableIrrigation, null);
            var observableCollectionIrrigation = (ObservableCollection<T>) propertyInfo.GetValue(filteredObservableIrrigation.ObservableUnfilteredIrrigation, null);

            if (record is IEquipment)
            {
                EquipmentAddUpdateMissingRecord((dynamic)observableCollectionIrrigation, (dynamic) observableFilteredCollectionIrrigation, subControllerIds);
                EquipmentRemoveMissingRecord((dynamic) observableCollectionIrrigation, (dynamic) observableFilteredCollectionIrrigation, subControllerIds);    
            }

            if (record is ISchedule)
            {
                ScheduleAddUpdateMissingRecord((dynamic)observableCollectionIrrigation, (dynamic) observableFilteredCollectionIrrigation, subControllerIds);
                ScheduleRemoveMissingRecord((dynamic) observableCollectionIrrigation, (dynamic) observableFilteredCollectionIrrigation, subControllerIds);
            }
            
        }
        
        private static void EquipmentRemoveMissingRecord<T>(ObservableCollection<T> observableCollectionIrrigation, ObservableCollection<T> filterObservableCollectionIrrigation, List<string> subControllerIds)
            where T : IEntity, IEquipment
        {
            for (var i = 0; i < filterObservableCollectionIrrigation.Count; i++)
            {
                var existingRecord = filterObservableCollectionIrrigation.FirstOrDefault(x => x.Id == observableCollectionIrrigation[i].Id);
                if (existingRecord != null) continue;
                filterObservableCollectionIrrigation.Remove(filterObservableCollectionIrrigation[i]);
                i--;
            }
        }
        
        private static void EquipmentAddUpdateMissingRecord<T>(ObservableCollection<T> observableCollectionIrrigation, ObservableCollection<T> filterObservableCollectionIrrigation, List<string> subControllerIds)
            where T : IEntity, IEquipment
        {
            foreach (var entity in observableCollectionIrrigation)
            {
                var existingRecord = filterObservableCollectionIrrigation.FirstOrDefault(x => x.Id == entity.Id && x.AttachedSubController == entity.AttachedSubController);
                if(existingRecord == null)
                    filterObservableCollectionIrrigation.Add(entity);
                else
                {
                    if (JsonConvert.SerializeObject(existingRecord) == JsonConvert.SerializeObject(entity)) 
                        continue;
                    var index = filterObservableCollectionIrrigation.IndexOf(existingRecord);
                    filterObservableCollectionIrrigation[index] = entity;
                }    
            }
        }
        
        private static void ScheduleRemoveMissingRecord<T>(ObservableCollection<T> observableCollectionIrrigation, ObservableCollection<T> filterObservableCollectionIrrigation, List<string> subControllerIds)
            where T : IEntity, ISchedule
        {
            for (var i = 0; i < filterObservableCollectionIrrigation.Count; i++)
            {
                var existingRecord = filterObservableCollectionIrrigation.FirstOrDefault(x => x.Id == observableCollectionIrrigation[i].Id);
                if (existingRecord != null) continue;
                filterObservableCollectionIrrigation.Remove(filterObservableCollectionIrrigation[i]);
                i--;
            }
        }
        
        private static void ScheduleAddUpdateMissingRecord<T>(ObservableCollection<T> observableCollectionIrrigation, ObservableCollection<T> filterObservableCollectionIrrigation, List<string> subControllerIds)
            where T : IEntity, ISchedule
        {
            foreach (var entity in observableCollectionIrrigation)
            {
                var existingRecord = filterObservableCollectionIrrigation.FirstOrDefault(x => x.Id == entity.Id && x.ScheduleDetails.Any(x => ));
                if(existingRecord == null)
                    filterObservableCollectionIrrigation.Add(entity);
                else
                {
                    if (JsonConvert.SerializeObject(existingRecord) == JsonConvert.SerializeObject(entity)) 
                        continue;
                    var index = filterObservableCollectionIrrigation.IndexOf(existingRecord);
                    filterObservableCollectionIrrigation[index] = entity;
                }
            }
        }

    }
}