using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pump.Database;
using Pump.Database.Table;
using Pump.IrrigationController;

namespace Pump.SocketController
{
    public static class ManageObservableIrrigationData
    {
        public static (dynamic type, List<IEntity> dynamicList) GetDynamicValueListFromJObject(string className,
            JObject jObject)
        {
            var dynamicList = new List<IEntity>();
            var type = Type.GetType("Pump.IrrigationController." + className);
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

        public static List<IrrigationConfiguration> GetConfigurationListFromJObject(JObject jObject)
        {
            var configList = new List<IrrigationConfiguration>();
            foreach (var keyValuePair in jObject)
            {
                var elementObject = keyValuePair.Value.ToString();
                var irrigationObject = JsonConvert.DeserializeObject<IrrigationConfiguration>(elementObject);
                configList.Add(irrigationObject);
            }

            return configList;
        }

        public static void AddUpdateOrRemoveConfigFromController(
            Dictionary<IrrigationConfiguration, ObservableIrrigation> observableDict,
            List<IrrigationConfiguration> newConfigLists, DatabaseController databaseController)
        {
            foreach (var config in newConfigLists)
            {
                var existingConfig = observableDict.Keys.FirstOrDefault(x => x.Path == config.Path);
                if (existingConfig == null)
                {
                    observableDict.Add(config, new ObservableIrrigation());
                }
                else
                {
                    if (JsonConvert.SerializeObject(config) == JsonConvert.SerializeObject(existingConfig))
                        continue;

                    existingConfig.Path = config.Path;
                    existingConfig.ConnectionType = config.ConnectionType;
                    existingConfig.ControllerPairs = config.ControllerPairs;
                    existingConfig.ExternalPath = config.ExternalPath;
                    existingConfig.InternalPath = config.InternalPath;
                    existingConfig.DeviceGuid = config.DeviceGuid;
                    existingConfig.LoRaNode = config.LoRaNode;
                    existingConfig.Address = config.Address;
                    databaseController.SaveIrrigationConfiguration(existingConfig);
                }
            }

            var deletedConfigPair =
                observableDict.Where(x => newConfigLists.Select(y => y.Path).Contains(x.Key.Path) == false);
            foreach (var deletedConfig in deletedConfigPair)
            {
                databaseController.DeleteIrrigationConfigurationConnection(deletedConfig.Key);
                observableDict.Remove(deletedConfig.Key);
            }
        }

        public static void AddUpdateOrRemoveRecordFromController<T>(T _, List<IEntity> dynamicValueList,
            ObservableIrrigation observableIrrigation) where T : IEntity
        {
            var observableType = typeof(ObservableIrrigation);
            var propertyInfo = observableType.GetProperties()
                .FirstOrDefault(x => x.PropertyType == typeof(ObservableCollection<T>));
            if (propertyInfo == null)
                return;
            var observableCollectionIrrigation =
                (ObservableCollection<T>)propertyInfo.GetValue(observableIrrigation, null);
            var incomingRecordList = dynamicValueList.ConvertAll(x => (T)x);

            AddUpdateRemoveMissingRecord(incomingRecordList, observableCollectionIrrigation);
        }

        private static void AddUpdateRemoveMissingRecord<T>(List<T> dynamicValueList,
            ObservableCollection<T> observableCollectionIrrigation)
            where T : IEntity
        {
            foreach (var entity in dynamicValueList)
            {
                var existingRecord = observableCollectionIrrigation.FirstOrDefault(x => x.Id == entity.Id);
                if (existingRecord == null)
                {
                    observableCollectionIrrigation.Add(entity);
                }
                else
                {
                    if (CompareTwoObjects(existingRecord, entity))
                        continue;
                    var index = observableCollectionIrrigation.IndexOf(existingRecord);
                    observableCollectionIrrigation[index] = entity;
                }
            }

            for (var i = 0; i < observableCollectionIrrigation.Count; i++)
            {
                var existingRecord = dynamicValueList.FirstOrDefault(x => x.Id == observableCollectionIrrigation[i].Id);
                if (existingRecord != null) continue;
                observableCollectionIrrigation.Remove(observableCollectionIrrigation[i]);
                i--;
            }
        }

        public static ObservableCollection<T> NewSiteAddFilteredUpdateOrRemove<T>(T record,
            ObservableFilteredIrrigation filteredObservableIrrigation) where T : IEntity
        {
            var observableType = typeof(ObservableIrrigation);
            var filterObservableType = typeof(ObservableFilteredIrrigation);

            var propertyInfo = observableType.GetProperties()
                .FirstOrDefault(x => x.PropertyType == typeof(ObservableCollection<T>));
            var filterPropertyInfo = filterObservableType.GetProperties()
                .FirstOrDefault(x => x.PropertyType == typeof(ObservableCollection<T>));
            if (propertyInfo == null || filterPropertyInfo == null)
                return null;

            var observableFilteredCollectionIrrigation =
                (ObservableCollection<T>)filterPropertyInfo.GetValue(filteredObservableIrrigation, null);
            var observableCollectionIrrigation =
                (ObservableCollection<T>)propertyInfo.GetValue(
                    filteredObservableIrrigation.ObservableUnfilteredIrrigation, null);

            if (record is IEquipment)
                NewSiteEquipmentAddUpdateMissingRecord((dynamic)observableCollectionIrrigation,
                    (dynamic)observableFilteredCollectionIrrigation, filteredObservableIrrigation.ControllerIdList);

            else if (record is ISchedule)
                NewSiteScheduleAddUpdateMissingRecord(filteredObservableIrrigation,
                    (dynamic)observableCollectionIrrigation, (dynamic)observableFilteredCollectionIrrigation,
                    filteredObservableIrrigation.ControllerIdList);

            else if (record is IManualSchedule)
                NewSiteManualAddUpdateMissingRecord(filteredObservableIrrigation,
                    (dynamic)observableCollectionIrrigation, (dynamic)observableFilteredCollectionIrrigation,
                    filteredObservableIrrigation.ControllerIdList);
            
            else if (record is ISubController)
                NewSiteSubControllerAddUpdateMissingRecord((dynamic)observableCollectionIrrigation, (dynamic)observableFilteredCollectionIrrigation,
                    filteredObservableIrrigation.ControllerIdList);

            return observableCollectionIrrigation;
        }

        private static void NewSiteEquipmentAddUpdateMissingRecord<T>(
            ObservableCollection<T> observableCollectionIrrigation,
            ObservableCollection<T> filterObservableCollectionIrrigation, List<string> subControllerIds)
            where T : IEntity, IEquipment
        {
            foreach (var record in observableCollectionIrrigation)
            {
                var existingRecord = filterObservableCollectionIrrigation.FirstOrDefault(x => x.Id == record.Id);
                if (existingRecord == null)
                {
                    if (subControllerIds.Contains(record.AttachedSubController))
                        filterObservableCollectionIrrigation.Add(record);
                }
                else
                {
                    if (CompareTwoObjects(existingRecord, record))
                        continue;
                    var index = filterObservableCollectionIrrigation.IndexOf(existingRecord);
                    filterObservableCollectionIrrigation[index] = record;
                }
            }
        }

        private static void NewSiteScheduleAddUpdateMissingRecord<T>(
            ObservableFilteredIrrigation filteredObservableIrrigation,
            ObservableCollection<T> observableCollectionIrrigation,
            ObservableCollection<T> filterObservableCollectionIrrigation, List<string> subControllerIds)
            where T : IEntity, ISchedule
        {
            var filteredEquipmentIds = filteredObservableIrrigation.ObservableUnfilteredIrrigation.EquipmentList
                .Where(x => subControllerIds.Contains(x.AttachedSubController)).Select(x => x.Id).ToList().ToList();

            foreach (var record in observableCollectionIrrigation)
            {
                var existingRecord = filterObservableCollectionIrrigation.FirstOrDefault(x => x.Id == record.Id);
                if (existingRecord == null)
                {
                    var scheduleEquipmentIds = record.ScheduleDetails.Select(x => x.id_Equipment);
                    var matchIds =
                        scheduleEquipmentIds.Intersect(filteredEquipmentIds, StringComparer.OrdinalIgnoreCase);
                    if (matchIds.Any())
                        filterObservableCollectionIrrigation.Add(record);
                }
                else
                {
                    if (CompareTwoObjects(existingRecord, record))
                        continue;
                    var index = filterObservableCollectionIrrigation.IndexOf(existingRecord);
                    filterObservableCollectionIrrigation[index] = record;
                }
            }
        }

        private static void NewSiteManualAddUpdateMissingRecord<T>(
            ObservableFilteredIrrigation filteredObservableIrrigation,
            ObservableCollection<T> observableCollectionIrrigation,
            ObservableCollection<T> filterObservableCollectionIrrigation, List<string> subControllerIds)
            where T : IEntity, IManualSchedule
        {
            var filteredEquipmentIds = filteredObservableIrrigation.ObservableUnfilteredIrrigation.EquipmentList
                .Where(x => subControllerIds.Contains(x.AttachedSubController)).Select(x => x.Id).ToList().ToList();

            foreach (var record in observableCollectionIrrigation)
            {
                var existingRecord = filterObservableCollectionIrrigation.FirstOrDefault(x => x.Id == record.Id);
                if (existingRecord == null)
                {
                    var manualEquipmentIds = record.ManualDetails.Select(x => x.id_Equipment);
                    var matchIds = manualEquipmentIds.Intersect(filteredEquipmentIds, StringComparer.OrdinalIgnoreCase);
                    if (matchIds.Any())
                        filterObservableCollectionIrrigation.Add(record);
                }
                else
                {
                    if (CompareTwoObjects(existingRecord, record))
                        continue;
                    var index = filterObservableCollectionIrrigation.IndexOf(existingRecord);
                    filterObservableCollectionIrrigation[index] = record;
                }
            }
        }
        
        private static void NewSiteSubControllerAddUpdateMissingRecord<T>(
            ObservableCollection<T> observableCollectionIrrigation,
            ObservableCollection<T> filterObservableCollectionIrrigation, List<string> subControllerIds)
            where T : IEntity, ISubController
        {
            foreach (var record in observableCollectionIrrigation.Where(x => subControllerIds.Contains(x.Id)))
            {
                var existingRecord = filterObservableCollectionIrrigation.FirstOrDefault(x => x.Id == record.Id);
                
                if (existingRecord == null)
                {
                    filterObservableCollectionIrrigation.Add(record);
                }
                else
                {
                    if (CompareTwoObjects(existingRecord, record))
                        continue;
                    var index = filterObservableCollectionIrrigation.IndexOf(existingRecord);
                    filterObservableCollectionIrrigation[index] = record;
                }
            }
        }
        

        public static void FilteredAddUpdate<T>(T record, ObservableFilteredIrrigation filteredObservableIrrigation)
            where T : IEntity
        {
            var filterObservableType = typeof(ObservableFilteredIrrigation);
            var filterPropertyInfo = filterObservableType.GetProperties()
                .FirstOrDefault(x => x.PropertyType == typeof(ObservableCollection<T>));
            if (filterPropertyInfo == null)
                return;

            var observableFilteredCollectionIrrigation =
                (ObservableCollection<T>)filterPropertyInfo.GetValue(filteredObservableIrrigation, null);

            if (record is IEquipment)
                SiteEquipmentAddUpdate(record, (dynamic)observableFilteredCollectionIrrigation,
                    filteredObservableIrrigation.ControllerIdList);

            else if (record is ISchedule)
                SiteScheduleAddUpdate(record, filteredObservableIrrigation,
                    (dynamic)observableFilteredCollectionIrrigation);

            else if (record is IManualSchedule)
                SiteManualAddUpdate(record, filteredObservableIrrigation,
                    (dynamic)observableFilteredCollectionIrrigation);
            
            else if (record is ISubController)
                SiteSubControllerAddUpdate(record, (dynamic)observableFilteredCollectionIrrigation,
                    filteredObservableIrrigation.ControllerIdList);
        }

        public static void FilteredRemove<T>(T record, ObservableFilteredIrrigation filteredObservableIrrigation)
            where T : IEntity
        {
            var filterObservableType = typeof(ObservableFilteredIrrigation);
            var filterPropertyInfo = filterObservableType.GetProperties()
                .FirstOrDefault(x => x.PropertyType == typeof(ObservableCollection<T>));
            if (filterPropertyInfo == null)
                return;

            var observableFilteredCollectionIrrigation =
                (ObservableCollection<T>)filterPropertyInfo.GetValue(filteredObservableIrrigation, null);

            var existingRecord = observableFilteredCollectionIrrigation.FirstOrDefault(x => x.Id == record.Id);
            if (existingRecord != null)
                observableFilteredCollectionIrrigation.Remove(existingRecord);
        }

        private static void SiteEquipmentAddUpdate<T>(T record,
            ObservableCollection<T> filterObservableCollectionIrrigation, List<string> subControllerIds)
            where T : IEntity, IEquipment
        {
            if (!subControllerIds.Contains(record.AttachedSubController))
                return;

            var existingRecord = filterObservableCollectionIrrigation.FirstOrDefault(x => x.Id == record.Id);
            if (existingRecord == null)
            {
                filterObservableCollectionIrrigation.Add(record);
            }
            else
            {
                if (CompareTwoObjects(existingRecord, record))
                    return;
                var index = filterObservableCollectionIrrigation.IndexOf(existingRecord);
                filterObservableCollectionIrrigation[index] = record;
            }
        }

        private static void SiteScheduleAddUpdate<T>(T record,
            ObservableFilteredIrrigation filteredObservableIrrigation,
            ObservableCollection<T> filterObservableCollectionIrrigation)
            where T : IEntity, ISchedule
        {
            var filteredEquipmentIds = filteredObservableIrrigation.ObservableUnfilteredIrrigation.EquipmentList
                .Where(x => filteredObservableIrrigation.ControllerIdList.Contains(x.AttachedSubController))
                .Select(x => x.Id).ToList().ToList();

            var existingRecord = filterObservableCollectionIrrigation.FirstOrDefault(x => x.Id == record.Id);
            if (existingRecord == null)
            {
                var scheduleEquipmentIds = record.ScheduleDetails.Select(x => x.id_Equipment);
                var matchIds = scheduleEquipmentIds.Intersect(filteredEquipmentIds, StringComparer.OrdinalIgnoreCase);
                if (matchIds.Any())
                    filterObservableCollectionIrrigation.Add(record);
            }
            else
            {
                if (CompareTwoObjects(existingRecord, record))
                    return;
                var index = filterObservableCollectionIrrigation.IndexOf(existingRecord);
                filterObservableCollectionIrrigation[index] = record;
            }
        }

        private static void SiteManualAddUpdate<T>(T record, ObservableFilteredIrrigation filteredObservableIrrigation,
            ObservableCollection<T> filterObservableCollectionIrrigation)
            where T : IEntity, IManualSchedule
        {
            var filteredEquipmentIds = filteredObservableIrrigation.ObservableUnfilteredIrrigation.EquipmentList
                .Where(x => filteredObservableIrrigation.ControllerIdList.Contains(x.AttachedSubController))
                .Select(x => x.Id).ToList().ToList();

            var existingRecord = filterObservableCollectionIrrigation.FirstOrDefault(x => x.Id == record.Id);
            if (existingRecord == null)
            {
                var manualEquipmentIds = record.ManualDetails?.Select(x => x.id_Equipment);
                var matchIds = manualEquipmentIds?.Intersect(filteredEquipmentIds, StringComparer.OrdinalIgnoreCase);
                if (matchIds != null && matchIds.Any())
                    filterObservableCollectionIrrigation.Add(record);
            }
            else
            {
                if (CompareTwoObjects(existingRecord, record))
                    return;
                var index = filterObservableCollectionIrrigation.IndexOf(existingRecord);
                filterObservableCollectionIrrigation[index] = record;
            }
        }
        
        private static void SiteSubControllerAddUpdate<T>(T record,
            ObservableCollection<T> filterObservableCollectionIrrigation, List<string> subControllerIds)
            where T : IEntity, ISubController
        {
            if (!subControllerIds.Contains(record.Id))
                return;

            var existingRecord = filterObservableCollectionIrrigation.FirstOrDefault(x => x.Id == record.Id);
            if (existingRecord == null)
            {
                filterObservableCollectionIrrigation.Add(record);
            }
            else
            {
                if (CompareTwoObjects(existingRecord, record))
                    return;
                var index = filterObservableCollectionIrrigation.IndexOf(existingRecord);
                filterObservableCollectionIrrigation[index] = record;
            }
        }
        
        
        private static bool CompareTwoObjects<T>(T existingRecord,
            T incomingEntity)
            where T : IEntity
        {
            if(existingRecord.GetType() == incomingEntity.GetType() && existingRecord is IStatus existingStatus)
                if (existingStatus.HasUpdated != ((IStatus)incomingEntity).HasUpdated)
                    return false;
            
            return JsonConvert.SerializeObject(existingRecord) == JsonConvert.SerializeObject(incomingEntity);
        }
    }
}