using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class Schedule : ISchedule
    {
        public Schedule()
        {
            isActive = "1";
            WEEK = string.Empty;
            ScheduleDetails = new List<ScheduleDetail>();
        }

        public string NAME { get; set; }
        public string TimeAdjustment { get; set; }
        
        public bool AdaptiveSchedule { get; set; }
        
        public string Key { get; set; }
        public string TIME { get; set; }
        public string WEEK { get; set; }
        public string isActive { get; set; }

        [JsonIgnore] public string Id { get; set; }
        [JsonIgnore] public bool HasUpdated { get; set; }
        public bool DeleteAwaiting { get; set; }
        public string id_Pump { get; set; }


        public List<ScheduleDetail> ScheduleDetails { get; set; }


        public ControllerStatus ControllerStatus { get; set; }
    }

    public class ScheduleDetail
    {
        [JsonIgnore] public string ID { get; set; }

        public string DURATION { get; set; }
        public string id_Equipment { get; set; }

        public ScheduleDetail Clone()
        {
            return (ScheduleDetail)MemberwiseClone();
        }
    }
}