using Newtonsoft.Json.Linq;
using Pump.IrrigationController;

namespace Pump.SocketController
{
    internal static class SocketCommands
    {
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
            var collectAllTogether = new JObject { { "Task", new JObject {{"AllTogether", true}, { "PartitionNumber", 0 } } }};
            return collectAllTogether;
        }
    }
}