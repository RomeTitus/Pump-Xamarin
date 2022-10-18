using System.Collections.Generic;
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
        public List<IrrigationConfiguration> GetIrrigationConfigurationList()
        {
            lock (Locker)
            {
                var irrigationConfigurationList = _database.Table<IrrigationConfiguration>();
                var configList = irrigationConfigurationList.ToList();
                configList.ForEach(x => x.DeserializedProperties());
                return configList;
            }
        }

        public void SaveIrrigationConfiguration(IrrigationConfiguration irrigationConfiguration)
        {
            lock (Locker)
            {
                var existingIrrigationConfiguration = _database.Table<IrrigationConfiguration>()
                    .FirstOrDefault(x => x.Path.Equals(irrigationConfiguration.Path));
                if (existingIrrigationConfiguration != null)
                {
                    existingIrrigationConfiguration.ConnectionType = irrigationConfiguration.ConnectionType;
                    existingIrrigationConfiguration.ExternalPath = irrigationConfiguration.ExternalPath;
                    existingIrrigationConfiguration.ExternalPort = irrigationConfiguration.ExternalPort;
                    existingIrrigationConfiguration.InternalPath = irrigationConfiguration.InternalPath;
                    existingIrrigationConfiguration.InternalPort = irrigationConfiguration.InternalPort;
                    existingIrrigationConfiguration.ControllerPairs = irrigationConfiguration.ControllerPairs;
                    existingIrrigationConfiguration.DeviceGuid = irrigationConfiguration.DeviceGuid;
                    existingIrrigationConfiguration.LoRaSet = irrigationConfiguration.LoRaSet;
                    existingIrrigationConfiguration.Address = irrigationConfiguration.Address;
                    existingIrrigationConfiguration.Freq = irrigationConfiguration.Freq;
                    existingIrrigationConfiguration.Power = irrigationConfiguration.Power;
                    existingIrrigationConfiguration.Modem = irrigationConfiguration.Modem;
                    existingIrrigationConfiguration.SerializedProperties();
                    _database.Update(existingIrrigationConfiguration);
                }
                else
                {
                    irrigationConfiguration.SerializedProperties();
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
        
        public void DeleteAllIrrigationConfigurationConnection()
        {
            lock (Locker)
            {
                foreach (var configuration in GetIrrigationConfigurationList())
                {
                    _database.Delete(configuration);
                }
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