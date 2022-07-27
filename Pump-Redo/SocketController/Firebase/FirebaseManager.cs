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

        private async void Set<T>(T entity, string path) where T : IEntity
        {
            try
            {
                if (entity.Id == null)
                {
                    var result = await FirebaseQuery
                        .Child(path + "/" + entity.GetType().Name)
                        .PostAsync(entity);
                    entity.Id = result.Key;
                }

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

            Set(entity, path);
            
            /*
            if (entity.GetType() == typeof(ManualSchedule))
            {
                var manualSchedule = (ManualSchedule)entity;
                return manualSchedule.DeleteAwaiting
                    ? await DeleteManualSchedule(manualSchedule.Id, path)
                    : await SetManualSchedule(manualSchedule, path);
            }

            if (entity.GetType() == typeof(Schedule))
            {
                var schedule = (Schedule)entity;
                return schedule.DeleteAwaiting
                    ? await DeleteSchedule(schedule.Id, path)
                    : await Set(schedule, path);
            }

            if (entity.GetType() == typeof(CustomSchedule))
            {
                var customSchedule = (CustomSchedule)entity;
                return customSchedule.DeleteAwaiting
                    ? await DeleteCustomSchedule(customSchedule.Id, path)
                    : await SetCustomSchedule(customSchedule, path);
            }

            if (entity.GetType() == typeof(Equipment))
            {
                var equipment = (Equipment)entity;
                return equipment.DeleteAwaiting
                    ? await DeleteEquipment(equipment.Id, path)
                    : await SetEquipment(equipment, path);
            }

            if (entity.GetType() == typeof(Sensor))
            {
                var sensor = (Sensor)entity;
                return sensor.DeleteAwaiting ? await DeleteSensor(sensor.Id, path) : await SetSensor(sensor, path);
            }

            if (entity.GetType() == typeof(Alive))
            {
                var alive = (Alive)entity;
                return await SetAlive(alive, path);
            }

            if (entity.GetType() == typeof(NotificationToken))
            {
                var notificationToken = (NotificationToken)entity;
                return notificationToken.DeleteAwaiting
                    ? await DeleteNotificationToken(notificationToken.Id, path)
                    : await SetNotificationToken(notificationToken, path);
            }

            if (entity.GetType() == typeof(SubController))
            {
                var subController = (SubController)entity;
                return subController.DeleteAwaiting
                    ? await DeleteSubController(subController.Id, path)
                    : await SetSubController(subController, path);
            }

*/
            return "";
        }
    }
}