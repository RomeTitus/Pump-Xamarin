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

        private IEnumerable<ActiveSchedule> GetActiveSchedules(bool loadNextWeek = false)
        {
            if (_scheduleList == null)
                return new List<ActiveSchedule>();

            var activeScheduleList = new List<ActiveSchedule>();
            var today = DateTime.Today;
            foreach (var schedule in _scheduleList)
            {
                if (schedule.isActive == "0")
                    continue;
                var scheduleTimeSplit = schedule.TIME.Split(':');
                
                foreach (var weekDay in schedule.WEEK.Split(','))
                {
                    if(string.IsNullOrEmpty(weekDay))
                        continue;
                    var scheduleDate = LastDay(today, weekDay, loadNextWeek).AddHours(Convert.ToInt32(scheduleTimeSplit.First())).AddMinutes(Convert.ToInt32(scheduleTimeSplit.Last()));
                    var endTime = scheduleDate;
                    
                    activeScheduleList.AddRange(CreateActiveScheduleList(schedule, endTime, weekDay));
                }
            }
            return activeScheduleList;
        }

        private List<ActiveSchedule> CreateActiveScheduleList(Schedule schedule, DateTime endTime, string weekDay)
        {
            var activeScheduleList = new List<ActiveSchedule>();
            
            int? timeAdjustment = null;
            var timeAdjustmentDetail = schedule.TimeAdjustment.Split(',')
                .FirstOrDefault(x => x.Contains(weekDay.ToUpper()));

            if (timeAdjustmentDetail is not null)
            {
                timeAdjustment = Convert.ToInt32(timeAdjustmentDetail.Split('@')[1]);   
                endTime = endTime.AddSeconds(timeAdjustment.Value);
            }

            foreach (var detail in schedule.ScheduleDetails)
            {
                var scheduleDetail = new ActiveSchedule
                {
                    Id = weekDay + "@" + schedule.Id, Name = schedule.NAME,
                    IdEquipment = detail.id_Equipment,
                    IdPump = schedule.id_Pump,
                    TimeAdjustment = timeAdjustment,
                    Weekday = weekDay
                };
                        
                scheduleDetail.NameEquipment =
                    _equipmentList.FirstOrDefault(x => x?.Id == scheduleDetail.IdEquipment)?.NAME;
                scheduleDetail.NamePump =
                    _equipmentList.FirstOrDefault(x => x?.Id == scheduleDetail.IdPump)?.NAME;

                scheduleDetail.StartTime = endTime;
                        
                var durationHour = detail.DURATION.Split(':').First();
                var durationMinute = detail.DURATION.Split(':').Last();
                        
                endTime += TimeSpan.FromHours(Convert.ToInt32(durationHour)) +
                           TimeSpan.FromMinutes(Convert.ToInt32(durationMinute));
                scheduleDetail.EndTime = endTime;

                activeScheduleList.Add(scheduleDetail);
            }

            return activeScheduleList;
        }

        private static DateTime LastDay(DateTime date, string dayName, bool loadNextWeek)
        {
            var daysOfWeek = new List<string>
            {
                "SUNDAY", "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY"
            };

            var targetDay = daysOfWeek.IndexOf(dayName.ToUpper());
            
            var deltaDay = targetDay - (int) date.DayOfWeek; 
            if (deltaDay == 0)
                deltaDay = 7;

            switch (deltaDay)
            {
                case -7:
                    return date;
                case > 0:
                    deltaDay -= 7; // go back 7 days
                    break;
            }

            if (loadNextWeek)
            {
                return date + TimeSpan.FromDays(deltaDay+7);
            }
                
            return date + TimeSpan.FromDays(deltaDay);
        }

        public IEnumerable<ActiveSchedule> GetRunningSchedule()
        {
            var activeScheduleList = GetActiveSchedules();
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime < DateTime.Now && activeSchedule.EndTime > DateTime.Now).ToList();
        }

        public IEnumerable<ActiveSchedule> GetQueSchedule()
        {
            var activeScheduleList = GetActiveSchedules(true);
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime > DateTime.Now && activeSchedule.StartTime < DateTime.Now.AddDays(2)).ToList();
        }
    }
}