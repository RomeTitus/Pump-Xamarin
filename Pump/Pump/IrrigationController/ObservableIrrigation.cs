﻿using System.Collections.ObjectModel;

namespace Pump.IrrigationController
{
    public class ObservableIrrigation
    {
        public readonly ObservableCollection<Equipment> EquipmentList = new ObservableCollection<Equipment> { null };
        public readonly ObservableCollection<Sensor> SensorList = new ObservableCollection<Sensor> { null };
        public readonly ObservableCollection<ManualSchedule> ManualScheduleList = new ObservableCollection<ManualSchedule> { null };
        public readonly ObservableCollection<Schedule> ScheduleList = new ObservableCollection<Schedule> { null };
        public readonly ObservableCollection<CustomSchedule> CustomScheduleList = new ObservableCollection<CustomSchedule> { null };
        public readonly ObservableCollection<Site> SiteList = new ObservableCollection<Site> { null };
        public readonly ObservableCollection<Alive> AliveList = new ObservableCollection<Alive> { null };
        public readonly ObservableCollection<SubController> SubControllerList = new ObservableCollection<SubController> { null };
        
    }
}
