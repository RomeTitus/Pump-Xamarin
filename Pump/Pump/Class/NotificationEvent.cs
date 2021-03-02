using System;
using System.Collections.Generic;
using System.Text;

namespace Pump.Class
{
    public class NotificationEvent
    {
        public delegate void NotificationUpdateHandler(object sender, NotificationEventArgs e);
        public event NotificationUpdateHandler OnNotificationUpdate;

        public void Notification(string header, string main, string buttonText)
        {
            // Make sure someone is listening to event
            if (OnNotificationUpdate == null) return;

            var args = new NotificationEventArgs(header, main, buttonText);
            OnNotificationUpdate(this, args);
        }
    }
}
