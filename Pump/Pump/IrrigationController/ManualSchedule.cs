using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class ManualSchedule
    {
        [JsonIgnore]
        public string ID;
        [JsonIgnore]
        public bool DeleteAwaiting { get; set; }
        public long EndTime { get; set; }
        public bool RunWithSchedule { get; set; }
        public List<ManualScheduleEquipment> ManualDetails { get; set; }
    }

    public class ManualScheduleEquipment
    {
        public string id_Equipment { get; set; }
    }
}
