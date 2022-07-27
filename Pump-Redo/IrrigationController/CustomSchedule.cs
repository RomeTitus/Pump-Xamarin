using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class CustomSchedule : ISchedule
    {
        public CustomSchedule()
        {
            id_Pump = string.Empty;
            ScheduleDetails = new List<ScheduleDetail>();
            Repeat = 0;
        }

        public string NAME { get; set; }

        public string Key { get; set; }
        public long StartTime { get; set; }
        public long Repeat { get; set; }

        [JsonIgnore] public string Id { get; set; }

        public bool DeleteAwaiting { get; set; }
        public string id_Pump { get; set; }

        public List<ScheduleDetail> ScheduleDetails { get; set; }
        public ControllerStatus ControllerStatus { get; }
    }
}