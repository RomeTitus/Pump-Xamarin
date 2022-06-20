using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using Pump.Class;
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

            for (var i = 0; i < stringChars.Length; i++) stringChars[i] = chars[random.Next(chars.Length)];

            return new string(stringChars);
        }

        public static JObject WiFiScan()
        {
            var wiFiScan = new JObject { { "Task", "WiFiScan" } };
            return wiFiScan;
        }

        public static JObject ConnectionInfo()
        {
            var wiFiScan = new JObject { { "Task", "ConnectionInfo" } };
            return wiFiScan;
        }

        public static JObject WiFiConnect(JObject wiFiConnect)
        {
            var wiFiCommand = new JObject { { "Task", new JObject() } };
            wiFiCommand["Task"] = new JObject { { "WiFiConnect", new JObject() } };
            wiFiCommand["Task"]["WiFiConnect"] = wiFiConnect;
            return wiFiCommand;
        }

        public static JObject TempDhcpConfig(JObject dhcpConfig)
        {
            var tempDhcpConfigCommand = new JObject { { "Task", new JObject() } };
            tempDhcpConfigCommand["Task"] = new JObject { { "TempDhcpConfig", new JObject() } };
            tempDhcpConfigCommand["Task"]["TempDhcpConfig"] = dhcpConfig;
            return tempDhcpConfigCommand;
        }
        
        public static JObject SetupFirebaseController(JObject controllerConfig)
        {
            var wiFiCommand = new JObject { { "Task", new JObject() } };
            wiFiCommand["Task"] = new JObject { { "controllerConfig", new JObject() } };
            wiFiCommand["Task"]["controllerConfig"] = controllerConfig;
            return wiFiCommand;
        }

        public static JObject SetupSubController(SubController subController, string id)
        {
            var createSubControllerCommand = new JObject
                { { "SubController", new JObject() }, { "Task", new JObject { { "IsMaster", false } } } };
            createSubControllerCommand["SubController"] = new JObject { { id, JToken.FromObject(subController) } };
            return createSubControllerCommand;
        }

        public static JObject AllTogether()
        {
            var collectAllTogether = new JObject { { "Task", new JObject { { "AllTogether", true } } } };
            return collectAllTogether;
        }

        private static JObject DeleteManualSchedule(ManualSchedule manualSchedule)
        {
            var deleteManualScheduleCommand = new JObject { { nameof(ManualSchedule), new JObject() } };
            deleteManualScheduleCommand[nameof(ManualSchedule)] = new JObject { { manualSchedule.ID, new JObject() } };
            return deleteManualScheduleCommand;
        }

        private static JObject SetManualSchedule(ManualSchedule manualSchedule)
        {
            if (manualSchedule.ID == null)
                manualSchedule.ID = GenerateKey(20);
            var setManualScheduleCommand = new JObject { { nameof(ManualSchedule), new JObject() } };
            setManualScheduleCommand[nameof(ManualSchedule)] = new JObject
                { { manualSchedule.ID, JToken.FromObject(manualSchedule) } };
            return setManualScheduleCommand;
        }

        private static JObject DeleteSchedule(Schedule schedule)
        {
            var deleteScheduleCommand = new JObject { { nameof(Schedule), new JObject() } };
            deleteScheduleCommand[nameof(Schedule)] = new JObject { { schedule.ID, new JObject() } };
            return deleteScheduleCommand;
        }

        private static JObject SetSchedule(Schedule schedule)
        {
            if (schedule.ID == null)
                schedule.ID = GenerateKey(20);
            var setScheduleCommand = new JObject { { nameof(Schedule), new JObject() } };
            setScheduleCommand[nameof(Schedule)] = new JObject { { schedule.ID, JToken.FromObject(schedule) } };
            return setScheduleCommand;
        }

        private static JObject DeleteCustomSchedule(CustomSchedule customSchedule)
        {
            var deleteCustomScheduleCommand = new JObject { { nameof(CustomSchedule), new JObject() } };
            deleteCustomScheduleCommand[nameof(CustomSchedule)] = new JObject { { customSchedule.ID, new JObject() } };
            return deleteCustomScheduleCommand;
        }

        private static JObject SetCustomSchedule(CustomSchedule customSchedule)
        {
            if (customSchedule.ID == null)
                customSchedule.ID = GenerateKey(20);
            var setCustomScheduleCommand = new JObject { { nameof(CustomSchedule), new JObject() } };
            setCustomScheduleCommand[nameof(CustomSchedule)] = new JObject
                { { customSchedule.ID, JToken.FromObject(customSchedule) } };
            return setCustomScheduleCommand;
        }

        private static JObject DeleteEquipment(Equipment equipment)
        {
            var deleteEquipmentCommand = new JObject { { nameof(Equipment), new JObject() } };
            deleteEquipmentCommand[nameof(Equipment)] = new JObject { { equipment.ID, new JObject() } };
            return deleteEquipmentCommand;
        }

        private static JObject SetEquipment(Equipment equipment)
        {
            if (equipment.ID == null)
                equipment.ID = GenerateKey(20);
            var setEquipmentCommand = new JObject { { nameof(Equipment), new JObject() } };
            setEquipmentCommand[nameof(Equipment)] = new JObject { { equipment.ID, JToken.FromObject(equipment) } };
            return setEquipmentCommand;
        }

        private static JObject DeleteSensor(Sensor sensor)
        {
            var deleteSensorCommand = new JObject { { nameof(Sensor), new JObject() } };
            deleteSensorCommand[nameof(Sensor)] = new JObject { { sensor.ID, new JObject() } };
            return deleteSensorCommand;
        }

        private static JObject SetSensor(Sensor sensor)
        {
            if (sensor.ID == null)
                sensor.ID = GenerateKey(20);
            var setSensorCommand = new JObject { { nameof(Sensor), new JObject() } };
            setSensorCommand[nameof(Sensor)] = new JObject { { sensor.ID, JToken.FromObject(sensor) } };
            return setSensorCommand;
        }

        private static JObject DeleteSite(Site site)
        {
            var deleteSiteCommand = new JObject { { nameof(Site), new JObject() } };
            deleteSiteCommand[nameof(Site)] = new JObject { { site.ID, new JObject() } };
            return deleteSiteCommand;
        }

        private static JObject SetSite(Site site)
        {
            if (site.ID == null)
                site.ID = GenerateKey(20);
            var setSiteCommand = new JObject { { nameof(Site), new JObject() } };
            setSiteCommand[nameof(Site)] = new JObject { { site.ID, JToken.FromObject(site) } };
            return setSiteCommand;
        }

        private static JObject DeleteSubController(SubController subController)
        {
            var deleteSubControllerCommand = new JObject { { nameof(SubController), new JObject() } };
            deleteSubControllerCommand[nameof(SubController)] = new JObject { { subController.ID, new JObject() } };
            return deleteSubControllerCommand;
        }

        private static JObject SetSubController(SubController subController)
        {
            if (subController.ID == null)
                subController.ID = GenerateKey(20);
            var setSubControllerCommand = new JObject { { nameof(SubController), new JObject() } };
            setSubControllerCommand[nameof(SubController)] = new JObject
                { { subController.ID, JToken.FromObject(subController) } };
            return setSubControllerCommand;
        }

        public static JObject Descript(object entity)
        {
            if (entity.GetType() == typeof(ManualSchedule))
            {
                var manualSchedule = (ManualSchedule)entity;
                return CleanJObject(manualSchedule.DeleteAwaiting
                    ? DeleteManualSchedule(manualSchedule)
                    : SetManualSchedule(manualSchedule));
            }

            if (entity.GetType() == typeof(Schedule))
            {
                var schedule = (Schedule)entity;
                return CleanJObject(schedule.DeleteAwaiting ? DeleteSchedule(schedule) : SetSchedule(schedule));
            }

            if (entity.GetType() == typeof(CustomSchedule))
            {
                var customSchedule = (CustomSchedule)entity;
                return CleanJObject(customSchedule.DeleteAwaiting
                    ? DeleteCustomSchedule(customSchedule)
                    : SetCustomSchedule(customSchedule));
            }

            if (entity.GetType() == typeof(Equipment))
            {
                var equipment = (Equipment)entity;
                return CleanJObject(equipment.DeleteAwaiting ? DeleteEquipment(equipment) : SetEquipment(equipment));
            }

            if (entity.GetType() == typeof(Sensor))
            {
                var sensor = (Sensor)entity;
                return CleanJObject(sensor.DeleteAwaiting ? DeleteSensor(sensor) : SetSensor(sensor));
            }

            if (entity.GetType() == typeof(Site))
            {
                var site = (Site)entity;
                return CleanJObject(site.DeleteAwaiting ? DeleteSite(site) : SetSite(site));
            }

            if (entity.GetType() == typeof(SubController))
            {
                var subController = (SubController)entity;
                return CleanJObject(subController.DeleteAwaiting
                    ? DeleteSubController(subController)
                    : SetSubController(subController));
            }

            //TODO see how this will work with BlueTooth / Socket
            if (entity.GetType() == typeof(Alive))
            {
                var alive = (Alive)entity;
                return CleanJObject((JObject)ScheduleTime.GetUnixTimeStampUtcNow());
            }

            return null;
        }

        private static JObject CleanJObject(JObject jObject)
        {
            foreach (var type in jObject)
            {
                var typeKey = jObject[type.Key];
                foreach (var jToken in typeKey.First.First.ToList())
                {
                    var properties = (JProperty)jToken;
                    var result = properties.Value.ToString();
                    if (string.IsNullOrEmpty(result) || result == "[]") jToken.Remove();
                }
            }

            return jObject;
        }
    }
}