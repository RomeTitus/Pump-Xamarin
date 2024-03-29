﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using Pump.Class;
using Pump.Database;
using Pump.IrrigationController;

namespace Pump.SocketController.Firebase
{
    internal class Authentication
    {
        private readonly string _blueTooth;
        public Authentication(string blueTooth = null)
        {
            _blueTooth = blueTooth;
            FirebaseClient = new FirebaseClient("https://pump-25eee.firebaseio.com/");//, new FirebaseOptions
            //{
            //    OfflineDatabaseFactory = (t, s) => new OfflineDatabase(t, s)
            //});

        }
        private string _oldBody = string.Empty;
        private ControllerStatus _controllerStatus;
        public FirebaseClient FirebaseClient { get; }


        public string GetConnectedPi()
        {
            if (_blueTooth != null)
                return _blueTooth;
            try
            {
                var pumpDetail = new DatabaseController().GetControllerConnectionSelection();
                //return "DC:A6:32:33:63:CA";
                return pumpDetail.Mac;
            }
            catch
            {
                return "";
            }
           
        }

        public List<string> GetAllConnectedPi()
        {
            try
            {
                var pumpDetailList = new DatabaseController().GetControllerConnectionList();
                return pumpDetailList.Select(x => x.Mac).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public async Task<IrrigationSelf> GetConnectedControllerInfo()
        {
            try
            {

                var firebaseSelf = await FirebaseClient
                    .Child(GetConnectedPi() + "/SelfSetup")
                    .OnceAsync<IrrigationSelf>();

                return firebaseSelf.FirstOrDefault()?.Object;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<FirebaseObject<bool>> IrrigationSystemPath(string path)
        {
            try
            {
                
                var firebaseExist = await FirebaseClient
                    .Child(path + "/MasterStatus")
                    .OnceAsync<bool>();

               
                return firebaseExist.Count > 0 ? firebaseExist.FirstOrDefault(o => o.Object) : null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            
        }

        
        //Schedule
        private async Task<string> SetSchedule(Schedule schedule)
        {
            try
            {
                if (schedule.ID == null)
                {
                    var result = await FirebaseClient
                        .Child(GetConnectedPi() + "/Schedule")
                        .PostAsync(schedule);
                    schedule.ID = result.Key;
                    return result.Key;
                }

                await FirebaseClient
                    .Child(GetConnectedPi() + "/Schedule/" + schedule.ID)
                    .PutAsync(schedule);
                return schedule.ID;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        private async Task<string> DeleteSchedule(Schedule schedule)
        {
            try
            {
                await FirebaseClient
                    .Child(GetConnectedPi() + "/Schedule/" + schedule.ID)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

            return null;
        }
        //CustomSchedule
        private async Task<string> SetCustomSchedule(CustomSchedule schedule)
        {
            try
            {
                if (schedule.ID == null)
                {
                    var result = await FirebaseClient
                        .Child(GetConnectedPi() + "/CustomSchedule")
                        .PostAsync(schedule);
                    schedule.ID = result.Key;
                    return result.Key;
                }

                await FirebaseClient
                    .Child(GetConnectedPi() + "/CustomSchedule/" + schedule.ID)
                    .PutAsync(schedule);
                return schedule.ID;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private async Task<string> DeleteCustomSchedule(CustomSchedule schedule)
        {
            try
            {
                await FirebaseClient
                    .Child(GetConnectedPi() + "/CustomSchedule/" + schedule.ID)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
        //ManualSchedule
        private async Task<string> SetManualSchedule(ManualSchedule manual, NotificationEvent notificationEvent)
        {
            try
            {
                notificationEvent.UpdateStatus("Uploading....");
                if (manual.ID == null)
                {
                    var result = await FirebaseClient
                        .Child(GetConnectedPi() + "/ManualSchedule")
                        .PostAsync(manual);
                    manual.ID = result.Key;
                }
                else
                {
                    await FirebaseClient
                        .Child(GetConnectedPi() + "/ManualSchedule/" + manual.ID)
                        .PutAsync(manual);
                }
                notificationEvent.UpdateStatus("Complete\nController Received....");
                

                _controllerStatus = new ControllerStatus();

                var firebaseReplyListener = NotificationLiveObserver(manual.ID, notificationEvent);

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while (_controllerStatus.IsComplete == null || _controllerStatus.IsComplete == false)
                {
                    if (stopwatch.Elapsed > TimeSpan.FromSeconds(15) && _controllerStatus.IsComplete == null)
                        break;
                    await Task.Delay(100);
                }
                stopwatch.Stop();

                if (_controllerStatus.IsComplete == null)
                {
                    notificationEvent.UpdateStatus("\nOperation Failed\nWe never got a reply back");

                    await FirebaseClient
                        .Child(GetConnectedPi() + "/ManualSchedule/" + manual.ID)
                        .DeleteAsync();
                }

                firebaseReplyListener.Dispose();
                await DeleteStatus(manual.ID);
                return _controllerStatus.Body;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Error!$Somewhere something went wrong\n" + e.Message;
            }


        }

        private async Task<string> DeleteManualSchedule(ManualSchedule manual, NotificationEvent notificationEvent)
        {
            try
            {
                _controllerStatus = new ControllerStatus();
                notificationEvent.UpdateStatus("Uploading....");
                await FirebaseClient
                    .Child(GetConnectedPi() + "/ManualSchedule/" + manual.ID)
                    .DeleteAsync();

                notificationEvent.UpdateStatus("Complete\nController Received....");
                var firebaseReplyListener = NotificationLiveObserver(manual.ID, notificationEvent);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                while (_controllerStatus.IsComplete == null || _controllerStatus.IsComplete == false)
                {
                    if (stopwatch.Elapsed > TimeSpan.FromSeconds(15) && _controllerStatus.IsComplete == null)
                        break;
                    await Task.Delay(100);
                }
                stopwatch.Stop();

                if (_controllerStatus.IsComplete == null)
                {
                    notificationEvent.UpdateStatus("\nOperation Failed\nWe never got a reply back");
                }
                    
                firebaseReplyListener.Dispose();
                await DeleteStatus(manual.ID);
                return _controllerStatus.Body;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Error!$Somewhere something went wrong\n" + e.Message; 
            }
        }

        private async Task DeleteStatus(string statusId)
        {
            try
            {
                
                await FirebaseClient
                    .Child(GetConnectedPi() + "/Status/" + statusId)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private IDisposable NotificationLiveObserver(string id, NotificationEvent notificationEvent)
        {
            var firebaseReplyListener = FirebaseClient
                .Child(GetConnectedPi()).Child("ControllerStatus")
                .AsObservable<ControllerStatus>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (x.Object == null)
                            return;
                        var result = x.Object;
                        result.ID = x.Key;
                        if (x.Key == id)
                        {
                                //TODO Clear this up
                                int pos = result.Body.IndexOf(_oldBody, StringComparison.Ordinal);
                                if (pos >= 0)
                                {
                                    string strDiff = result.Body.Remove(pos, _oldBody.Length);
                                    notificationEvent.UpdateStatus(strDiff);
                                }
                                else
                                    notificationEvent.UpdateStatus(result.Body);

                                _oldBody = result.Body;

                                _controllerStatus.Body = result.Body;
                                _controllerStatus.IsComplete = result.IsComplete;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            return firebaseReplyListener;
        }

        //Equipment
        private async Task<string> SetEquipment(Equipment equipment)
        {
            try
            {
                if (equipment.ID == null)
                {
                    var result = await FirebaseClient
                        .Child(GetConnectedPi() + "/Equipment")
                        .PostAsync(equipment);
                    equipment.ID = result.Key;
                    return result.Key;
                }

                await FirebaseClient
                    .Child(GetConnectedPi() + "/Equipment/" + equipment.ID)
                    .PutAsync(equipment);
                return equipment.ID;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private async Task<string> DeleteEquipment(Equipment equipment)
        {
            try
            {
                await FirebaseClient
                    .Child(GetConnectedPi() + "/Equipment/" + equipment.ID)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        //Status
        private async Task<string> SetAlive(Alive alive)
        {
            try
            {
                await FirebaseClient
                    .Child(GetConnectedPi() + "/Alive/Status")
                    .PatchAsync(alive);
            }
            catch
            {
                // ignored
            }

            return null;

        }

        //Sensor
        private async Task<string> SetSensor(Sensor sensor)
        {
            try
            {
                if (sensor.ID == null)
                {
                    var result = await FirebaseClient
                        .Child(GetConnectedPi() + "/Sensor")
                        .PostAsync(sensor);
                    sensor.ID = result.Key;
                    return result.Key;
                }

                await FirebaseClient
                    .Child(GetConnectedPi() + "/Sensor/" + sensor.ID)
                    .PutAsync(sensor);
                return sensor.ID;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private async Task<string> DeleteSensor(Sensor sensor)
        {
            try
            {
                await FirebaseClient
                    .Child(GetConnectedPi() + "/Sensor/" + sensor.ID)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }
        //Site
        private async Task<string> SetSite(Site site)
        {
            try
            {
                if (site.ID == null)
                {
                    var result = await FirebaseClient
                        .Child(GetConnectedPi() + "/Site")
                        .PostAsync(site);
                    site.ID = result.Key;
                    return result.Key;
                }

                await FirebaseClient
                    .Child(GetConnectedPi() + "/Site/" + site.ID)
                    .PutAsync(site);
                return site.ID;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private async Task<string> DeleteSite(Site site)
        {
            try
            {
                await FirebaseClient
                    .Child(GetConnectedPi() + "/Site/" + site.ID)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }


        //SubController
        public async Task<string> SetSubController(SubController subController, NotificationEvent notificationEvent)
        {
            try
            {
                notificationEvent.UpdateStatus("Uploading....");
                if (subController.ID == null)
                {
                    var result = await FirebaseClient
                        .Child(GetConnectedPi() + "/SubController")
                        .PostAsync(subController);
                    subController.ID = result.Key;
                    //return result.Key;
                }

                await FirebaseClient
                    .Child(GetConnectedPi() + "/SubController/" + subController.ID)
                    .PutAsync(subController);
                //return subController.ID;
                notificationEvent.UpdateStatus("Complete\nController Received....");
                _controllerStatus = new ControllerStatus();

                var firebaseReplyListener = NotificationLiveObserver(subController.ID, notificationEvent);

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while (_controllerStatus.IsComplete == null || _controllerStatus.IsComplete == false)
                {
                    if (stopwatch.Elapsed > TimeSpan.FromSeconds(15) && _controllerStatus.IsComplete == null)
                        break;
                    await Task.Delay(100);
                }
                stopwatch.Stop();

                if (_controllerStatus.IsComplete == null)
                {
                    notificationEvent.UpdateStatus("\nOperation Failed\nWe never got a reply back");
                }

                firebaseReplyListener.Dispose();
                await DeleteStatus(subController.ID);
                return _controllerStatus.Body;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Error!$Somewhere something went wrong\n" + e.Message;
            }
        }

        private async Task<string> DeleteSubController(SubController subController, NotificationEvent notificationEvent)
        {
            try
            {
                _controllerStatus = new ControllerStatus();
                notificationEvent.UpdateStatus("Uploading....");
                
                foreach (var mac in GetAllConnectedPi())
                {
                    await FirebaseClient
                        .Child(mac + "/SubController/" + subController.ID)
                        .DeleteAsync();
                }
                notificationEvent.UpdateStatus("Complete\nController Received....");
                var firebaseReplyListener = NotificationLiveObserver(subController.ID, notificationEvent);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                while (_controllerStatus.IsComplete == null || _controllerStatus.IsComplete == false)
                {
                    if (stopwatch.Elapsed > TimeSpan.FromSeconds(15) && _controllerStatus.IsComplete == null)
                        break;
                    await Task.Delay(100);
                }
                stopwatch.Stop();

                if (_controllerStatus.IsComplete == null)
                {
                    notificationEvent.UpdateStatus("\nOperation Failed\nWe never got a reply back");
                }
                    
                firebaseReplyListener.Dispose();
                await DeleteStatus(subController.ID);
                return _controllerStatus.Body;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Error!$Somewhere something went wrong\n" + e.Message; 
            }
        }
        
        //NotificationToken
        private async Task<string> SetNotificationToken(NotificationToken notificationToken)
        {
            try
            {
                foreach (var mac in GetAllConnectedPi())
                {
                    if (notificationToken.ID == null)
                    {
                        var result = await FirebaseClient
                            .Child(mac + "/NotificationToken")
                            .PostAsync(notificationToken);
                        notificationToken.ID = result.Key;
                        return result.Key;
                    }

                    await FirebaseClient
                        .Child(mac + "/NotificationToken/" + notificationToken.ID)
                        .PutAsync(notificationToken);
                    return notificationToken.ID;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
        private async Task<string> DeleteNotificationToken(NotificationToken notificationToken)
        {
            try
            {
                foreach (var mac in GetAllConnectedPi())
                {
                    await FirebaseClient
                        .Child(mac + "/NotificationToken/" + notificationToken.ID)
                        .DeleteAsync();
                }
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        public async Task<IReadOnlyCollection<FirebaseObject<JObject>>> GetRecordingBetweenDates(long startFrom, long endOn)
        {
            try
            {

                var readings = await FirebaseClient
                    .Child(GetConnectedPi() + "/Record/").OrderBy("$key").StartAt(startFrom.ToString).EndAt(endOn.ToString)
                    .OnceAsync<JObject>();
                return readings;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        public async Task<string> Descript(object entity, NotificationEvent notificationEvent)
        {
            if (entity.GetType() == typeof(ManualSchedule))
            {
                var manualSchedule = (ManualSchedule) entity;
                return manualSchedule.DeleteAwaiting ? await DeleteManualSchedule(manualSchedule, notificationEvent) : await SetManualSchedule(manualSchedule, notificationEvent);
            }
            else if (entity.GetType() == typeof(Schedule))
            {
                var schedule = (Schedule)entity;
                return schedule.DeleteAwaiting ? await DeleteSchedule(schedule) : await SetSchedule(schedule);
            }
            else if (entity.GetType() == typeof(CustomSchedule))
            {
                var customSchedule = (CustomSchedule)entity;
                return customSchedule.DeleteAwaiting ? await DeleteCustomSchedule(customSchedule) : await SetCustomSchedule(customSchedule);
            }
            else if (entity.GetType() == typeof(Equipment))
            {
                var equipment = (Equipment)entity;
                return equipment.DeleteAwaiting ? await DeleteEquipment(equipment) : await SetEquipment(equipment);
            }
            else if (entity.GetType() == typeof(Sensor))
            {
                var sensor = (Sensor)entity;
                return sensor.DeleteAwaiting ? await DeleteSensor(sensor) : await SetSensor(sensor);
            }
            else if (entity.GetType() == typeof(Site))
            {
                var site = (Site)entity;
                return site.DeleteAwaiting ? await DeleteSite(site) : await SetSite(site);
            }
            else if (entity.GetType() == typeof(Alive))
            {
                var alive = (Alive)entity;
                return await SetAlive(alive);
            }
            else if (entity.GetType() == typeof(NotificationToken))
            {
                var notificationToken = (NotificationToken)entity;
                return notificationToken.DeleteAwaiting ? await DeleteNotificationToken(notificationToken) : await SetNotificationToken(notificationToken);
            }
            else if (entity.GetType() == typeof(SubController))
            {
                var subController = (SubController)entity;
                return subController.DeleteAwaiting ? await DeleteSubController(subController, notificationEvent) : await SetSubController(subController, notificationEvent);
            }
            return "";
        }
    }
}