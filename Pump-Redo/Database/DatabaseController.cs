using System.Collections.Generic;
using System.Linq;
using Pump.Database.Table;
using SQLite;
using Xamarin.Forms;

namespace Pump.Database
{
    public class DatabaseController
    {
        //used to be thread safe
        private static readonly object Locker = new object();
        private readonly SQLiteConnection _database;

        public DatabaseController()
        {
            _database = DependencyService.Get<ISQLite>().GetConnection();

            _database.CreateTable<PumpConnection>();
            _database.CreateTable<PumpSelection>();
            _database.CreateTable<UserAuthentication>();
        }

        public PumpConnection GetControllerConnectionSelection()
        {
            lock (Locker)
            {
                if (_database.Table<PumpSelection>().Any())
                {
                    var pumpSelected = _database.Table<PumpSelection>().First();
                    foreach (var pumpConnection in _database.Table<PumpConnection>().ToList())
                        if (pumpConnection == null)
                            _database.Delete(pumpConnection);

                    return _database.Table<PumpConnection>().FirstOrDefault(x => x.ID == pumpSelected.PumpConnectionId);
                }

                if (_database.Table<PumpConnection>().Any())
                {
                    _database.DeleteAll<PumpSelection>();
                    var selectedNewPump = _database.Table<PumpConnection>().FirstOrDefault();

                    _database.Insert(new PumpSelection(selectedNewPump.ID));

                    return selectedNewPump;
                }

                return null;
            }
        }

        public List<PumpConnection> GetControllerConnectionList()
        {
            lock (Locker)
            {
                return _database.Table<PumpConnection>().Any()
                    ? _database.Table<PumpConnection>().ToList()
                    : new List<PumpConnection>();
            }
        }
        
        public void UpdateControllerConnection(PumpConnection pumpConnection)
        {
            lock (Locker)
            {
                var existingPumpConnection = _database.Table<PumpConnection>()
                    .FirstOrDefault(x => x.Mac.Equals(pumpConnection.Mac));
                if (existingPumpConnection != null)
                {
                    _database.Update(pumpConnection);
                }
                else
                {
                    if (_database.Table<PumpSelection>().Any())
                        _database.DeleteAll<PumpSelection>();
                    _database.Insert(pumpConnection);
                    _database.Insert(new PumpSelection(pumpConnection.ID));
                }
            }
        }

        public void DeleteControllerConnection(PumpConnection pumpConnection)
        {
            lock (Locker)
            {
                _database.DeleteAll<PumpSelection>();
                _database.Delete(pumpConnection);
                if (!_database.Table<PumpConnection>().Any()) return;
                var selectedNewPump = _database.Table<PumpConnection>().FirstOrDefault();
                _database.Insert(new PumpSelection(selectedNewPump.ID));
            }
        }

        public void SetSelectedController(PumpConnection pumpConnection)
        {
            lock (Locker)
            {
                _database.DeleteAll<PumpSelection>();
                _database.Insert(new PumpSelection { PumpConnectionId = pumpConnection.ID });
            }
        }

        public PumpConnection GetControllerNameByMac(string bt)
        {
            if (string.IsNullOrEmpty(bt)) return null;
            lock (Locker)
            {
                var pumpConnection = _database.Table<PumpConnection>().FirstOrDefault(x => x.Mac.Equals(bt));
                return pumpConnection;
            }
        }

        public bool IsRealtimeFirebaseSelected()
        {
            var selectedPump = GetControllerConnectionSelection();
            if (selectedPump?.RealTimeDatabase != null) return (bool)selectedPump.RealTimeDatabase;

            return false;
        }
        
        public UserAuthentication GetUserAuthentication()
        {
            lock (Locker)
            {
                return _database.Table<UserAuthentication>().FirstOrDefault();
            }
        }
        
        public void DeleteUserAuthentication()
        {
            lock (Locker)
            {
                _database.DeleteAll<UserAuthentication>();
            }
        }
        public void SaveUserAuthentication(UserAuthentication userAuthentication)
        {
            lock (Locker)
            {
                _database.DeleteAll<UserAuthentication>();
                _database.Insert(userAuthentication);
            }
        }
    }
}