using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using Pump.SocketController;

namespace Pump.IrrigationController
{
    public class ObservableFilteredIrrigation
    {
        public readonly ObservableIrrigation ObservableUnfilteredIrrigation;
        public ObservableCollection<CustomSchedule> CustomScheduleList { get; }
        public ObservableCollection<Equipment> EquipmentList { get; }
        public ObservableCollection<ManualSchedule> ManualScheduleList { get; }
        public ObservableCollection<Schedule> ScheduleList { get; }
        public ObservableCollection<Sensor> SensorList { get; }
        public ObservableCollection<SubController> SubControllerList { get; }
        
        public List<string> ControllerIdList { get; }


        public ObservableFilteredIrrigation(ObservableIrrigation observableUnfilteredIrrigation, List<string> controllerIdList)
        {
            ObservableUnfilteredIrrigation = observableUnfilteredIrrigation;
            CustomScheduleList = new ObservableCollection<CustomSchedule>();
            EquipmentList = new ObservableCollection<Equipment> ();
            ManualScheduleList = new ObservableCollection<ManualSchedule>();
            ScheduleList = new ObservableCollection<Schedule>();
            SensorList = new ObservableCollection<Sensor>();
            SubControllerList = new ObservableCollection<SubController>();
            ControllerIdList = controllerIdList;
            Subscribe();

            /*
            foreach (var equipment in observableUnfilteredIrrigation.EquipmentList.Where(x => controllerIdList.Contains(x?.Id)))
                EquipmentList.Add(equipment);
            observableUnfilteredIrrigation.EquipmentList.CollectionChanged += CollectionChanged;

            foreach (var sensor in observableUnfilteredIrrigation.SensorList.Where(x => controllerIdList.Contains(x?.Id)))
                SensorList.Add(sensor);
            observableUnfilteredIrrigation.SensorList.CollectionChanged += CollectionChanged;

            foreach (var manualSchedule in observableUnfilteredIrrigation.ManualScheduleList.Where(x =>
                         x.ManualDetails.Any(y => controllerIdList.Contains(y.id_Equipment))))
                ManualScheduleList.Add(manualSchedule);
            observableUnfilteredIrrigation.ManualScheduleList.CollectionChanged += CollectionChanged;


            foreach (var schedule in observableUnfilteredIrrigation.ScheduleList.Where(x =>
                         x.ScheduleDetails.Any(y => controllerIdList.Contains(y.id_Equipment)) || controllerIdList.Contains(x.id_Pump)))
                ScheduleList.Add(schedule);

            observableUnfilteredIrrigation.ScheduleList.CollectionChanged += CollectionChanged;


            foreach (var customSchedule in observableUnfilteredIrrigation.CustomScheduleList.Where(x =>
                         x.ScheduleDetails.Any(y => controllerIdList.Contains(y.id_Equipment)) ||
                         controllerIdList.Contains(x.id_Pump)))
                CustomScheduleList.Add(customSchedule);
            observableUnfilteredIrrigation.CustomScheduleList.CollectionChanged += CollectionChanged;
        */
        }

        private void Subscribe()
        {
            var propertyFilteredObservableInfo = typeof(ObservableFilteredIrrigation).GetProperties();
            
            foreach (var filteredPropertyInfo in propertyFilteredObservableInfo)
            {
                var name = filteredPropertyInfo.Name;
                if(name == nameof(ObservableUnfilteredIrrigation))
                    continue;
                var type = Type.GetType("Pump.IrrigationController."+ name.Replace("List", ""));
                if(type == null)
                    continue;
                dynamic instance = Activator.CreateInstance(type);
                var observableCollection = ManageObservableIrrigationData.NewSiteAddFilteredUpdateOrRemove(instance, this);
                if(observableCollection is INotifyCollectionChanged notifyCollectionChanged)
                    notifyCollectionChanged.CollectionChanged += DynamicCollectionChanged;

            }
        }
        
        private void DynamicCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (dynamic item in e.NewItems)
                {
                    ManageObservableIrrigationData.FilteredAddUpdate(item, this);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (dynamic item in e.OldItems)
                {
                    ManageObservableIrrigationData.FilteredRemove(item, this);
                }
            }
        }

        /*
        private void CustomScheduleListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    CustomScheduleList.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (CustomSchedule newItem in e.NewItems)
                        if (newItem != null && (controllerIdList.Contains(newItem.id_Pump) ||
                                                newItem.ScheduleDetails.Any(x =>
                                                    controllerIdList.Contains(x.id_Equipment))))
                            CustomScheduleList.Add(newItem);

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (CustomSchedule newItem in e.NewItems)
                    {
                        if (!controllerIdList.Contains(newItem.id_Pump) ||
                            !newItem.ScheduleDetails.Any(x => controllerIdList.Contains(x.id_Equipment))) continue;

                        var replaceElement = CustomScheduleList.FirstOrDefault(x => x?.Id == newItem.Id);
                        if (replaceElement != null)
                            CustomScheduleList[CustomScheduleList.IndexOf(replaceElement)] = newItem;
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (CustomSchedule oldItem in e.OldItems) CustomScheduleList.Remove(oldItem);

                    break;
                }
            }
        }

        private void ScheduleListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    ScheduleList.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (Schedule newItem in e.NewItems)
                        if (newItem != null && (controllerIdList.Contains(newItem.id_Pump) ||
                                                newItem.ScheduleDetails.Any(x =>
                                                    controllerIdList.Contains(x.id_Equipment))))
                            ScheduleList.Add(newItem);

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (Schedule newItem in e.NewItems)
                    {
                        if (!controllerIdList.Contains(newItem.id_Pump) ||
                            !newItem.ScheduleDetails.Any(x => controllerIdList.Contains(x.id_Equipment))) continue;

                        var replaceElement = ScheduleList.FirstOrDefault(x => x?.Id == newItem.Id);
                        if (replaceElement != null)
                            ScheduleList[ScheduleList.IndexOf(replaceElement)] = newItem;
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (Schedule oldItem in e.OldItems) ScheduleList.Remove(oldItem);

                    break;
                }
            }
        }


        private void ManualScheduleListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    ManualScheduleList.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (ManualSchedule newItem in e.NewItems)
                        if (newItem != null &&
                            newItem.ManualDetails.Any(x => controllerIdList.Contains(x.id_Equipment)))
                            ManualScheduleList.Add(newItem);

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (ManualSchedule newItem in e.NewItems)
                    {
                        if (!newItem.ManualDetails.Any(x => controllerIdList.Contains(x.id_Equipment))) continue;
                        var replaceElement = ManualScheduleList.FirstOrDefault(x => x?.Id == newItem.Id);
                        if (replaceElement != null)
                            ManualScheduleList[ManualScheduleList.IndexOf(replaceElement)] = newItem;
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (ManualSchedule oldItem in e.OldItems) ManualScheduleList.Remove(oldItem);

                    break;
                }
            }
        }

        private void SensorListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    SensorList.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (Sensor newItem in e.NewItems)
                        if (newItem != null && controllerIdList.Contains(newItem.Id))
                            SensorList.Add(newItem);

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (Sensor newItem in e.NewItems)
                    {
                        if (!controllerIdList.Contains(newItem.Id)) continue;
                        var replaceElement = SensorList.FirstOrDefault(x => x?.Id == newItem.Id);
                        if (replaceElement != null)
                            SensorList[SensorList.IndexOf(replaceElement)] = newItem;
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (Sensor oldItem in e.OldItems) SensorList.Remove(oldItem);

                    break;
                }
            }
        }

        private void EquipmentListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    EquipmentList.Clear();
                    break;
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (Equipment newItem in e.NewItems)
                        if (newItem != null && controllerIdList.Contains(newItem.Id))
                            EquipmentList.Add(newItem);

                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    foreach (Equipment newItem in e.NewItems)
                    {
                        if (!controllerIdList.Contains(newItem.Id)) continue;
                        var replaceElement = EquipmentList.FirstOrDefault(x => x?.Id == newItem.Id);
                        if (replaceElement != null)
                            EquipmentList[EquipmentList.IndexOf(replaceElement)] = newItem;
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (Equipment oldItem in e.OldItems) EquipmentList.Remove(oldItem);

                    break;
                }
            }
        }
        */

        public bool LoadedAllData()
        {
            return ObservableUnfilteredIrrigation.LoadedData;
        }
    }
}