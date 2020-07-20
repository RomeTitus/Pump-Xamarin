using SQLite;

namespace Pump.Droid.Database.Table
{
    public class PumpConnection
    {
        public PumpConnection()
        {
        }

        public PumpConnection(string Name, string Mac, string InternalPath, int InternalPort, string ExternalPath,
            int ExternalPort)
        {
            this.Name = Name;
            this.Mac = Mac;
            this.InternalPath = InternalPath;
            this.InternalPort = InternalPort;
            this.ExternalPath = ExternalPath;
            this.ExternalPort = ExternalPort;
        }

        [PrimaryKey] [AutoIncrement] public int ID { get; set; }

        public string Name { get; set; }
        public string Mac { get; set; }
        public string InternalPath { get; set; }
        public int InternalPort { get; set; }
        public string ExternalPath { get; set; }
        public int ExternalPort { get; set; }
        public bool? RealTimeDatabase { get; set; }
    }
}