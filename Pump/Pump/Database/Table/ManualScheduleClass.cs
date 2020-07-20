using System.Collections.Generic;
using System.Linq;

namespace Pump.Database.Table
{
    internal class ManualScheduleClass
    {
        private readonly List<string> EquipmentID = new List<string>();
        public bool RunWithSchedule { get; set; }
        public string ScheduleTime { get; set; }

        public void setEquipmentIDAndTime(List<string> ScheduleDetail)
        {
            foreach (var Schedule in ScheduleDetail)
            {
                var detail = Schedule.Split(',').ToList();
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