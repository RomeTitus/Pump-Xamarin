using System;

namespace Pump.Class
{
    public class NotificationEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string ControllerName { get; set; }
    }
}