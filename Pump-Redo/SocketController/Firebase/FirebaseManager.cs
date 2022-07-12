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

        //Schedule
        private async Task<string> SetSchedule(Schedule schedule, string path)
        {
            try
            {
                if (schedule.Id == null)
                {
                    var result = await FirebaseQuery
                        .Child(path + "/Schedule")
                        .PostAsync(schedule);
                    schedule.Id = result.Key;
                    return result.Key;
                }

                await FirebaseQuery
                    .Child(path + "/Schedule/" + schedule.Id)
                    .PutAsync(schedule);
                return schedule.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private async Task<string> DeleteSchedule(string scheduleId, string path)
        {
            try
            {
                await FirebaseQuery
                    .Child(path + "/Schedule/" + scheduleId)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        //CustomSchedule
        private async Task<string> SetCustomSchedule(CustomSchedule customSchedule, string path)
        {
            try
            {
                if (customSchedule.Id == null)
                {
                    var result = await FirebaseQuery
                        .Child(path + "/CustomSchedule")
                        .PostAsync(customSchedule);
                    customSchedule.Id = result.Key;
                    return result.Key;
                }

                await FirebaseQuery
                    .Child(path + "/CustomSchedule/" + customSchedule.Id)
                    .PutAsync(customSchedule);
                return customSchedule.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private async Task<string> DeleteCustomSchedule(string customScheduleId, string path)
        {
            try
            {
                await FirebaseQuery
                    .Child(path + "/CustomSchedule/" + customScheduleId)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        //ManualSchedule
        private async Task<string> SetManualSchedule(ManualSchedule manual, string path)
        {
            try
            {
                if (manual.Id == null)
                {
                    var result = await FirebaseQuery
                        .Child(path + "/ManualSchedule")
                        .PostAsync(manual);
                    manual.Id = result.Key;
                }
                else
                {
                    await FirebaseQuery
                        .Child(path + "/ManualSchedule/" + manual.Id)
                        .PutAsync(manual);
                }

                return manual.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Error!$Somewhere something went wrong\n" + e.Message;
            }
        }

        private async Task<string> DeleteManualSchedule(string manualId, string path)
        {
            try
            {
                await FirebaseQuery
                    .Child(path + "/ManualSchedule/" + manualId)
                    .DeleteAsync();

                return manualId;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Error!$Somewhere something went wrong\n" + e.Message;
            }
        }

        private async Task DeleteStatus(string statusId, string path)
        {
            try
            {
                await FirebaseQuery
                    .Child(path + "/Status/" + statusId)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        //Equipment
        private async Task<string> SetEquipment(Equipment equipment, string path)
        {
            try
            {
                if (equipment.Id == null)
                {
                    var result = await FirebaseQuery
                        .Child(path + "/Equipment")
                        .PostAsync(equipment);
                    equipment.Id = result.Key;
                    return result.Key;
                }

                await FirebaseQuery
                    .Child(path + "/Equipment/" + equipment.Id)
                    .PutAsync(equipment);
                return equipment.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private async Task<string> DeleteEquipment(string equipmentId, string path)
        {
            try
            {
                await FirebaseQuery
                    .Child(path + "/Equipment/" + equipmentId)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        //Status
        private async Task<string> SetAlive(Alive alive, string path)
        {
            try
            {
                await FirebaseQuery
                    .Child(path + "/Alive/Status")
                    .PatchAsync(alive);
            }
            catch
            {
                // ignored
            }

            return null;
        }

        //Sensor
        private async Task<string> SetSensor(Sensor sensor, string path)
        {
            try
            {
                if (sensor.Id == null)
                {
                    var result = await FirebaseQuery
                        .Child(path + "/Sensor")
                        .PostAsync(sensor);
                    sensor.Id = result.Key;
                    return result.Key;
                }

                await FirebaseQuery
                    .Child(path + "/Sensor/" + sensor.Id)
                    .PutAsync(sensor);
                return sensor.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private async Task<string> DeleteSensor(string sensorId, string path)
        {
            try
            {
                await FirebaseQuery
                    .Child(path + "/Sensor/" + sensorId)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        //SubController
        private async Task<string> SetSubController(SubController subController, string path)
        {
            try
            {
                if (subController.Id == null)
                {
                    var result = await FirebaseQuery
                        .Child(path + "/SubController")
                        .PostAsync(subController);
                    subController.Id = result.Key;
                }

                await FirebaseQuery
                    .Child(path + "/SubController/" + subController.Id)
                    .PutAsync(subController);
                return subController.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Error!$Somewhere something went wrong\n" + e.Message;
            }
        }

        private async Task<string> DeleteSubController(string subControllerId, string path)
        {
            try
            {
                await FirebaseQuery
                    .Child(path + "/SubController/" + subControllerId)
                    .DeleteAsync();

                return subControllerId;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Error!$Somewhere something went wrong\n" + e.Message;
            }
        }

        //NotificationToken
        private async Task<string> SetNotificationToken(NotificationToken notificationToken, string path)
        {
            try
            {
                if (notificationToken.Id == null)
                {
                    var result = await FirebaseQuery
                        .Child(path + "/NotificationToken")
                        .PostAsync(notificationToken);
                    notificationToken.Id = result.Key;
                    return result.Key;
                }

                await FirebaseQuery
                    .Child(path + "/NotificationToken/" + notificationToken.Id)
                    .PutAsync(notificationToken);
                return notificationToken.Id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        private async Task<string> DeleteNotificationToken(string notificationTokenId, string path)
        {
            try
            {
                await FirebaseQuery
                    .Child(path + "/NotificationToken/" + notificationTokenId)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        public async Task<string> UpdateIrrigationConfig(IrrigationConfiguration config)
        {
            try
            {
                var result = await FirebaseQuery
                    .Child("/Config/ " + config.Path)
                    .PostAsync(config);
                return result.Key;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
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

        public async Task<string> Description(object entity, string path)
        {
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
                    : await SetSchedule(schedule, path);
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

            return "";
        }
    }
}