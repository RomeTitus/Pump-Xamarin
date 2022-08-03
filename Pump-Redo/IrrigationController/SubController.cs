using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class SubController : ISubController
    {
        public string Name { get; set; }
        public string Mac { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public int IncomingKey { get; set; }
        public List<int> OutgoingKey { get; set; }
        public bool UseLoRa { get; set; }
        [JsonIgnore] public string Id { get; set; }
        public ControllerStatus ControllerStatus { get; set; }
        [JsonIgnore] public bool HasUpdated { get; set; }
        public bool DeleteAwaiting { get; set; }
        
        [JsonIgnore] public string Key { get; set; }
    }
}