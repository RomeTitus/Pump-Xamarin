using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class ManualSchedule : IEntity, IManualSchedule
    {
        [JsonIgnore] public string Id { get; set; }

        [JsonIgnore] public bool DeleteAwaiting { get; set; }

        public long EndTime { get; set; }

        public string Key { get; set; }
        public bool RunWithSchedule { get; set; }
        public List<ManualScheduleEquipment> ManualDetails { get; set; }
    }

    public class ManualScheduleEquipment
    {
        public string id_Equipment { get; set; }
    }
}