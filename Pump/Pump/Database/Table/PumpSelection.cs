using System;
using System.Collections.Generic;
using SQLite;

namespace Pump.Droid.Database.Table
{
    class PumpSelection
    {
        public PumpSelection() { }

        public PumpSelection(int PumpConnectionID)
        {
            this.PumpConnectionId = PumpConnectionID;
        }

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
       
        public int PumpConnectionId { get; set; }



    }
}