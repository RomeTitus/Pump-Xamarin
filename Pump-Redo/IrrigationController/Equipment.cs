using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class Equipment : IEquipment
    {
        public string NAME { get; set; }

        public string Key { get; set; }
        public long GPIO { get; set; }
        public bool isPump { get; set; }
        public long? DirectOnlineGPIO { get; set; }
        [JsonIgnore] public string Id { get; set; }
        [JsonIgnore] public bool HasUpdated { get; set; }
        public bool DeleteAwaiting { get; set; }
        public string AttachedSubController { get; set; }
        public ControllerStatus ControllerStatus { get; set;  }
    }
}