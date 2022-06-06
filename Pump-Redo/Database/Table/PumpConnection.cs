using System;
using System.Collections.Generic;
using SQLite;

namespace Pump.Database.Table
{
    public class PumpConnection
    {
        public readonly List<string> ConnectionTypeList = new List<string> { "Cloud", "Network", "BlueTooth" };

        public PumpConnection()
        {
            IDeviceGuid = Guid.Empty;
        }

        [PrimaryKey] [AutoIncrement] public int ID { get; set; }
        public string Name { get; set; }
        public string Mac { get; set; }
        public Guid IDeviceGuid { get; set; }
        public int ConnectionType { get; set; }
        public string InternalPath { get; set; }
        public int InternalPort { get; set; }
        public string ExternalPath { get; set; }
        public int ExternalPort { get; set; }
        public bool? RealTimeDatabase { get; set; }
        public string SiteSelectedId { get; set; }
    }
}