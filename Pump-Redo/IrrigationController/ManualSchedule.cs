using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class ManualSchedule : IEntity, IManualSchedule, IStatus
    {
        public long EndTime { get; set; }

        public string Key { get; set; }
        public bool RunWithSchedule { get; set; }
        [JsonIgnore] public string Id { get; set; }

        [JsonIgnore] public bool DeleteAwaiting { get; set; }
        public List<ManualScheduleEquipment> ManualDetails { get; set; }
        public bool Failed { get; }
        public bool Complete { get; }
        public List<string> Steps { get; }
    }

    public class ManualScheduleEquipment
    {
        public string id_Equipment { get; set; }
    }
}