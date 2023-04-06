using SQLite;

namespace Pump.Database.Table
{
    public class UserAuthentication
    {
        [PrimaryKey] [AutoIncrement] public int ID { get; set; }

        public string UserInfo { get; set; }
        public string FirebaseCredential { get; set; }
    }
}