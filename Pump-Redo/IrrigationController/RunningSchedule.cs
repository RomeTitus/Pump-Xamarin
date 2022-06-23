using System;
using System.Collections.Generic;
using System.Linq;

namespace Pump.IrrigationController
{
    internal class RunningSchedule
    {
        private readonly IEnumerable<Equipment> _equipmentList;
        private readonly IEnumerable<Schedule> _scheduleList;

        public RunningSchedule(IEnumerable<Schedule> scheduleList, IEnumerable<Equipment> equipmentList)
        {
            _scheduleList = scheduleList;
            _equipmentList = equipmentList;
        }

        public IEnumerable<ActiveSchedule> GetActiveSchedule()
        {
            var activeScheduleList = new List<ActiveSchedule>();
            if (_scheduleList == null)
                return new List<ActiveSchedule>();

            foreach (var schedule in _scheduleList)
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
                            Id = schedule.Id + scheduleDetails.id_Equipment, Name = schedule.NAME,
                            IdEquipment = scheduleDetails.id_Equipment
                        };
                        activeSchedule.NameEquipment =
                            _equipmentList.FirstOrDefault(x => x?.Id == activeSchedule.IdEquipment)?.NAME;
                        activeSchedule.IdPump = schedule.id_Pump;
                        activeSchedule.NamePump =
                            _equipmentList.FirstOrDefault(x => x?.Id == activeSchedule.IdPump)?.NAME;
                        activeSchedule.StartTime = startTimeDateTime;
                        activeSchedule.Week = schedule.WEEK;
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

        public IEnumerable<ActiveSchedule> GetRunningSchedule()
        {
            var activeScheduleList = GetActiveSchedule();
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime < DateTime.Now && activeSchedule.EndTime > DateTime.Now).ToList();
        }

        public IEnumerable<ActiveSchedule> GetQueSchedule()
        {
            var activeScheduleList = GetActiveSchedule();
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime > DateTime.Now && activeSchedule.StartTime < DateTime.Now.AddDays(1)).ToList();
        }
    }
}