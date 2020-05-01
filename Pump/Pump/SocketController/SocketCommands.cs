using System;
using System.Collections.Generic;
using System.Text;

namespace Pump.SocketController
{
    class SocketCommands
    {
        public string getActiveSchedule()
        {
            return "getActiveSchedule";
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
    }


}
