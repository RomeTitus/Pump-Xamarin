using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    public class AttachedSensor
    {
        [JsonIgnore]
        public string ID { get; set; }
        public double ThresholdLow { get; set; }
        public double ThresholdHigh { get; set; }
        public double ThresholdTimer { get; set; }
    }
}
