using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class Equipment
    {
        [JsonIgnore] public string ID { get; set; }

        [JsonIgnore] public bool DeleteAwaiting { get; set; }

        public string AttachedSubController { get; set; }
        public string NAME { get; set; }

        public string Key { get; set; }
        public long GPIO { get; set; }
        public bool isPump { get; set; }
        public long? DirectOnlineGPIO { get; set; }
    }
}