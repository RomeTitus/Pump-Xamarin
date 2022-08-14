using System.Collections.Generic;
namespace Pump.IrrigationController
{
    public class ControllerStatus
    {
        public bool Failed { get; set; }
        public bool Complete { get; set;}
        
        public long? LastUpdated { get; set; }
        public List<string> Steps { get; set;}
    }
}