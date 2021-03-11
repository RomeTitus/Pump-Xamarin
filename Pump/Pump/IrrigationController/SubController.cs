using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class SubController
    {
        [JsonIgnore]
        public string ID { get; set; }
        public string NAME { get; set; }
        public string BTmac { get; set; }
        public string IpAdress { get; set; }
        public int Port { get; set; }
        public int Key { get; set; }
        public bool UseLoRa { get; set; }
    }
}
