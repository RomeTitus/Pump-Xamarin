using System;
using SQLite;

namespace Pump.Database.Table
{
    public class IrrigationConfiguration
    {

        public IrrigationConfiguration()
        {
        }

        [PrimaryKey] [AutoIncrement] public int ID { get; set; }
        public string Mac { get; set; }
        public Guid IDeviceGuid { get; set; }
        public int ConnectionType { get; set; }
        public string InternalPath { get; set; }
        public int? InternalPort { get; set; }
        public string ExternalPath { get; set; }
        public int? ExternalPort { get; set; }
    }
}