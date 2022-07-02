using System.Collections.Generic;
using Newtonsoft.Json;
using SQLite;

namespace Pump.Database.Table
{
    public class IrrigationConfiguration
    {

        public IrrigationConfiguration()
        {
        }

        [PrimaryKey] [AutoIncrement] public int Id { get; set; }
        public string Path { get; set; }
        public string Mac { get; set; }
        public int ConnectionType { get; set; }
        public string InternalPath { get; set; }
        public int? InternalPort { get; set; }
        public string ExternalPath { get; set; }
        public int? ExternalPort { get; set; }
        
        [Ignore]
        public Dictionary<string, List<string>> ControllerPairs { get; set; }

        private string ControllerPairsSerialized { get; set; }

        public void SerializedControllerPair()
        {
            ControllerPairsSerialized = ControllerPairsSerialized == null? string.Empty : JsonConvert.SerializeObject(ControllerPairs);
        }
        public void DeserializedControllerPair()
        {
            ControllerPairs = ControllerPairsSerialized == null? new Dictionary<string, List<string>>() : JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(ControllerPairsSerialized);
        }
    }
}