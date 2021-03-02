using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Pump.IrrigationController;

namespace Pump.Class
{
    public class NotificationEventArgs
    {
        public string Header { get; private set; }
        public string Main { get; private set; }
        public string ButtonText { get; private set; }

        public NotificationEventArgs(string header, string main, string buttonText)
        {
            Header = header;
            Main = main;
            ButtonText = buttonText;
        }
    }

}
