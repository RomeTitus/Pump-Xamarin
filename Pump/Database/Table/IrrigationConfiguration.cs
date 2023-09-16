﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Pump.Class;
using SQLite;

namespace Pump.Database.Table
{
    public class IrrigationConfiguration
    {
        [PrimaryKey] [AutoIncrement] public int Id { get; set; }
        public short Address { get; set; }
        public string Path { get; set; }
        public string DeviceGuid { get; set; }
        public int ConnectionType { get; set; }
        public string InternalPath { get; set; }
        public string ExternalPath { get; set; }
        [Ignore] public LoRaNodeInfo LoRaNode { get; set; }
        [Ignore] public Dictionary<string, List<string>> ControllerPairs { get; set; }

        //Needs to be public else it wont save :(
        public string ControllerPairsSerialized { get; set; }
        
        public string LoRaNodeSerialized { get; set; }

        public void SerializedProperties()
        {
            ControllerPairsSerialized =
                ControllerPairs == null ? string.Empty : JsonConvert.SerializeObject(ControllerPairs);
            LoRaNodeSerialized =
                ControllerPairs == null ? string.Empty : JsonConvert.SerializeObject(LoRaNode);
        }

        public void DeserializedProperties()
        {
            ControllerPairs = string.IsNullOrEmpty(ControllerPairsSerialized) 
                ? new Dictionary<string, List<string>>()
                : JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(ControllerPairsSerialized);

            if (ControllerPairs.Any() == false && Path != null)
            {
                ControllerPairs = new Dictionary<string, List<string>> { { Path, null } };
            }
            
            LoRaNode = string.IsNullOrEmpty(LoRaNodeSerialized)
                ? null
                : JsonConvert.DeserializeObject<LoRaNodeInfo>(LoRaNodeSerialized);
        }
    }
}