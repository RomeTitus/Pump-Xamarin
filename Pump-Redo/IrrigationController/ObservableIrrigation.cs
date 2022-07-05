using System.Collections.ObjectModel;

namespace Pump.IrrigationController
{
    public class ObservableIrrigation
    {

        public ObservableIrrigation()
        {
            AliveList = new ObservableCollection<Alive>();
            CustomScheduleList = new ObservableCollection<CustomSchedule>();
            EquipmentList = new ObservableCollection<Equipment> ();
            ManualScheduleList = new ObservableCollection<ManualSchedule>();
            ScheduleList = new ObservableCollection<Schedule>();
            SensorList = new ObservableCollection<Sensor>();
            SubControllerList = new ObservableCollection<SubController>();
        }
        public ObservableCollection<Alive> AliveList { get; }

        public ObservableCollection<CustomSchedule> CustomScheduleList { get; }

        public ObservableCollection<Equipment> EquipmentList { get; }

        public ObservableCollection<ManualSchedule> ManualScheduleList { get; }

        public ObservableCollection<Schedule> ScheduleList { get; }
        public ObservableCollection<Sensor> SensorList { get; }

        public ObservableCollection<SubController> SubControllerList { get; }
        
        public bool LoadedData { get; set; }
    }
}