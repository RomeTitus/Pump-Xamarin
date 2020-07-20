using System;
using System.Collections.Generic;
using System.Linq;
using Pump.FirebaseDatabase;

namespace Pump.IrrigationController
{
    internal class RunningSchedule
    {
        public List<ActiveSchedule> GetActiveSchedule(List<Schedule> scheduleList, List<Equipment> EquipmentList)
        {
            var activeScheduleList = new List<ActiveSchedule>();


            foreach (var schedule in scheduleList)
            {
                if (schedule.isActive == "0")
                    continue;


                var hour = schedule.TIME.Split(':').First();
                var minute = schedule.TIME.Split(':').Last();
                var weekCalc = weekCalculator(schedule.WEEK);
                foreach (var startTime in weekCalc)
                {
                    var startTimeDateTime = startTime;
                    startTimeDateTime += TimeSpan.FromHours(Convert.ToInt32(hour)) +
                                         TimeSpan.FromMinutes(Convert.ToInt32(minute));

                    foreach (var scheduleDetails in schedule.ScheduleDetails)
                    {
                        var activeSchedule = new ActiveSchedule();
                        activeSchedule.ID = schedule.ID;
                        activeSchedule.NAME = schedule.NAME;
                        activeSchedule.id_Equipment = scheduleDetails.id_Equipment;
                        activeSchedule.name_Equipment =
                            EquipmentList.FirstOrDefault(x => x.ID == activeSchedule.id_Equipment)?.NAME;
                        activeSchedule.id_Pump = schedule.id_Pump;
                        activeSchedule.name_Pump =
                            EquipmentList.FirstOrDefault(x => x.ID == activeSchedule.id_Pump)?.NAME;
                        activeSchedule.StartTime = startTimeDateTime;
                        activeSchedule.WEEK = schedule.WEEK;
                        var durationHour = scheduleDetails.DURATION.Split(':').First();
                        var durationMinute = scheduleDetails.DURATION.Split(':').Last();
                        startTimeDateTime += TimeSpan.FromHours(Convert.ToInt32(durationHour)) +
                                             TimeSpan.FromMinutes(Convert.ToInt32(durationMinute));
                        activeSchedule.EndTime = startTimeDateTime;
                        activeScheduleList.Add(activeSchedule);
                    }
                }
            }

            return activeScheduleList;
        }

        private List<DateTime> weekCalculator(string week)
        {
            var startTimeList = new List<DateTime>();
            var weekStringDayList = week.Split(',');
            var weekIntDayList = new List<int>();
            if (weekStringDayList.Contains("SUNDAY"))
                weekIntDayList.Add(0);
            if (weekStringDayList.Contains("MONDAY"))
                weekIntDayList.Add(1);
            if (weekStringDayList.Contains("TUESDAY"))
                weekIntDayList.Add(2);
            if (weekStringDayList.Contains("WEDNESDAY"))
                weekIntDayList.Add(3);
            if (weekStringDayList.Contains("THURSDAY"))
                weekIntDayList.Add(4);
            if (weekStringDayList.Contains("FRIDAY"))
                weekIntDayList.Add(5);
            if (weekStringDayList.Contains("SATURDAY"))
                weekIntDayList.Add(6);
            foreach (var weekInt in weekIntDayList)
            {
                var startWeek = Convert.ToInt32(DateTime.Today.DayOfWeek);
                var daysAhead = weekInt - startWeek;
                if (daysAhead < 0)
                    daysAhead += 7;

                startTimeList.Add(DateTime.Today + TimeSpan.FromDays(daysAhead));
            }

            return startTimeList;
        }

        public List<ActiveSchedule> GetRunningSchedule(List<ActiveSchedule> activeScheduleList)
        {
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime < DateTime.Now && activeSchedule.EndTime > DateTime.Now).ToList();
        }

        public List<ActiveSchedule> GetQueSchedule(List<ActiveSchedule> activeScheduleList)
        {
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.EndTime > DateTime.Now && activeSchedule.StartTime < DateTime.Now.AddDays(1)).ToList();
        }
    }
}