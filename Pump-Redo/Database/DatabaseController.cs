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

            _database.CreateTable<IrrigationConfiguration>();
            _database.CreateTable<PumpSelection>();
            _database.CreateTable<UserAuthentication>();
        }

        public IrrigationConfiguration GetControllerConnectionSelection()
        {
            lock (Locker)
            {
                if (_database.Table<PumpSelection>().Any())
                {
                    var pumpSelected = _database.Table<PumpSelection>().First();
                    return _database.Table<IrrigationConfiguration>().FirstOrDefault(x => x.ID == pumpSelected.IrrigationConfigurationId);
                }

                if (_database.Table<IrrigationConfiguration>().Any())
                {
                    _database.DeleteAll<PumpSelection>();
                    var selectedNewPump = _database.Table<IrrigationConfiguration>().FirstOrDefault();

                    _database.Insert(new PumpSelection(selectedNewPump.ID));

                    return selectedNewPump;
                }
                return null;
            }
        }

        public List<IrrigationConfiguration> GetControllerConnectionList()
        {
            lock (Locker)
            {
                return _database.Table<IrrigationConfiguration>().Any()
                    ? _database.Table<IrrigationConfiguration>().ToList()
                    : new List<IrrigationConfiguration>();
            }
        }
        
        public void UpdateControllerConnection(IrrigationConfiguration irrigationConfiguration)
        {
            lock (Locker)
            {
                var existingIrrigationConfiguration = _database.Table<IrrigationConfiguration>()
                    .FirstOrDefault(x => x.Mac.Equals(irrigationConfiguration.Mac));
                if (existingIrrigationConfiguration != null)
                {
                    _database.Update(irrigationConfiguration);
                }
                else
                {
                    if (_database.Table<PumpSelection>().Any())
                        _database.DeleteAll<PumpSelection>();
                    _database.Insert(irrigationConfiguration);
                    _database.Insert(new PumpSelection(irrigationConfiguration.ID));
                }
            }
        }

        public void DeleteControllerConnection(IrrigationConfiguration irrigationConfiguration)
        {
            lock (Locker)
            {
                _database.DeleteAll<PumpSelection>();
                _database.Delete(irrigationConfiguration);
                if (!_database.Table<IrrigationConfiguration>().Any()) return;
                var selectedNewPump = _database.Table<IrrigationConfiguration>().FirstOrDefault();
                _database.Insert(new PumpSelection(selectedNewPump.ID));
            }
        }

        public void SetSelectedController(IrrigationConfiguration irrigationConfiguration)
        {
            lock (Locker)
            {
                _database.DeleteAll<PumpSelection>();
                _database.Insert(new PumpSelection { IrrigationConfigurationId = irrigationConfiguration.ID });
            }
        }

        public IrrigationConfiguration GetControllerNameByMac(string bt)
        {
            if (string.IsNullOrEmpty(bt)) return null;
            lock (Locker)
            {
                var irrigationConfiguration = _database.Table<IrrigationConfiguration>().FirstOrDefault(x => x.Mac.Equals(bt));
                return irrigationConfiguration;
            }
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