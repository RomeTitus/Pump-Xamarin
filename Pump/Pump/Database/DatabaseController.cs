using Pump.Droid.Database.Table;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Pump.Database
{
    class DatabaseController
    {
        //used to be thread safe
        static object locker = new object();
        SQLiteConnection database;

        public DatabaseController()
        {
            //database = DependencyService.Get<ISQLite>.GetConnection();
            
            database = DependencyService.Get<ISQLite>().GetConnection();

            database.CreateTable<PumpConnection>();

            database.CreateTable<PumpSelection>();
        }

        public PumpConnection getPumpSelection()
        {
            lock (locker)
            {
                if(database.Table<PumpSelection>().Count() > 0)
                {
                    var pumpSelected = database.Table<PumpSelection>().First();
                    return database.Table<PumpConnection>().FirstOrDefault(x => x.ID == pumpSelected.PumpConnectionID);
                }
                else
                    return null;
            }
        }

        public void addPumpConnection(PumpConnection pumpConnection)
        {
            lock (locker)
            {
                if (database.Table<PumpSelection>().Count() > 0)
                    database.DeleteAll<PumpSelection>();

                database.Insert(pumpConnection);
                database.Insert(new PumpSelection(pumpConnection.ID));
            }
        }
    }
}
