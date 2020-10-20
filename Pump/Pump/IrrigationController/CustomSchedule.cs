using System;
using System.Collections.Generic;

namespace Pump.IrrigationController
{
    public class CustomSchedule
    {

        public CustomSchedule()
        {
            id_Pump = string.Empty;
            ScheduleDetails = new List<ScheduleDetail>();
            Repeat = 0;
        }
        public string ID { get; set; }
        public string NAME { get; set; }
        public string id_Pump { get; set; }
        public long StartTime { get; set; }
        public long Repeat { get; set; }


        public List<ScheduleDetail> ScheduleDetails { get; set; }
    }
}