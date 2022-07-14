using System.Collections.Generic;

namespace Pump.IrrigationController
{
    public class ControllerStatus
    {
        public bool Failed { get; }
        public bool? Complete { get; }
        public List<string> Steps { get; }
    }
}