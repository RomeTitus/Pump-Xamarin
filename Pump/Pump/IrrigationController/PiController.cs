﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Pump.IrrigationController
{
    public class PiController
    {
        public string ID { get; set; }
        public string NAME { get; set; }
        public string BTmac { get; set; }
        public string IpAdress { get; set; }
        public int Port { get; set; }
    }
}
