using SQLite;

namespace Pump.Database.Table
{
    public class PumpSelection
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