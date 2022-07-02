using System.Collections.ObjectModel;

namespace Pump.IrrigationController
{
    public class ObservableIrrigation
    {

        public ObservableIrrigation()
        {
            AliveList = new ObservableCollection<Alive> { null };
            CustomScheduleList = new ObservableCollection<CustomSchedule> { null };
            EquipmentList = new ObservableCollection<Equipment> { null };
            ManualScheduleList = new ObservableCollection<ManualSchedule> { null };
            ScheduleList = new ObservableCollection<Schedule> { null };
            SensorList = new ObservableCollection<Sensor> { null };
            SubControllerList = new ObservableCollection<SubController> { null };
        }
        
        public ObservableCollection<Alive> AliveList { get; }

        public ObservableCollection<CustomSchedule> CustomScheduleList { get; }

        public ObservableCollection<Equipment> EquipmentList { get; }

        public ObservableCollection<ManualSchedule> ManualScheduleList { get; }

        public ObservableCollection<Schedule> ScheduleList { get; }
        public ObservableCollection<Sensor> SensorList { get; }

        public ObservableCollection<SubController> SubControllerList { get; }

        
        public bool IsDisposable { get; set; }


        public bool LoadedAllData()
        {
            if (IsDisposable)
                return false;
            return !EquipmentList.Contains(null) && !SensorList.Contains(null) && !ManualScheduleList.Contains(null) &&
                   !ScheduleList.Contains(null) && !CustomScheduleList.Contains(null) &&
                   !AliveList.Contains(null) && !SubControllerList.Contains(null);
        }
    }
}