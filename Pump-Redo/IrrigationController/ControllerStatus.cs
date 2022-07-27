using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class ControllerStatus
    {
        public bool Failed { get; set; }
        public bool Complete { get; set;}
        public List<string> Steps { get; set;}
        [JsonIgnore] public string EntityType { get; set; }
        [JsonIgnore] public string Id { get; set; }
    }
}