using System;
using System.Collections.Generic;
using System.Text;

namespace Pump.Class
{
    public class ControllerEventArgs: EventArgs
    {
        public string Status { get; private set; }

        public ControllerEventArgs(string status)
        {
            Status = status;
        }
    }
}
