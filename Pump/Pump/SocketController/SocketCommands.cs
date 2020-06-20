using Xamarin.Essentials;

namespace Pump.SocketController
{
    class SocketCommands
    {
        public string getActiveSchedule()
        {
            return "getActiveSchedule";
        }
        public string getScheduleInfo(int id)
        {
            return id +"$getScheduleInfo";
        }

        public string getSchedule()
        {
            return "getSchedule";
        }

        public string addSchedule(string schedule)
        {
            return schedule + "$SCHEDULE";
        }
        public string updateSchedule(int id, string schedule)
        {
            return id + "#" +schedule + "$EDITSCHEDULE";
        }
        public string deleteSchedule(int id)
        {
            return id + "$DeleteSchedule";
        }
        public string getMacAddress()
        {
            return "getMAC";
        }

        public string getQueueSchedule()
        {
            return "getQueueSchedule";
        }

        public string getActiveSensorStatus()
        {
            return "getActiveSensorStatus";
        }

        public string getPumps()
        {
            return "getPumps";
        }

        public string getValves()
        {
            return "getValves";
        }

        public string getManualSchedule()
        {
            return "getManualSchedule";
        }
        

        public string StopManualSchedule()
        {
            return "StopManualSchedule";
        }

        public string ChangeSchedule(int id, int isActive)
        {
            return id + ","+ isActive + "$ChangeSchedule";
        }

        public string getEquipmentLastUsed()
        {
            return "getEquipmentLastUsedHistoryReading";
        }

        public string setToken(string token)
        {
            var deviceId = Preferences.Get("my_deviceId", string.Empty);
            if (string.IsNullOrEmpty(deviceId))
            {
                deviceId = System.Guid.NewGuid().ToString();
                Preferences.Set("my_deviceId", deviceId);
            }
            return deviceId + "," + token + "$setToken";
        }
    }


}
