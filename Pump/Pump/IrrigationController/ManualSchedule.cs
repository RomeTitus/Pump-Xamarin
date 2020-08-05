using System;
using System.Collections.Generic;
using System.Text;

namespace Pump.IrrigationController
{
    class ManualSchedule
    {
        public long EndTime { get; set; }
        public string DURATION { get; set; }
        public bool RunWithSchedule { get; set; }
        public List<ManualScheduleEquipment> equipmentIdList { get; set; }
    }

    class ManualScheduleEquipment
    {
        public string ID { get; set; }
    }
}
