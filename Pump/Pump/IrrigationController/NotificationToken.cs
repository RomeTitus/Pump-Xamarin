using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class NotificationToken
    {
        public NotificationToken()
        {
        }

        [JsonIgnore]
        public string ID { get; set; }
        [JsonIgnore]
        public bool DeleteAwaiting { get; set; }
        public string Token { get; set; }

    }
}