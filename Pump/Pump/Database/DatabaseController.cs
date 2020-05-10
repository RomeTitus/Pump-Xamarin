using Pump.Droid.Database.Table;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace Pump.Database
{
    class DatabaseController
    {
        //used to be thread safe
        public static readonly object Locker = new object();
        private readonly SQLiteConnection _database;

        public DatabaseController()
        {
            //database = DependencyService.Get<ISQLite>.GetConnection();
            
            _database = DependencyService.Get<ISQLite>().GetConnection();

            _database.CreateTable<PumpConnection>();

            _database.CreateTable<PumpSelection>();
        }

        public PumpConnection GetPumpSelection()
        {
            lock (Locker)
            {
                if(_database.Table<PumpSelection>().Any())
                {
                    var pumpSelected = _database.Table<PumpSelection>().First();
                    return _database.Table<PumpConnection>().FirstOrDefault(x => x.ID == pumpSelected.PumpConnectionId);
                }
                else
                    return null;
            }
        }

        public void AddPumpConnection(PumpConnection pumpConnection)
        {
            lock (Locker)
            {
                if (_database.Table<PumpSelection>().Any())
                    _database.DeleteAll<PumpSelection>();

                _database.Insert(pumpConnection);
                _database.Insert(new PumpSelection(pumpConnection.ID));
            }
        }

        public void UpdatePumpConnection(PumpConnection pumpConnection)
        {
            _database.Update(pumpConnection);
        }
    }
}
