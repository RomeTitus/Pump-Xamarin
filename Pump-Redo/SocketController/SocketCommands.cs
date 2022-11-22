using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using Pump.Class;
using Pump.Database.Table;
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
            var setupControllerCommand = new JObject { { "Task", new JObject() } };
            setupControllerCommand["Task"] = new JObject { { "ControllerAuth", new JObject() } };
            setupControllerCommand["Task"]["ControllerAuth"] = controllerConfig;
            return setupControllerCommand;
        }

        private static JObject PairSubController(JObject pairSub , IrrigationConfiguration irrigationConfiguration, int key)
        {
            pairSub["Task"]["SubPair"]["LoRaConfig"] = "SetConfig" + "," + key + "," +
                                                       irrigationConfiguration.Freq + "," +
                                                       irrigationConfiguration.Power + "," +
                                                       irrigationConfiguration.Modem;
            
            pairSub["Task"]["SubPair"]["UseLoRa"] = true;
            return pairSub;
        }
        
        public static JObject PairSubController(IrrigationConfiguration irrigationConfiguration, string name, List<int> keyPath, bool pairWithLoRa)
        {
            var pairSub =  new JObject { { "Task", new JObject() } };

            pairSub["Task"]["SubPair"] = new JObject();
                
            pairSub["Task"]["SubPair"]["Name"] = name;
            pairSub["Task"]["SubPair"]["KeyPath"] = JToken.FromObject(keyPath); //Its Address to Parent Address :) Will always have 2 or more keys Sub --> Main
            pairSub["Task"]["SubPair"]["UseLoRa"] = false;

            if (string.IsNullOrEmpty(irrigationConfiguration.InternalPath) == false)
            {
                pairSub["Task"]["SubPair"]["AddressPath"] = irrigationConfiguration.InternalPath;
            }

            if (pairWithLoRa)
                PairSubController(pairSub,irrigationConfiguration, keyPath.First());
            
            return pairSub;
        }

        public static JObject GetLoRaConfig()
        {
            var config = new JObject { { "Task", "GetConfig" } };
            return config;
        }
        
        public static JObject SetLoRaConfig((int address, double freq, int power, int modem) loRaConfig)
        {
            var config = new JObject { { "Task", "SetConfig" + "," + loRaConfig.address + "," +  loRaConfig.freq.ToString("0.0",CultureInfo.InvariantCulture) + "," + loRaConfig.power + "," +  loRaConfig.modem} };
            return config;
        }
        
        public static JObject AllTogether()
        {
            var collectAllTogether = new JObject { { "Task", new JObject { { "AllTogether", true } } } };
            return collectAllTogether;
        }

        private static JObject DeleteManualSchedule(ManualSchedule manualSchedule)
        {
            var deleteManualScheduleCommand = new JObject { { nameof(ManualSchedule), new JObject() } };
            deleteManualScheduleCommand[nameof(ManualSchedule)] = new JObject { { manualSchedule.Id, new JObject() } };
            return deleteManualScheduleCommand;
        }

        private static JObject SetManualSchedule(ManualSchedule manualSchedule)
        {
            if (manualSchedule.Id == null)
                manualSchedule.Id = GenerateKey(20);
            var setManualScheduleCommand = new JObject { { nameof(ManualSchedule), new JObject() } };
            setManualScheduleCommand[nameof(ManualSchedule)] = new JObject
                { { manualSchedule.Id, JToken.FromObject(manualSchedule) } };
            return setManualScheduleCommand;
        }

        private static JObject DeleteSchedule(Schedule schedule)
        {
            var deleteScheduleCommand = new JObject { { nameof(Schedule), new JObject() } };
            deleteScheduleCommand[nameof(Schedule)] = new JObject { { schedule.Id, new JObject() } };
            return deleteScheduleCommand;
        }

        private static JObject SetSchedule(Schedule schedule)
        {
            if (schedule.Id == null)
                schedule.Id = GenerateKey(20);
            var setScheduleCommand = new JObject { { nameof(Schedule), new JObject() } };
            setScheduleCommand[nameof(Schedule)] = new JObject { { schedule.Id, JToken.FromObject(schedule) } };
            return setScheduleCommand;
        }

        private static JObject DeleteCustomSchedule(CustomSchedule customSchedule)
        {
            var deleteCustomScheduleCommand = new JObject { { nameof(CustomSchedule), new JObject() } };
            deleteCustomScheduleCommand[nameof(CustomSchedule)] = new JObject { { customSchedule.Id, new JObject() } };
            return deleteCustomScheduleCommand;
        }

        private static JObject SetCustomSchedule(CustomSchedule customSchedule)
        {
            if (customSchedule.Id == null)
                customSchedule.Id = GenerateKey(20);
            var setCustomScheduleCommand = new JObject { { nameof(CustomSchedule), new JObject() } };
            setCustomScheduleCommand[nameof(CustomSchedule)] = new JObject
                { { customSchedule.Id, JToken.FromObject(customSchedule) } };
            return setCustomScheduleCommand;
        }

        private static JObject DeleteEquipment(Equipment equipment)
        {
            var deleteEquipmentCommand = new JObject { { nameof(Equipment), new JObject() } };
            deleteEquipmentCommand[nameof(Equipment)] = new JObject { { equipment.Id, new JObject() } };
            return deleteEquipmentCommand;
        }

        private static JObject SetEquipment(Equipment equipment)
        {
            if (equipment.Id == null)
                equipment.Id = GenerateKey(20);
            var setEquipmentCommand = new JObject { { nameof(Equipment), new JObject() } };
            setEquipmentCommand[nameof(Equipment)] = new JObject { { equipment.Id, JToken.FromObject(equipment) } };
            return setEquipmentCommand;
        }

        private static JObject DeleteSensor(Sensor sensor)
        {
            var deleteSensorCommand = new JObject { { nameof(Sensor), new JObject() } };
            deleteSensorCommand[nameof(Sensor)] = new JObject { { sensor.Id, new JObject() } };
            return deleteSensorCommand;
        }

        private static JObject SetSensor(Sensor sensor)
        {
            if (sensor.Id == null)
                sensor.Id = GenerateKey(20);
            var setSensorCommand = new JObject { { nameof(Sensor), new JObject() } };
            setSensorCommand[nameof(Sensor)] = new JObject { { sensor.Id, JToken.FromObject(sensor) } };
            return setSensorCommand;
        }

        private static JObject DeleteSubController(SubController subController)
        {
            var deleteSubControllerCommand = new JObject { { nameof(SubController), new JObject() } };
            deleteSubControllerCommand[nameof(SubController)] = new JObject { { subController.Id, new JObject() } };
            return deleteSubControllerCommand;
        }

        private static JObject SetSubController(SubController subController)
        {
            if (subController.Id == null)
                subController.Id = GenerateKey(20);
            var setSubControllerCommand = new JObject { { nameof(SubController), new JObject() } };
            setSubControllerCommand[nameof(SubController)] = new JObject
                { { subController.Id, JToken.FromObject(subController) } };
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