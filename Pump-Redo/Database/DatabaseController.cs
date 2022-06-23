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
            _database.CreateTable<UserAuthentication>();
        }

        public List<IrrigationConfiguration> GetControllerConfigurationList()
        {
            lock (Locker)
            {
                return _database.Table<IrrigationConfiguration>().Any()
                    ? _database.Table<IrrigationConfiguration>().ToList()
                    : new List<IrrigationConfiguration>();
            }
        }
        
        public void SaveControllerConnection(IrrigationConfiguration irrigationConfiguration)
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
                    _database.Insert(irrigationConfiguration);
                }
            }
        }

        public void DeleteControllerConnection(IrrigationConfiguration irrigationConfiguration)
        {
            lock (Locker)
            {
                _database.Delete(irrigationConfiguration);
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