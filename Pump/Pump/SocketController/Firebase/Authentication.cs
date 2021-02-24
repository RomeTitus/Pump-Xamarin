using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using Pump.Database;
using Pump.IrrigationController;

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
        public async void DeleteSchedule(Schedule schedule)
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
        }
        public Schedule GetJsonSchedulesToObjectList(JObject scheduleDetailObject, string key)
        {
            try
            {
                var schedule = new Schedule
                {
                    ID = key,
                    NAME = scheduleDetailObject["NAME"].ToString(),
                    TIME = scheduleDetailObject["TIME"].ToString(),
                    WEEK = scheduleDetailObject["WEEK"].ToString(),
                    id_Pump = scheduleDetailObject["id_Pump"].ToString(),
                    isActive = scheduleDetailObject["isActive"].ToString()
                };
                try
                {
                    var scheduleDetailList = scheduleDetailObject["ScheduleDetails"]
                        .Select(scheduleDuration => new ScheduleDetail
                        {
                            id_Equipment = scheduleDuration["id_Equipment"]
                                .ToString(),
                            DURATION = scheduleDuration["DURATION"]
                                .ToString()
                        })
                        .ToList();
                    schedule.ScheduleDetails = scheduleDetailList;

                }
                catch
                {
                    var scheduleDetailList = new List<ScheduleDetail>();
                    foreach (var scheduleDuration in (JObject)scheduleDetailObject["ScheduleDetails"])
                        scheduleDetailList.Add(
                            new ScheduleDetail
                            {
                                ID = scheduleDuration.Key,
                                id_Equipment = scheduleDetailObject["ScheduleDetails"][scheduleDuration.Key]["id_Equipment"]
                                    .ToString(),
                                DURATION = scheduleDetailObject["ScheduleDetails"][scheduleDuration.Key]["DURATION"]
                                    .ToString()
                            });
                    schedule.ScheduleDetails = scheduleDetailList;
                }
                
                return schedule;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        //CustomSchedule
        public async Task<string> SetCustomSchedule(CustomSchedule schedule)
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
        public async void DeleteCustomSchedule(CustomSchedule schedule)
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
        }
        public CustomSchedule GetJsonCustomSchedulesToObjectList(JObject customScheduleDetailObject, string key)
        {
            try
            {
                var schedule = new CustomSchedule
                {
                    ID = key,
                    NAME = customScheduleDetailObject["NAME"].ToString(),
                    id_Pump = customScheduleDetailObject["id_Pump"].ToString()
                };

                if (customScheduleDetailObject.ContainsKey("StartTime"))
                    schedule.StartTime = long.Parse(customScheduleDetailObject["StartTime"].ToString());

                if (customScheduleDetailObject.ContainsKey("isActive"))
                    schedule.StartTime = long.Parse(customScheduleDetailObject["isActive"].ToString());


                var scheduleDetailList = new List<ScheduleDetail>();
                foreach (var scheduleDuration in (JObject)customScheduleDetailObject["ScheduleDetails"])
                    scheduleDetailList.Add(
                        new ScheduleDetail
                        {
                            ID = scheduleDuration.Key,
                            id_Equipment = customScheduleDetailObject["ScheduleDetails"][scheduleDuration.Key]["id_Equipment"]
                                .ToString(),
                            DURATION = customScheduleDetailObject["ScheduleDetails"][scheduleDuration.Key]["DURATION"]
                                .ToString()
                        });
                schedule.ScheduleDetails = scheduleDetailList;
                return schedule;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        //ManualSchedule
        public async Task<string> SetManualSchedule(ManualSchedule manual)
        {
            try
            {
                if (manual.ID == null)
                {
                    var result = await _FirebaseClient
                        .Child(getConnectedPi() + "/ManualSchedule")
                        .PostAsync(manual);
                    return result.Key;
                }

                await _FirebaseClient
                    .Child(getConnectedPi() + "/ManualSchedule/" + manual.ID)
                    .PutAsync(manual);
                return manual.ID;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }


        }
        public async Task<string> DeleteManualSchedule()
        {
            try
            {
                var firebaseManualScheduleDetail = await _FirebaseClient
                    .Child(getConnectedPi() + "/ManualSchedule")
                    .OnceAsync<JObject>();

                var keyList = firebaseManualScheduleDetail.Select(scheduleInfoDetail =>
                    scheduleInfoDetail.Key).ToList();

                await _FirebaseClient
                    .Child(getConnectedPi() + "/ManualSchedule")
                    .DeleteAsync();

                return keyList[0];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            
        }
        public ManualSchedule GetJsonManualSchedulesToObjectList(JObject scheduleDetailObject, string key)
        {
            try
            {
                var manualSchedule = new ManualSchedule()
                {
                    EndTime = long.Parse(scheduleDetailObject["EndTime"].ToString()),
                    RunWithSchedule = scheduleDetailObject["RunWithSchedule"].ToString() == "1"
                };

                var manualScheduleDetailList = new List<ManualScheduleEquipment>();
                if (scheduleDetailObject.ContainsKey("ManualDetails"))
                {


                    foreach (var scheduleDuration in (JObject)scheduleDetailObject["ManualDetails"])
                    {
                        manualScheduleDetailList.Add(
                            new ManualScheduleEquipment
                            {
                                id_Equipment = scheduleDetailObject["ManualDetails"][scheduleDuration.Key]["id_Equipment"].ToString()
                            });
                    }

                    manualSchedule.ManualDetails = manualScheduleDetailList;
                    return manualSchedule;
                }
                else
                {
                    var result = Task.Run(DeleteManualSchedule).Result;
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }


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
        public async void DeleteEquipment(Equipment equipment)
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
        }

        //Status
        public async void SetAlive(Alive alive)
        {
            try
            {
                await _FirebaseClient
                    .Child(getConnectedPi() + "/Alive/Status")
                    .PatchAsync(alive);
            }
            catch
            {
                return;
            }
            
        }
        public Alive GetJsonLastOnRequest(JObject aliveObject, Alive alive)
        {
            try
            {
                if (aliveObject.ContainsKey("RequestedTime"))
                    alive.RequestedTime = long.Parse(aliveObject["RequestedTime"].ToString());
                if (aliveObject.ContainsKey("ResponseTime"))
                    alive.ResponseTime = long.Parse(aliveObject["ResponseTime"].ToString());
                
                return alive;
            }
            catch
            {
                return null;
            }
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

        public async void DeleteSite(Site site)
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
    }
}