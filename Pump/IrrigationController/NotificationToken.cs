using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class NotificationToken : IEntity
    {
        [JsonIgnore] public string Id { get; set; }

        public bool DeleteAwaiting { get; set; }

        public string Token { get; set; }
    }
}