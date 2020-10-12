using System;
using System.Collections.Generic;
using System.Text;

namespace Pump.IrrigationController
{
    public class ManualSchedule
    {
        public string ID;
        public long EndTime { get; set; }
        public string DURATION { get; set; }
        public bool RunWithSchedule { get; set; }
        public List<ManualScheduleEquipment> ManualDetails { get; set; }
        public ManualSchedule Clone()
        {
            return (ManualSchedule)this.MemberwiseClone();
        }
    }

    public class ManualScheduleEquipment
    {
        public string id_Equipment { get; set; }
    }
}
