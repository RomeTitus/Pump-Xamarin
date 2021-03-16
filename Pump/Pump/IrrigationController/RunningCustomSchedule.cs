using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pump.Class;

namespace Pump.IrrigationController
{
    class RunningCustomSchedule
    {

        public List<ActiveSchedule> GetActiveCustomSchedule(List<CustomSchedule> customScheduleList, List<Equipment> equipmentList)
        {
            var activeScheduleList = new List<ActiveSchedule>();


            foreach (var schedule in customScheduleList)
            {

                var startTimeDateTime = ScheduleTime.FromUnixTimeStampUtc(schedule.StartTime);
                for (var i = 0; i < schedule.Repeat+1; i++)
                {
                    foreach (var scheduleDetails in schedule.ScheduleDetails)
                    {
                        var activeSchedule = new ActiveSchedule
                        {
                            Id = schedule.ID + scheduleDetails.id_Equipment,
                            Name = schedule.NAME,
                            IdEquipment = scheduleDetails.id_Equipment
                        };
                        activeSchedule.NameEquipment =
                            equipmentList.FirstOrDefault(x => x.ID == activeSchedule.IdEquipment)?.NAME;
                        activeSchedule.IdPump = schedule.id_Pump;
                        activeSchedule.NamePump =
                            equipmentList.FirstOrDefault(x => x.ID == activeSchedule.IdPump)?.NAME;
                        activeSchedule.StartTime = startTimeDateTime;

                        //gets Next Schedule Start Time
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

        public static ScheduleDetail GetCustomScheduleDetailRunning(CustomSchedule customScheduleList)
        {
            try
            {
                var startTimeDateTime = ScheduleTime.FromUnixTimeStampUtc(customScheduleList.StartTime);
                var currentTime = DateTime.UtcNow;
                var index = 0;
                for (var i = 0; i < customScheduleList.Repeat+1; i++)
                {
                    foreach (var scheduleDetails in customScheduleList.ScheduleDetails)
                    {
                        //gets Next Schedule Start Time
                        scheduleDetails.ID = index.ToString();
                        var durationHour = scheduleDetails.DURATION.Split(':').First();
                        var durationMinute = scheduleDetails.DURATION.Split(':').Last();
                        var endTimeDateTime = startTimeDateTime + TimeSpan.FromHours(Convert.ToInt32(durationHour)) +
                                              TimeSpan.FromMinutes(Convert.ToInt32(durationMinute));
                        if (startTimeDateTime < currentTime && endTimeDateTime > currentTime)
                            return scheduleDetails.Clone();
                        startTimeDateTime = endTimeDateTime;
                        index++;
                    }
                }
            }
            catch
            {
                // ignored
            }

            return null;
        }

        public DateTime? getCustomScheduleEndTime(CustomSchedule customScheduleList)
        {
            try
            {
                var startTimeDateTime = ScheduleTime.FromUnixTimeStampUtc(customScheduleList.StartTime);
                for (var i = 0; i < customScheduleList.Repeat+1; i++)
                {
                    foreach (var scheduleDetails in customScheduleList.ScheduleDetails)
                    {
                        //gets Next Schedule Start Time
                        var durationHour = scheduleDetails.DURATION.Split(':').First();
                        var durationMinute = scheduleDetails.DURATION.Split(':').Last();
                        var endTimeDateTime = startTimeDateTime + TimeSpan.FromHours(Convert.ToInt32(durationHour)) +
                                              TimeSpan.FromMinutes(Convert.ToInt32(durationMinute));
                        startTimeDateTime = endTimeDateTime;
                    }
                }

                return startTimeDateTime;
            }
            catch
            {
                // ignored
            }
            return null;
        }

        public static DateTime? GetCustomScheduleRunningTimeForEquipment(CustomSchedule customScheduleList, int selectIndex)
        {
            try
            {
                var startTimeDateTime = DateTime.UtcNow;
                var index = 0;
                for (var i = 0; i < customScheduleList.Repeat+1; i++)
                {
                    foreach (var scheduleDetails in customScheduleList.ScheduleDetails)
                    {
                        if (index == selectIndex)
                        {
                            return startTimeDateTime;
                        }

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

            }
            catch
            {
                // ignored
            }

            return null;
        }

        public List<ActiveSchedule> GetRunningCustomSchedule(List<ActiveSchedule> activeScheduleList)
        {
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime < DateTime.UtcNow && activeSchedule.EndTime > DateTime.UtcNow).ToList();
        }

        public List<ActiveSchedule> GetQueCustomSchedule(List<ActiveSchedule> activeScheduleList)
        {
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime > DateTime.UtcNow && activeSchedule.StartTime < DateTime.UtcNow.AddDays(1)).ToList();
        }
    }
}

