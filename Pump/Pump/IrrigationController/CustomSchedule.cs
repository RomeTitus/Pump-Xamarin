using System.Collections.Generic;

namespace Pump.IrrigationController
{
    public class CustomSchedule
    {

        public CustomSchedule()
        {
            isActive = "0";
        }
        public string ID { get; set; }
        public string NAME { get; set; }
        public string id_Pump { get; set; }
        public string isActive { get; set; }
        public long StartTime { get; set; }


        public List<ScheduleDetail> ScheduleDetails { get; set; }
    }
}