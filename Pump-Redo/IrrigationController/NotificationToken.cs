using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class NotificationToken
    {
        [JsonIgnore] public string Id { get; set; }

        [JsonIgnore] public bool DeleteAwaiting { get; set; }

        public string Token { get; set; }
    }
}