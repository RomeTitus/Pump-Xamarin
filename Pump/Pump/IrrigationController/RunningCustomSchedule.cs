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

                    foreach (var scheduleDetails in schedule.ScheduleDetails)
                    {
                        var activeSchedule = new ActiveSchedule
                        {
                            ID = schedule.ID+ scheduleDetails.id_Equipment, NAME = schedule.NAME, id_Equipment = scheduleDetails.id_Equipment
                        };
                        activeSchedule.name_Equipment =
                            equipmentList.FirstOrDefault(x => x.ID == activeSchedule.id_Equipment)?.NAME;
                        activeSchedule.id_Pump = schedule.id_Pump;
                        activeSchedule.name_Pump =
                            equipmentList.FirstOrDefault(x => x.ID == activeSchedule.id_Pump)?.NAME;
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
            
            var sortedList = activeScheduleList.OrderBy(o => o.StartTime).ToList();
            return sortedList;


        }

        public static ScheduleDetail GetCustomScheduleDetailRunning(CustomSchedule customScheduleList)
        {
            try
            {
                var startTimeDateTime = ScheduleTime.FromUnixTimeStampUtc(customScheduleList.StartTime);
                var currentTime = DateTime.UtcNow;
                foreach (var scheduleDetails in customScheduleList.ScheduleDetails)
                {
                    //gets Next Schedule Start Time
                    var durationHour = scheduleDetails.DURATION.Split(':').First();
                    var durationMinute = scheduleDetails.DURATION.Split(':').Last();
                    var endTimeDateTime = startTimeDateTime + TimeSpan.FromHours(Convert.ToInt32(durationHour)) +
                                          TimeSpan.FromMinutes(Convert.ToInt32(durationMinute));
                    if (startTimeDateTime < currentTime && endTimeDateTime > currentTime)
                        return scheduleDetails;
                    startTimeDateTime = endTimeDateTime;
                }
            }
            catch (Exception e)
            {

            }
            return null;
        }

        public DateTime? getCustomScheduleEndTime(CustomSchedule customScheduleList)
        {
            try
            {
                var startTimeDateTime = ScheduleTime.FromUnixTimeStampUtc(customScheduleList.StartTime);
                foreach (var scheduleDetails in customScheduleList.ScheduleDetails)
                {
                    //gets Next Schedule Start Time
                    var durationHour = scheduleDetails.DURATION.Split(':').First();
                    var durationMinute = scheduleDetails.DURATION.Split(':').Last();
                    var endTimeDateTime = startTimeDateTime + TimeSpan.FromHours(Convert.ToInt32(durationHour)) +
                                          TimeSpan.FromMinutes(Convert.ToInt32(durationMinute));
                    startTimeDateTime = endTimeDateTime;
                }

                return startTimeDateTime;
            }
            catch (Exception e)
            {

            }
            return null;
        }

        public static DateTime? GetCustomScheduleRunningTimeForEquipment(CustomSchedule customScheduleList, Equipment equipment)
        {
            try
            {
                var startTimeDateTime = DateTime.UtcNow;
                foreach (var scheduleDetails in customScheduleList.ScheduleDetails)
                {
                    if (scheduleDetails.id_Equipment == equipment.ID)
                    {
                        return startTimeDateTime;
                    }
                    //gets Next Schedule Start Time
                    var durationHour = scheduleDetails.DURATION.Split(':').First();
                    var durationMinute = scheduleDetails.DURATION.Split(':').Last();
                    var endTimeDateTime = startTimeDateTime - (TimeSpan.FromHours(Convert.ToInt32(durationHour)) +
                                          TimeSpan.FromMinutes(Convert.ToInt32(durationMinute)));
                    startTimeDateTime = endTimeDateTime;
                    
                }

            }
            catch (Exception e)
            {

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

