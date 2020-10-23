using System;

namespace Pump.IrrigationController
{
    public class Sensor
    {
        public Sensor()
        {
        }
        public string ID { get; set; }
        public string LastReading { get; set; }
        public string NAME { get; set; }
        public string TYPE { get; set; }
        public long GPIO { get; set; }
        public string AttachedSubController { get; set; }



       
    }
}