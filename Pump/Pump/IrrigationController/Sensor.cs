using System;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class Sensor
    {
        public Sensor()
        {
        }
        [JsonIgnore]
        public string ID { get; set; }
        [JsonIgnore]
        public bool DeleteAwaiting { get; set; }
        public string LastReading { get; set; }
        public string NAME { get; set; }
        public string TYPE { get; set; }
        public long GPIO { get; set; }
        public string AttachedSubController { get; set; }



       
    }
}