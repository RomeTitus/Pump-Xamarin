using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    class ControllerStatus
    {
        public ControllerStatus(string notification)
        {
            var notificationList = notification.Split('$');
            if (notificationList.Length == 1)
            {
                Code = notificationList[0];
                Operation = "Not supplied";
                Accept = null;

            }
            else if (notificationList.Length == 2)
            {
                Code = notificationList[0];
                Operation = notificationList[1];
                Accept = null;
            }
            else if (notificationList.Length == 3)
            {
                Code = notificationList[0];
                Operation = notificationList[1];
                Accept = notificationList[2];
            }
            else
            {
                Code = "Not supplied";
                Operation = "Not supplied";
                Accept = null;
            }
        }
        [JsonIgnore]
        public string ID { get; set; }
        [JsonIgnore]
        public string Accept { get; set; }
        public string Code { get; set; }
        public string Operation { get; set; }

        
    }
}
