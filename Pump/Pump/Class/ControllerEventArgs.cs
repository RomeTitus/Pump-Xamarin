using System;

namespace Pump.Class
{
    public class ControllerEventArgs : EventArgs
    {
        public ControllerEventArgs(string status)
        {
            Status = status;
        }

        public string Status { get; private set; }
    }
}