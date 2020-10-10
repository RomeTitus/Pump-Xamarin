using System;
using System.Globalization;

namespace Pump.Class
{
    internal class ScheduleTime
    {
        public static string TimeDiffNow(string time)
        {
            try
            {

                var datetime = Convert.ToDateTime(time);

                var dateDiff = datetime - DateTime.Now;
                var hour = Convert.ToInt32(Math.Floor(Convert.ToDouble(dateDiff.TotalHours.ToString(CultureInfo.InvariantCulture))));
                var minute = Math.Ceiling(Convert.ToDouble(dateDiff.Minutes.ToString()));
                if (Convert.ToDouble(dateDiff.Seconds.ToString()) > 0) minute += 1;
                var stringHour = hour.ToString();
                var stringMinute = minute.ToString(CultureInfo.InvariantCulture);

                if (minute < 10)
                    stringMinute = "0" + minute;

                if (hour < 10)
                    stringHour = "0" + hour;

                return stringHour + ":" + stringMinute;
            }
            catch (Exception)
            {
                return time;
            }
        }

        public static string ConvertTimeSpanToString(TimeSpan timeSpan)
        {
            string hour;
            if (timeSpan.Hours > 9)
                hour = timeSpan.Hours.ToString();
            else
                hour = "0" + timeSpan.Hours;
            string minute;
            if (timeSpan.Minutes > 9)
                minute = timeSpan.Minutes.ToString();
            else
                minute = "0" + timeSpan.Minutes;
            return hour + ":" + minute;
        }

        public static DateTime FromUnixTimeStampLocal(long unixTimeStamp)
        {
            return (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(unixTimeStamp);
        }
        public static DateTime FromUnixTimeStampUtc(long unixTimeStamp)
        {
            return (new DateTime(1970, 1, 1, 0, 0, 0, 0)).AddSeconds(unixTimeStamp);
        }

        public int getUnixTimeStampNow()
        {
            return (int)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        public static int GetUnixTimeStampUtcNow()
        {
            return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        public static int GetUnixTimeStampUtcNow(TimeSpan hours, TimeSpan minutes)
        {
            return (int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).Add(hours).Add(minutes).TotalSeconds;

        }
    }
}