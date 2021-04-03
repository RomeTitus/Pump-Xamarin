using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using Pump.Database;
using Pump.IrrigationController;
using Pump.Layout;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace Pump.FirebaseDatabase
{
    internal class Authentication
    {
        private string _blueTooth;
        public Authentication(string blueTooth = null)
        {
            _blueTooth = blueTooth;
            _FirebaseClient = new FirebaseClient("https://pump-25eee.firebaseio.com/");//, new FirebaseOptions
            //{
            //    OfflineDatabaseFactory = (t, s) => new OfflineDatabase(t, s)
            //});

        }

        private string _notificationResult;
        public FirebaseClient _FirebaseClient { get; }


        public string getConnectedPi()
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


        public async Task<IrrigationSelf> GetConnectedControllerInfo()
        {
            try
            {

                var firebaseSelf = await _FirebaseClient
                    .Child(getConnectedPi() + "/SelfSetup")
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
                
                var firebaseExist = await _FirebaseClient
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
        public async Task<string> SetSchedule(Schedule schedule)
        {
            
            try
            {
                

                if (schedule.ID == null)
                {
                    var result = await _FirebaseClient
                        .Child(getConnectedPi() + "/Schedule")
                        .PostAsync(schedule);
                    return result.Key;
                }

                await _FirebaseClient
                    .Child(getConnectedPi() + "/Schedule/" + schedule.ID)
                    .PutAsync(schedule);
                return schedule.ID;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            
        }
        public async Task<string> DeleteSchedule(Schedule schedule)
        {
            try
            {
                await _FirebaseClient
                    .Child(getConnectedPi() + "/Schedule/" + schedule.ID)
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
                    var result = await _FirebaseClient
                        .Child(getConnectedPi() + "/CustomSchedule")
                        .PostAsync(schedule);
                    return result.Key;
                }

                await _FirebaseClient
                    .Child(getConnectedPi() + "/CustomSchedule/" + schedule.ID)
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
                await _FirebaseClient
                    .Child(getConnectedPi() + "/CustomSchedule/" + schedule.ID)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
        //ManualSchedule
        private async Task<string> SetManualSchedule(ManualSchedule manual)
        {
            try
            {
                if (manual.ID == null)
                {
                    var result = await _FirebaseClient
                        .Child(getConnectedPi() + "/ManualSchedule")
                        .PostAsync(manual);
                    manual.ID = result.Key;
                }
                else
                {
                    await _FirebaseClient
                        .Child(getConnectedPi() + "/ManualSchedule/" + manual.ID)
                        .PutAsync(manual);
                }



                _notificationResult = string.Empty;

                var firebaseReplyListener = ManualScheduleLiveObserver(manual.ID);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                while (stopwatch.Elapsed < TimeSpan.FromSeconds(15) && string.IsNullOrEmpty(_notificationResult))
                {
                    await Task.Delay(500);
                }
                stopwatch.Stop();

                if (string.IsNullOrEmpty(_notificationResult))
                    _notificationResult = "No Reply$We never got a reply back\nWould you like to keep your changes?$Revert";

                firebaseReplyListener.Dispose();
                await DeleteStatus(manual.ID);
                return _notificationResult;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return "Error!$Somewhere something went wrong\n" + e.Message;
            }


        }

        private async Task<string> DeleteManualSchedule(ManualSchedule manual)
        {
            try
            {
                _notificationResult = string.Empty;

                
                await _FirebaseClient
                    .Child(getConnectedPi() + "/ManualSchedule/" + manual.ID)
                    .DeleteAsync();

                var firebaseReplyListener = ManualScheduleLiveObserver(manual.ID);

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                
                while (stopwatch.Elapsed < TimeSpan.FromSeconds(15) && string.IsNullOrEmpty(_notificationResult))
                {
                    await Task.Delay(500);
                }
                stopwatch.Stop();

                if (string.IsNullOrEmpty(_notificationResult))
                    _notificationResult = "Cleared!$We never got a reply back";
                
                firebaseReplyListener.Dispose();
                await DeleteStatus(manual.ID);
                return _notificationResult;
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
                
                await _FirebaseClient
                    .Child(getConnectedPi() + "/Status/" + statusId)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        private IDisposable ManualScheduleLiveObserver(string id)
        {
            var firebaseReplyListener = _FirebaseClient
                .Child(getConnectedPi()).Child("Status")
                .AsObservable<ControllerStatus>()
                .Subscribe(x =>
                {
                    try
                    {
                        if(x.Object == null)
                            return;
                        var result = x.Object;
                        result.ID = x.Key;
                        if (x.Key == id && string.IsNullOrEmpty(_notificationResult) && !string.IsNullOrEmpty(result.Code))
                        {
                            _notificationResult = result.Operation + '$' + result.Code;
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
        public async Task<string> SetEquipment(Equipment equipment)
        {
            try
            {
                if (equipment.ID == null)
                {
                    var result = await _FirebaseClient
                        .Child(getConnectedPi() + "/Equipment")
                        .PostAsync(equipment);
                    return result.Key;
                }

                await _FirebaseClient
                    .Child(getConnectedPi() + "/Equipment/" + equipment.ID)
                    .PutAsync(equipment);
                return equipment.ID;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        public async Task<string> DeleteEquipment(Equipment equipment)
        {
            try
            {
                await _FirebaseClient
                    .Child(getConnectedPi() + "/Equipment/" + equipment.ID)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        //Status
        public async Task<string> SetAlive(Alive alive)
        {
            try
            {
                await _FirebaseClient
                    .Child(getConnectedPi() + "/Alive/Status")
                    .PatchAsync(alive);
            }
            catch
            {
               
            }

            return null;

        }

    //Sensor
        public async Task<string> SetSensor(Sensor sensor)
        {
            try
            {
                if (sensor.ID == null)
                {
                    var result = await _FirebaseClient
                        .Child(getConnectedPi() + "/Sensor")
                        .PostAsync(sensor);
                    return result.Key;
                }

                await _FirebaseClient
                    .Child(getConnectedPi() + "/Sensor/" + sensor.ID)
                    .PutAsync(sensor);
                return sensor.ID;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<string> DeleteSensor(Sensor sensor)
        {
            try
            {
                await _FirebaseClient
                    .Child(getConnectedPi() + "/Sensor/" + sensor.ID)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }
        //Site
        public async Task<string> SetSite(Site site)
        {
            try
            {
                if (site.ID == null)
                {
                    var result = await _FirebaseClient
                        .Child(getConnectedPi() + "/Site")
                        .PostAsync(site);
                    return result.Key;
                }

                await _FirebaseClient
                    .Child(getConnectedPi() + "/Site/" + site.ID)
                    .PutAsync(site);
                return site.ID;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<string> DeleteSite(Site site)
        {
            try
            {
                await _FirebaseClient
                    .Child(getConnectedPi() + "/Site/" + site.ID)
                    .DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }


        //SubController
        public async Task<string> SetSubController(SubController subController)
        {

            try
            {
                if (subController.ID == null)
                {
                    var result = await _FirebaseClient
                        .Child(getConnectedPi() + "/SubController")
                        .PostAsync(subController);
                    return result.Key;
                }

                await _FirebaseClient
                    .Child(getConnectedPi() + "/SubController/" + subController.ID)
                    .PutAsync(subController);
                return subController.ID;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        public async Task<string> Descript(object entity)
        {
            if (entity.GetType() == typeof(ManualSchedule))
            {
                ManualSchedule manualSchedule = (ManualSchedule) entity;
                return manualSchedule.DeleteAwaiting ? await DeleteManualSchedule(manualSchedule) : await SetManualSchedule(manualSchedule);
            }
            else if (entity.GetType() == typeof(Schedule))
            {
                Schedule schedule = (Schedule)entity;
                return schedule.DeleteAwaiting ? await DeleteSchedule(schedule) : await SetSchedule(schedule);
            }
            else if (entity.GetType() == typeof(CustomSchedule))
            {
                CustomSchedule customSchedule = (CustomSchedule)entity;
                return customSchedule.DeleteAwaiting ? await DeleteCustomSchedule(customSchedule) : await SetCustomSchedule(customSchedule);
            }
            else if (entity.GetType() == typeof(Equipment))
            {
                Equipment equipment = (Equipment)entity;
                return equipment.DeleteAwaiting ? await DeleteEquipment(equipment) : await SetEquipment(equipment);
            }
            else if (entity.GetType() == typeof(Sensor))
            {
                Sensor sensor = (Sensor)entity;
                return sensor.DeleteAwaiting ? await DeleteSensor(sensor) : await SetSensor(sensor);
            }
            else if (entity.GetType() == typeof(Site))
            {
                Site site = (Site)entity;
                return site.DeleteAwaiting ? await DeleteSite(site) : await SetSite(site);
            }
            else if (entity.GetType() == typeof(Alive))
            {
                Alive alive = (Alive)entity;
                return await SetAlive(alive);
            }
            return "";
        }
    }
}