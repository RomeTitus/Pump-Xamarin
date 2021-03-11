using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Pump.IrrigationController
{
    class ControllerStatus
    {
        [JsonIgnore]
        public string ID { get; set; }
        public string Code { get; set; }
        public string Operation { get; set; }
    }
}
