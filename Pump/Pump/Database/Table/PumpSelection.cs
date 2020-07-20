using SQLite;

namespace Pump.Droid.Database.Table
{
    internal class PumpSelection
    {
        public PumpSelection()
        {
        }

        public PumpSelection(int PumpConnectionID)
        {
            PumpConnectionId = PumpConnectionID;
        }

        [PrimaryKey] [AutoIncrement] public int ID { get; set; }

        public int PumpConnectionId { get; set; }
    }
}