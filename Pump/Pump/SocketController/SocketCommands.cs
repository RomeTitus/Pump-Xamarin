﻿using System;
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
    }


}
