using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using Pump.Database.Table;
using Pump.IrrigationController;

namespace Pump.SocketController.Firebase
{
    public class FirebaseManager
    {
        public ChildQuery FirebaseQuery { get; private set; }

        public void InitializeFirebase(User user)
        {
            FirebaseQuery = new FirebaseClient("https://pump-25eee.firebaseio.com/").Child(user.Uid);
        }

        public async Task<List<IrrigationConfiguration>> GetIrrigationConfigList()
        {
            try
            {
                var result = await FirebaseQuery.Child("Config").OnceAsync<JObject>();
                var convertedJObject = new JObject();

                foreach (var firebaseObject in result) convertedJObject.Add(firebaseObject.Key, firebaseObject.Object);
                return ManageObservableIrrigationData.GetConfigurationListFromJObject(convertedJObject);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return new List<IrrigationConfiguration>();
        }

        private async Task Set<T>(T entity, string path) where T : IEntity
        {
            try
            {
                if (entity.Id == null)
                {
                    entity.Id = (await FirebaseQuery
                        .Child(path + "/" + entity.GetType().Name)
                        .PostAsync(entity)).Key;
                }else
                    await FirebaseQuery
                        .Child(path + "/" + entity.GetType().Name + "/" + entity.Id)
                        .PutAsync(entity);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public async Task<bool> UpdateIrrigationConfig(IrrigationConfiguration config)
        {
            try
            {
                await FirebaseQuery
                    .Child("/Config/" + config.Path)
                    .PutAsync(config);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return false;
        }

        public async Task<IReadOnlyCollection<FirebaseObject<JObject>>> GetRecordingBetweenDates(long startFrom,
            long endOn, string path)
        {
            try
            {
                var readings = await FirebaseQuery
                    .Child(path + "/Record/").OrderBy("$key").StartAt(startFrom.ToString)
                    .EndAt(endOn.ToString)
                    .OnceAsync<JObject>();
                return readings;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        public async Task<string> Description(dynamic entity, string path)
        {
            if (entity is IStatus status)
            {
                status.ControllerStatus = new ControllerStatus();
                status.HasUpdated = true;
            }
            await Set(entity, path);
            return "";
        }
    }
}