using System;

namespace Pump.IrrigationController
{
    public class ActiveSchedule
    {
        public string ID { get; set; }
        public string NAME { get; set; }
        public string id_Pump { get; set; }
        public string name_Pump { get; set; }
        public string id_Equipment { get; set; }
        public string name_Equipment { get; set; }
        public string WEEK { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}