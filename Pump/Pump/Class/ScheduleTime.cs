using System;
using System.Globalization;

namespace Pump.Class
{
    internal class ScheduleTime
    {
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