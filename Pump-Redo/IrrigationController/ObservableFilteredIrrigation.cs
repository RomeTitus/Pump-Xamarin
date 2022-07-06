using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
            Filter();
        }

        private void Filter()
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

        public bool LoadedAllData()
        {
            return ObservableUnfilteredIrrigation.LoadedData;
        }
    }
}