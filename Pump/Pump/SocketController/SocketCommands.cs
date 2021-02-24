using System;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pump.IrrigationController;
using Xamarin.Essentials;

namespace Pump.SocketController
{
    internal static class SocketCommands
    {
        public static string WiFiScan = "{\"Task\" : \"WiFiScan\"}";

        public static string WiFiConnect(JObject wiFiConnect)
        {
            var wiFiCommand = new JObject{{ "Task", new JObject()}};
            wiFiCommand["Task"] = new JObject{{ "WiFiConnect", new JObject()}};
            wiFiCommand["Task"]["WiFiConnect"] = wiFiConnect;
            return wiFiCommand.ToString();
        }

        public static string FirebaseUid(string uid)
        {
            var wiFiCommand = new JObject { { "Task", new JObject() } };
            wiFiCommand["Task"] = new JObject { { "uid", new JObject() } };
            wiFiCommand["Task"]["uid"] = uid;
            return wiFiCommand.ToString();
        }

        public static string SetupSubController(SubController subController, string Id)
        {
            var createSubControllerCommand = new JObject { { "SubController", new JObject() } };
            createSubControllerCommand["SubController"] = new JObject { { Id,  JToken.FromObject(subController)}};
            return createSubControllerCommand.ToString();
        }

        public static string AllTogether()
        {
            var collectAllTogether = new JObject { { "Task", "AllTogether" } };
            return collectAllTogether.ToString();
        }
    }
}