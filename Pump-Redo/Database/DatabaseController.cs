using System.Collections.Generic;
using Pump.Database.Table;
using SQLite;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

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

        public List<IrrigationConfiguration> GetIrrigationConfigurationList()
        {
            lock (Locker)
            {
                var irrigationConfigurationList = _database.Table<IrrigationConfiguration>();
                var configList = irrigationConfigurationList.ToList();
                configList.ForEach(x => x.DeserializedControllerPair());
                return configList;
            }
        }
        
        public void SaveIrrigationConfiguration(IrrigationConfiguration irrigationConfiguration)
        {
            lock (Locker)
            {
                irrigationConfiguration.SerializedControllerPair();
                
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

        public void DeleteIrrigationConfigurationConnection(IrrigationConfiguration irrigationConfiguration)
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