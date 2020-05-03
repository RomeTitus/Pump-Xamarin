using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pump.Database.Table
{
    class ManualScheduleClass
    {
        public ManualScheduleClass(){}
        List<string> EquipmentID = new List<string>();
        public bool RunWithSchedule { get; set; }
        public string ScheduleTime { get; set; }

        public void setEquipmentIDAndTime(List<string> ScheduleDetail)
        {
            foreach (var Schedule in ScheduleDetail)
            {
                var detail = Schedule.Split(',').ToList<string>();
                ScheduleTime = detail[1];
                EquipmentID.Add(detail[0]);
            }
        }
    
        public List<string> getEquipmentID()
        {
            return EquipmentID;
        }
    }
}
