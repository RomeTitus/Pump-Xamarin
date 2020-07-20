using System;
using System.Collections.Generic;
using System.Text;

namespace Pump.IrrigationController
{
    class Equipment
    {
        public string ID { get; set; }
        public string AttachedPiController { get; set; }
        public string NAME { get; set; }
        public string GPIO { get; set; }
        public bool isPump { get; set; }
        public string DirectOnlineGPIO { get; set;}
    }
}
