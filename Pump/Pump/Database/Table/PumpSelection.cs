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
            this.PumpConnectionID = PumpConnectionID;
        }

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
       
        public int PumpConnectionID { get; set; }



    }
}