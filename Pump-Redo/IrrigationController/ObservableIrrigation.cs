using System.Collections.ObjectModel;

namespace Pump.IrrigationController
{
    public class ObservableIrrigation
    {
        public readonly ObservableCollection<Alive> AliveList = new ObservableCollection<Alive> { null };

        public readonly ObservableCollection<CustomSchedule> CustomScheduleList =
            new ObservableCollection<CustomSchedule> { null };

        public readonly ObservableCollection<Equipment> EquipmentList = new ObservableCollection<Equipment> { null };

        public readonly ObservableCollection<ManualSchedule> ManualScheduleList =
            new ObservableCollection<ManualSchedule> { null };

        public readonly ObservableCollection<Schedule> ScheduleList = new ObservableCollection<Schedule> { null };
        public readonly ObservableCollection<Sensor> SensorList = new ObservableCollection<Sensor> { null };

        public readonly ObservableCollection<SubController> SubControllerList = new ObservableCollection<SubController>
            { null };

        public bool IsDisposable = false;


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