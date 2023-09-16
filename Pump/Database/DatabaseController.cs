﻿using System.Collections.Generic;
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

        public IrrigationConfiguration GetIrrigationConfigurationByGUID(string mac)
        {
            lock (Locker)
            {
                var config = _database.Table<IrrigationConfiguration>().FirstOrDefault(x => x.DeviceGuid.Contains(mac));
                return config;
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
                    existingIrrigationConfiguration.InternalPath = irrigationConfiguration.InternalPath;
                    existingIrrigationConfiguration.ControllerPairs = irrigationConfiguration.ControllerPairs;
                    existingIrrigationConfiguration.DeviceGuid = irrigationConfiguration.DeviceGuid;
                    existingIrrigationConfiguration.LoRaNode = irrigationConfiguration.LoRaNode;
                    existingIrrigationConfiguration.Address = irrigationConfiguration.Address;
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
                try
                {
                    var result = _database.Table<UserAuthentication>().FirstOrDefault();
                    return result;
                }
                catch
                {

                }
                return null;
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