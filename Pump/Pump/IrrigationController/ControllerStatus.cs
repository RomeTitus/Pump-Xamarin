using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    class ControllerStatus
    {
        [JsonIgnore]
        public string ID { get; set; }
        
        public string Action {get; set;}
        
        public string Body {get; set;}

        public long LastUpdated {get; set;}
        public bool? IsComplete {get; set;}
        
        public bool IsSuccessful {get; set;}

    }
}
