using System;
using System.Collections.Generic;
using System.Text;

namespace Pump
{
    class ScheduleTime
    {


        public string TimeDiffNow(string Time)
        {
            try { 
            var datetime = Convert.ToDateTime(Time);

                var dateDiff = datetime - DateTime.Now;
                string hour = (Convert.ToInt32(Math.Floor(Convert.ToDouble(dateDiff.TotalHours.ToString())))).ToString();
                double minute = (Math.Ceiling(Convert.ToDouble(dateDiff.Minutes.ToString())));
                if (Convert.ToDouble(dateDiff.Seconds.ToString()) > 0)
                {
                    minute = minute + 1;
                }
                if(minute < 10)
                {
                    return hour + ":0" + minute;
                }
                return hour + ":" + minute;
            /*
            if (minute < 0 && hour > 0)
                {
                    hour--;
                    minute = minute + 60;

                }
                if (minute < 10)
                {
                    Time = hour + ":0" + minute;
                }
                else
                {
                    Time = hour + ":" + minute;
                }
                */
            }
            catch (Exception e)
            {
                return Time;
            }
        }
    }
}
