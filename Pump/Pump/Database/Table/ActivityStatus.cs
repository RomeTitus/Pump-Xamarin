using SQLite;

namespace Pump.Database.Table
{
    public class ActivityStatus
    {
        public ActivityStatus()
        {
        }

        public ActivityStatus(bool status)
        {
            this.status = status;
        }

        [PrimaryKey] [AutoIncrement] public int ID { get; set; }

        public bool status { get; set; }
    }
}