using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Pump.Database.Table
{
    public class NotificationToken
    {
        public NotificationToken() { }
        public NotificationToken(string token)
        {
            this.token = token;
        }

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string token { get; set; }
    }
}
