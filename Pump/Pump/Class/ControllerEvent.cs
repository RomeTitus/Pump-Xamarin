using System;
using System.Collections.Generic;
using System.Text;

namespace Pump.Class
{
    public class ControllerEvent
    {
        public delegate void StatusUpdateHandler(object sender, ControllerEventArgs e);
        public event StatusUpdateHandler OnUpdateStatus;

        public void NewControllerConnection()
        {
            // sending blank for test
            UpdateStatus("");
        }

        private void UpdateStatus(string status)
        {
            // Make sure someone is listening to event
            if (OnUpdateStatus == null) return;

            var args = new ControllerEventArgs(status);
            OnUpdateStatus(this, args);
        }
    }
}
