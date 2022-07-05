using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class Sensor : IEntity, IEquipment
    {
        public Sensor()
        {
            AttachedEquipment = new List<AttachedSensor>();
        }

        [JsonIgnore] public string Id { get; set; }

        [JsonIgnore] public bool DeleteAwaiting { get; set; }

        public string LastReading { get; set; }
        
        public string Key { get; set; }
        public string NAME { get; set; }
        public string TYPE { get; set; }
        public long GPIO { get; set; }
        public string AttachedSubController { get; set; }
        public long? LastUpdated { get; set; }
        public List<AttachedSensor> AttachedEquipment { get; set; }
    }
}