using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Pump.Class;

namespace Pump.IrrigationController
{
    internal static class RunningSchedule
    {
        private static IEnumerable<ActiveSchedule> GetActiveSchedules(ObservableCollection<CustomSchedule> scheduleList, ObservableCollection<Equipment> equipmentList)
        {
            if (scheduleList == null)
                return new List<ActiveSchedule>();

            var activeScheduleList = new List<ActiveSchedule>();
            
            foreach (var schedule in scheduleList)
            {
                if (schedule.StartTime == 0)
                    continue;

                activeScheduleList.AddRange(CreateActiveScheduleList(schedule, equipmentList));
            }


            foreach (var activeSchedul in activeScheduleList)
            {
                activeSchedul.StartTime = activeSchedul.StartTime.ToLocalTime();
                activeSchedul.EndTime = activeSchedul.EndTime.ToLocalTime();
            }

            return activeScheduleList;
        }
        
        private static IEnumerable<ActiveSchedule> CreateActiveScheduleList(CustomSchedule schedule, ObservableCollection<Equipment> equipmentList)
        {
            var activeScheduleList = new List<ActiveSchedule>();


            try
            {
                var startTime = schedule.StartTime;
                if (schedule.TimeAdjustment is not null)
                {
                    startTime -= schedule.TimeAdjustment.Value;
                }

                var customDateTimeStart = ScheduleTime.FromUnixTimeStampUtc(startTime);

                var currentTime = DateTime.UtcNow;
                var index = 0;
                for (var i = 0; i < schedule.Repeat + 1; i++)
                    foreach (var scheduleDetail in schedule.ScheduleDetails)
                    {
                        var activeSchedule = new ActiveSchedule
                        {
                            Id = schedule.Id + i,
                            TimeAdjustment = schedule.TimeAdjustment,
                            Name = schedule.NAME,
                            Weekday = null,
                            IdPump = schedule.id_Pump,
                            NamePump = equipmentList.FirstOrDefault(x => x.Id == schedule.id_Pump)?.NAME,
                            IdEquipment = scheduleDetail.id_Equipment,
                            NameEquipment = equipmentList.FirstOrDefault(x => x.Id == scheduleDetail.id_Equipment)?.NAME,
                            StartTime = customDateTimeStart
                        };

                        var durationHour = scheduleDetail.DURATION.Split(':').First();
                        var durationMinute = scheduleDetail.DURATION.Split(':').Last();
                        customDateTimeStart += TimeSpan.FromHours(Convert.ToInt32(durationHour)) +
                                               TimeSpan.FromMinutes(Convert.ToInt32(durationMinute));
                        activeSchedule.EndTime = customDateTimeStart;
                        
                        customDateTimeStart = activeSchedule.EndTime;
                        index++;

                        activeScheduleList.Add(activeSchedule);
                    }
            }
            catch
            {
                // ignored
            }
            return activeScheduleList;
        }

        private static IEnumerable<ActiveSchedule> GetActiveSchedules(ObservableCollection<Schedule> scheduleList, ObservableCollection<Equipment> equipmentList, bool loadNextWeek = false)
        {
            if (scheduleList == null)
                return new List<ActiveSchedule>();

            var activeScheduleList = new List<ActiveSchedule>();
            var today = DateTime.Today;
            foreach (var schedule in scheduleList)
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
                    
                    activeScheduleList.AddRange(CreateActiveScheduleList(schedule, equipmentList, endTime, weekDay));
                }
            }

            foreach (var activeSchedul in activeScheduleList)
            {
                activeSchedul.StartTime = activeSchedul.StartTime.ToLocalTime();
                activeSchedul.EndTime = activeSchedul.EndTime.ToLocalTime();
            }

            return activeScheduleList;
        }

        private static IEnumerable<ActiveSchedule> CreateActiveScheduleList(Schedule schedule, ObservableCollection<Equipment> equipmentList, DateTime endTime, string weekDay)
        {
            var activeScheduleList = new List<ActiveSchedule>();
            
            int? timeAdjustment = null;
            /*
            if(string.IsNullOrEmpty(schedule.TimeAdjustment) == false)
            {
                var timeAdjustmentDetail = schedule.TimeAdjustment.Split(',')
                .FirstOrDefault(x => x.Contains(weekDay.ToUpper()));

                if (timeAdjustmentDetail is not null)
                {
                    timeAdjustment = Convert.ToInt32(timeAdjustmentDetail.Split('@')[1]);
                    endTime = endTime.AddSeconds(timeAdjustment.Value);
                }
            }
            */

            foreach (var detail in schedule.ScheduleDetails)
            {
                var scheduleDetail = new ActiveSchedule
                {
                    Id = weekDay + "@" + schedule.Id, Name = schedule.NAME,
                    IdEquipment = detail.id_Equipment,
                    IdPump = schedule.id_Pump,
                    //TimeAdjustment = timeAdjustment,
                    Weekday = weekDay
                };

                scheduleDetail.NameEquipment =
                    equipmentList.FirstOrDefault(x => x?.Id == scheduleDetail.IdEquipment)?.NAME;
                scheduleDetail.NamePump =
                    equipmentList.FirstOrDefault(x => x?.Id == scheduleDetail.IdPump)?.NAME;

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
        
        public static DateTime? GetScheduleRunningTimeForEquipment(this CustomSchedule schedule, int selectIndex)
        {
            try
            {
                var startTimeDateTime = DateTime.UtcNow;
                var index = 0;
                for (var i = 0; i < schedule.Repeat + 1; i++)
                    foreach (var scheduleDetails in schedule.ScheduleDetails)
                    {
                        if (index == selectIndex) return startTimeDateTime;
                        //gets Next Schedule Start Time
                        var durationHour = scheduleDetails.DURATION.Split(':').First();
                        var durationMinute = scheduleDetails.DURATION.Split(':').Last();
                        var endTimeDateTime = startTimeDateTime - (TimeSpan.FromHours(Convert.ToInt32(durationHour)) +
                                                                   TimeSpan.FromMinutes(Convert.ToInt32(durationMinute))
                            );
                        startTimeDateTime = endTimeDateTime;
                        index++;
                    }
            }
            catch
            {
                // ignored
            }
            return null;
        }

        public static DateTime? GetScheduleEndTime(this CustomSchedule schedule)
        {
            try
            {
                var startTime = schedule.StartTime;
                if (schedule.TimeAdjustment is not null)
                {
                    startTime -= schedule.TimeAdjustment.Value;
                }
            
                var customDateTimeStart =  ScheduleTime.FromUnixTimeStampUtc(startTime);
                
                for (var i = 0; i < schedule.Repeat + 1; i++)
                    foreach (var scheduleDetails in schedule.ScheduleDetails)
                    {
                        //gets Next Schedule Start Time
                        var durationHour = scheduleDetails.DURATION.Split(':').First();
                        var durationMinute = scheduleDetails.DURATION.Split(':').Last();
                        var endTimeDateTime = customDateTimeStart + TimeSpan.FromHours(Convert.ToInt32(durationHour)) +
                                              TimeSpan.FromMinutes(Convert.ToInt32(durationMinute));
                        customDateTimeStart = endTimeDateTime;
                    }
                return customDateTimeStart;
            }
            catch
            {
                // ignored
            }
            return null;
        }
        
        public static ScheduleDetail GetScheduleDetailRunning(this CustomSchedule schedule)
        {
            try
            {
                var startTime = schedule.StartTime;
                if (schedule.TimeAdjustment is not null)
                {
                    startTime -= schedule.TimeAdjustment.Value;
                }
            
                var customDateTimeStart =  ScheduleTime.FromUnixTimeStampUtc(startTime);
                
                var currentTime = DateTime.UtcNow;
                var index = 0;
                for (var i = 0; i < schedule.Repeat + 1; i++)
                    foreach (var scheduleDetails in schedule.ScheduleDetails)
                    {
                        //gets Next Schedule Start Time
                        scheduleDetails.ID = index.ToString();
                        var durationHour = scheduleDetails.DURATION.Split(':').First();
                        var durationMinute = scheduleDetails.DURATION.Split(':').Last();
                        var endTimeDateTime = customDateTimeStart + TimeSpan.FromHours(Convert.ToInt32(durationHour)) +
                                              TimeSpan.FromMinutes(Convert.ToInt32(durationMinute));
                        if (customDateTimeStart < currentTime && endTimeDateTime > currentTime)
                            return scheduleDetails.Clone();
                        customDateTimeStart = endTimeDateTime;
                        index++;
                    }
            }
            catch
            {
                // ignored
            }
            return null;
        }
        
        public static IEnumerable<ActiveSchedule> GetRunningSchedule(this ObservableCollection<Schedule> scheduleList, ObservableCollection<Equipment> equipmentList)
        {
            var activeScheduleList = GetActiveSchedules(scheduleList, equipmentList);
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime < DateTime.Now.ToLocalTime() && activeSchedule.EndTime > DateTime.Now.ToLocalTime()).ToList();
        }

        public static IEnumerable<ActiveSchedule> GetQueSchedule(this ObservableCollection<Schedule> scheduleList, ObservableCollection<Equipment> equipmentList)
        {
            var activeScheduleList = GetActiveSchedules(scheduleList, equipmentList, true);
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime > DateTime.Now.ToLocalTime() && activeSchedule.StartTime < DateTime.Now.AddDays(2).ToLocalTime()).ToList();
        }
        
        public static IEnumerable<ActiveSchedule> GetRunningSchedule(this ObservableCollection<CustomSchedule> scheduleList, ObservableCollection<Equipment> equipmentList)
        {
            var activeScheduleList = GetActiveSchedules(scheduleList, equipmentList);
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime < DateTime.Now.ToLocalTime() && activeSchedule.EndTime > DateTime.Now.ToLocalTime()).ToList();
        }
        
        public static IEnumerable<ActiveSchedule> GetQueSchedule(this ObservableCollection<CustomSchedule> scheduleList, ObservableCollection<Equipment> equipmentList)
        {
            var activeScheduleList = GetActiveSchedules(scheduleList, equipmentList);
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime > DateTime.Now.ToLocalTime() && activeSchedule.StartTime < DateTime.Now.AddDays(7).ToLocalTime()).ToList();
        }
    }
}