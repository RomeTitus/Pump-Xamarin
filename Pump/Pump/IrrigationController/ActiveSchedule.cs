using System;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class ActiveSchedule
    {
        [JsonIgnore] public string Id { get; set; }

        public string Name { get; set; }
        public string IdPump { get; set; }
        public string NamePump { get; set; }
        public string IdEquipment { get; set; }
        public string NameEquipment { get; set; }
        public string Week { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}