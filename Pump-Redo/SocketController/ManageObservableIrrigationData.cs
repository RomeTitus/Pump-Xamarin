using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pump.IrrigationController;

namespace Pump.SocketController
{
    public static class ManageObservableIrrigationData
    {
        public static void AddOrUpdateToList<T>(T dynamicValue, ObservableIrrigation observableIrrigation) where T : IEntity
        {
            var observableType = typeof(ObservableIrrigation);
            var propertyInfo = observableType.GetProperties().FirstOrDefault(x => x.PropertyType == typeof(ObservableCollection<T>));
            if(propertyInfo == null)
                return;
            var observableCollectionType = (ObservableCollection<T>) propertyInfo.GetValue(observableIrrigation, null);
            
            if(observableCollectionType.Count == 1 && observableCollectionType[0] == null)
                observableCollectionType.Clear();

            var existingRecord = observableCollectionType.FirstOrDefault(x => x.Id == dynamicValue.Id);
            if(existingRecord == null)
                observableCollectionType.Add(dynamicValue);
            else
            {
                CopyValues(existingRecord, dynamicValue);
                var index = observableCollectionType.IndexOf(existingRecord);
                observableCollectionType[index] = existingRecord;
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
        
        private static void CopyValues<T>(T target, T source)
        {
            var t = typeof(T);
            var properties = t.GetProperties().Where(prop => prop.CanRead && prop.CanWrite);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(source, null);
                if (value != null)
                    prop.SetValue(target, value, null);
            }
        }
        
    }
}