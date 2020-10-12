using System;
using System.Collections.Generic;

namespace Pump.IrrigationController
{
    public class Schedule
    {
        public string ID { get; set; }
        public string NAME { get; set; }
        public string TIME { get; set; }
        public string WEEK { get; set; }
        public string id_Pump { get; set; }
        public string isActive { get; set; }


        public List<ScheduleDetail> ScheduleDetails { get; set; }
        public Schedule Clone()
        {
            return (Schedule)this.MemberwiseClone();
        }
    }

    public class ScheduleDetail
    {
        public string ID { get; set; }
        public string DURATION { get; set; }
        public string id_Equipment { get; set; }
    }
}