using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class SubController
    {
        [JsonIgnore] public string ID { get; set; }

        [JsonIgnore] public bool DeleteAwaiting { get; set; }

        public string NAME { get; set; }
        public string BTmac { get; set; }
        public string IpAdress { get; set; }
        public int Port { get; set; }
        public int IncomingKey { get; set; }
        public List<int> OutgoingKey { get; set; }
        public bool UseLoRa { get; set; }
    }
}