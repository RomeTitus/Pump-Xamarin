using System;

namespace Pump
{
    internal class ScheduleTime
    {
        public string TimeDiffNow(string Time)
        {
            try
            {
                var datetime = Convert.ToDateTime(Time);

                var dateDiff = datetime - DateTime.Now;
                var hour = Convert.ToInt32(Math.Floor(Convert.ToDouble(dateDiff.TotalHours.ToString())));
                var minute = Math.Ceiling(Convert.ToDouble(dateDiff.Minutes.ToString()));
                if (Convert.ToDouble(dateDiff.Seconds.ToString()) > 0) minute = minute + 1;
                var StringHour = hour.ToString();
                var StringMinute = minute.ToString();

                if (minute < 10)
                    StringMinute = "0" + minute;

                if (hour < 10)
                    StringHour = "0" + hour;

                return StringHour + ":" + StringMinute;
            }
            catch (Exception e)
            {
                return Time;
            }
        }
    }
}