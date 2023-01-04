using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class SubController : ISubController
    {
        public string Name { get; set; }
        public string DeviceGuid { get; set; }
        public List<int> KeyPath { get; set; }
        public string AddressPath { get; set; }
        public bool UseLoRa { get; set; }
        [JsonIgnore] public string Id { get; set; }
        public ControllerStatus ControllerStatus { get; set; }
        [JsonIgnore] public bool HasUpdated { get; set; }
        public bool DeleteAwaiting { get; set; }
    }
}