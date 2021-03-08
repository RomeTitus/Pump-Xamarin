using System;
using Newtonsoft.Json.Linq;
using Pump.IrrigationController;

namespace Pump.SocketController
{
    internal static class SocketCommands
    {
        public static string GenerateKey(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[length];
            var random = new Random();

            for (var i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        public static JObject WiFiScan()
        {
            var wiFiScan = new JObject { { "Task", "WiFiScan" } };
            return wiFiScan;
        }

        public static JObject WiFiConnect(JObject wiFiConnect)
        {
            var wiFiCommand = new JObject{{ "Task", new JObject()}};
            wiFiCommand["Task"] = new JObject{{ "WiFiConnect", new JObject()}};
            wiFiCommand["Task"]["WiFiConnect"] = wiFiConnect;
            return wiFiCommand;
        }

        public static JObject FirebaseUid(string uid)
        {
            var wiFiCommand = new JObject { { "Task", new JObject() } };
            wiFiCommand["Task"] = new JObject { { "uid", new JObject() } };
            wiFiCommand["Task"]["uid"] = uid;
            return wiFiCommand;
        }

        public static JObject SetupSubController(SubController subController, string Id)
        {
            var createSubControllerCommand = new JObject { { "SubController", new JObject() } , { "Task", new JObject{{ "IsMaster", false }}}};
            createSubControllerCommand["SubController"] = new JObject { { Id,  JToken.FromObject(subController)}};
            return createSubControllerCommand;
        }

        public static JObject AllTogether()
        {
            var collectAllTogether = new JObject { { "Task", new JObject {{"AllTogether", true}}}};
            return collectAllTogether;
        }
    }
}