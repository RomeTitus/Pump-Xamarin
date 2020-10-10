using System;
using System.Collections.Generic;
using System.Linq;

namespace Pump.IrrigationController
{
    internal class RunningSchedule
    {
        public static IEnumerable<ActiveSchedule> GetActiveSchedule(IEnumerable<Schedule> scheduleList, List<Equipment> equipmentList)
        {
            var activeScheduleList = new List<ActiveSchedule>();


            foreach (var schedule in scheduleList)
            {
                if (schedule.isActive == "0")
                    continue;


                var hour = schedule.TIME.Split(':').First();
                var minute = schedule.TIME.Split(':').Last();
                var weekCalc = WeekCalculator(schedule.WEEK);
                foreach (var startTime in weekCalc)
                {
                    var startTimeDateTime = startTime;
                    startTimeDateTime += TimeSpan.FromHours(Convert.ToInt32(hour)) +
                                         TimeSpan.FromMinutes(Convert.ToInt32(minute));

                    foreach (var scheduleDetails in schedule.ScheduleDetails)
                    {
                        var activeSchedule = new ActiveSchedule
                        {
                            ID = schedule.ID, NAME = schedule.NAME, id_Equipment = scheduleDetails.id_Equipment
                        };
                        activeSchedule.name_Equipment =
                            equipmentList.FirstOrDefault(x => x.ID == activeSchedule.id_Equipment)?.NAME;
                        activeSchedule.id_Pump = schedule.id_Pump;
                        activeSchedule.name_Pump =
                            equipmentList.FirstOrDefault(x => x.ID == activeSchedule.id_Pump)?.NAME;
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
            var sortedList = activeScheduleList.OrderBy(o => o.StartTime).ToList();
            return sortedList;


        }

        private static IEnumerable<DateTime> WeekCalculator(string week)
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

        public static IEnumerable<ActiveSchedule> GetRunningSchedule(IEnumerable<ActiveSchedule> activeScheduleList)
        {
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime < DateTime.UtcNow && activeSchedule.EndTime > DateTime.UtcNow).ToList();
        }

        public static IEnumerable<ActiveSchedule> GetQueSchedule(IEnumerable<ActiveSchedule> activeScheduleList)
        {
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime > DateTime.UtcNow && activeSchedule.StartTime < DateTime.UtcNow.AddDays(1)).ToList();
        }
    }
}