using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class SubController : IEntity
    {
        [JsonIgnore] public string Id { get; set; }

        [JsonIgnore] public bool DeleteAwaiting { get; set; }
        
        public string Name { get; set; }
        public string Mac { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int IncomingKey { get; set; }
        public List<int> OutgoingKey { get; set; }
        public bool UseLoRa { get; set; }
    }
}