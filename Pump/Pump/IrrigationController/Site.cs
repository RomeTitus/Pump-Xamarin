﻿using System.Collections.Generic;

namespace Pump.IrrigationController
{
    public class Site
    {
        public Site()
        {
            Attachments = new List<string>();
        }
        public string ID { get; set; }
        public string NAME { get; set; }
        public string Description { get; set; }
        public List<string> Attachments { get; set; }

    }
}