using System;
using System.Collections.Generic;

namespace Pump.IrrigationController
{
    public class CustomSchedule
    {

        public CustomSchedule()
        {
        }
        public string ID { get; set; }
        public string NAME { get; set; }
        public string id_Pump { get; set; }
        public string isActive => isScheduleRunning();
        public long StartTime { get; set; }


        public List<ScheduleDetail> ScheduleDetails { get; set; }

        private string isScheduleRunning()
        {
            var scheduleDetail = RunningCustomSchedule.GetCustomScheduleDetailRunning(this);
            return scheduleDetail != null ? "1" : "0";
        }

        public CustomSchedule Clone()
        {
            return (CustomSchedule)this.MemberwiseClone();
        }
    }
}