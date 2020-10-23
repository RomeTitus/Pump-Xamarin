using System;
using System.Collections.Generic;

namespace Pump.IrrigationController
{
    public class Equipment
    {
        public Equipment()
        {
            AttachedSensor = new List<AttachedSensor>();
        }
        public string ID { get; set; }
        public string AttachedSubController { get; set; }
        public List<AttachedSensor> AttachedSensor { get; set; }
        public string NAME { get; set; }
        public long GPIO { get; set; }
        public bool isPump { get; set; }
        public long? DirectOnlineGPIO { get; set; }
    }
}