using System;

namespace Pump.Notification
{
    public interface INotificationManager
    {
        event EventHandler NotificationReceived;
        void SendNotification(string title, string message, string BTmac, DateTime? notifyTime = null);
        void ReceiveNotification(string title, string message, string controllerName);
    }
}