using SQLite;

namespace Pump.Database.Table
{
    public class PumpSelection
    {
        public PumpSelection()
        {
        }

        public PumpSelection(int irrigationConfigurationId)
        {
            IrrigationConfigurationId = irrigationConfigurationId;
        }

        [PrimaryKey] [AutoIncrement] public int Id { get; set; }

        public int IrrigationConfigurationId { get; set; }
    }
}