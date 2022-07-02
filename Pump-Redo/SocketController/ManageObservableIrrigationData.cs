using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Pump.Database.Table;
using Pump.IrrigationController;

namespace Pump.SocketController
{
    public class ManageObservableIrrigationData
    {
        private readonly Dictionary<IrrigationConfiguration, ObservableIrrigation> _observableDict;
        public ManageObservableIrrigationData(Dictionary<IrrigationConfiguration, ObservableIrrigation> observableDict)
        {
            _observableDict = observableDict;
        }
        public void AddOrUpdateToList<T>(T dynamicValue, IrrigationConfiguration configuration) where T : IEntity
        {
            var observable =  _observableDict[configuration];
            
            var observableType = typeof(ObservableIrrigation);
            var propertyInfo = observableType.GetProperties().FirstOrDefault(x => x.PropertyType == typeof(ObservableCollection<T>));
            if(propertyInfo == null)
                return;
            var observableCollection = (ObservableCollection<T>) propertyInfo.GetValue(observable, null);
            
            if(observableCollection.Count == 1 && observableCollection[0] == null)
                observableCollection.Clear();

            var existingRecord = observableCollection.FirstOrDefault(x => x.Id == dynamicValue.Id);
            if(existingRecord == null)
                observableCollection.Add(dynamicValue);
            else
            {
                CopyValues(existingRecord, dynamicValue);
                var index = observableCollection.IndexOf(existingRecord);
                observableCollection[index] = existingRecord;
            }

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