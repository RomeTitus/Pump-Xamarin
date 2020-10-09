using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pump.IrrigationController
{
    class RunningCustomSchedule
    {

        public List<ActiveSchedule> GetActiveCustomSchedule(List<CustomSchedule> customScheduleList, List<Equipment> EquipmentList)
        {
            var activeScheduleList = new List<ActiveSchedule>();


            foreach (var schedule in customScheduleList)
            {

                var startTimeDateTime = new ScheduleTime().FromUnixTimeStamp(schedule.StartTime);

                    foreach (var scheduleDetails in schedule.ScheduleDetails)
                    {
                        var activeSchedule = new ActiveSchedule
                        {
                            ID = schedule.ID, NAME = schedule.NAME, id_Equipment = scheduleDetails.id_Equipment
                        };
                        activeSchedule.name_Equipment =
                            EquipmentList.FirstOrDefault(x => x.ID == activeSchedule.id_Equipment)?.NAME;
                        activeSchedule.id_Pump = schedule.id_Pump;
                        activeSchedule.name_Pump =
                            EquipmentList.FirstOrDefault(x => x.ID == activeSchedule.id_Pump)?.NAME;
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
            
            List<ActiveSchedule> sortedList = activeScheduleList.OrderBy(o => o.StartTime).ToList();
            return sortedList;


        }

        public ScheduleDetail getCustomScheduleDetailRunning(CustomSchedule customScheduleList)
        {
            try
            {
                var startTimeDateTime = new ScheduleTime().FromUnixTimeStamp(customScheduleList.StartTime);
                var currentTime = DateTime.Now;
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
                var startTimeDateTime = new ScheduleTime().FromUnixTimeStamp(customScheduleList.StartTime);
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

        public List<ActiveSchedule> GetRunningCustomSchedule(List<ActiveSchedule> activeScheduleList)
        {
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime < DateTime.Now && activeSchedule.EndTime > DateTime.Now).ToList();
        }

        public List<ActiveSchedule> GetQueCustomSchedule(List<ActiveSchedule> activeScheduleList)
        {
            return activeScheduleList.Where(activeSchedule =>
                activeSchedule.StartTime > DateTime.Now && activeSchedule.StartTime < DateTime.Now.AddDays(1)).ToList();
        }
    }
}

