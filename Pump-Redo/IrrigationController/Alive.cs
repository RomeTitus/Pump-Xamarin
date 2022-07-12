using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class Alive : IEntity
    {
        public long RequestedTime { get; set; }
        public long ResponseTime { get; set; }

        [JsonIgnore] public string Id { get; set; }

        [JsonIgnore] public bool DeleteAwaiting { get; set; }
    }
}