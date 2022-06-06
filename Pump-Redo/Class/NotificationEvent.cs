namespace Pump.Class
{
    public class NotificationEvent
    {
        public delegate void StatusUpdateHandler(object sender, ControllerEventArgs e);

        public event StatusUpdateHandler OnUpdateStatus;

        public void UpdateStatus(string status = null)
        {
            // Make sure someone is listening to event
            if (OnUpdateStatus == null) return;

            var args = new ControllerEventArgs(status);
            OnUpdateStatus(this, args);
        }
    }
}