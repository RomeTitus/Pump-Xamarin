using System;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public static string FirebaseUID(string uid)
        {
            var wiFiCommand = new JObject { { "Task", new JObject() } };
            wiFiCommand["Task"] = new JObject { { "uid", new JObject() } };
            wiFiCommand["Task"]["uid"] = uid;
            return wiFiCommand.ToString();
        }
    }
}