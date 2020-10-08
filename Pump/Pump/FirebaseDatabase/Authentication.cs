using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Database.Offline;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using Pump.Database;
using Pump.IrrigationController;

namespace Pump.FirebaseDatabase
{
    internal class Authentication
    {
        public Authentication()
        {

            _FirebaseClient = new FirebaseClient("https://pump-25eee.firebaseio.com/");//, new FirebaseOptions
            //{
            //    OfflineDatabaseFactory = (t, s) => new OfflineDatabase(t, s)
            //});

        }

        public FirebaseClient _FirebaseClient { get; }


        public string getConnectedPi()
        {
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
        public async Task<string> SetManualSchedule(IrrigationController.ManualSchedule manual)
        {
            try
            {
                var manualJObject = new JObject
                {
                    {"EndTime", manual.EndTime},{"DURATION", manual.DURATION}, {"RunWithSchedule", manual.RunWithSchedule ? "1" : "0"}
                };
                manualJObject["ManualDetails"] = new JObject();
                foreach (var equipment in manual.equipmentIdList)
                {
                    var key = Guid.NewGuid().ToString().GetHashCode().ToString("x");
                    manualJObject["ManualDetails"][key] = new JObject { ["id_Equipment"] = equipment.ID };
                }

                var result = await _FirebaseClient
                    .Child(getConnectedPi() + "/ManualSchedule")
                    .PostAsync(manualJObject);
                return result.Key;
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
                    DURATION = scheduleDetailObject["DURATION"].ToString(),
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
                                ID = scheduleDetailObject["ManualDetails"][scheduleDuration.Key]["id_Equipment"].ToString()
                            });
                    }

                    manualSchedule.equipmentIdList = manualScheduleDetailList;
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
        public Equipment GetJsonEquipmentToObjectList(JObject equipmentDetailObject, string key)
        {
            try
            {
                var equipment = new Equipment();

                equipment.ID = key;
                equipment.NAME = equipmentDetailObject["NAME"].ToString();
                equipment.GPIO = equipmentDetailObject["GPIO"].ToString();
                equipment.isPump = equipmentDetailObject["isPump"].ToString() == "1";

                if (equipmentDetailObject.ContainsKey("AttachedPiController"))
                    equipment.AttachedPiController = equipmentDetailObject["AttachedPiController"].ToString();
                if (equipmentDetailObject.ContainsKey("DirectOnlineGPIO"))
                    equipment.DirectOnlineGPIO = equipmentDetailObject["DirectOnlineGPIO"].ToString();
                return equipment;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            
        }

        //Sensor
        public async Task<List<Sensor>> GetAllSensors()
        {
            try
            {
                var firebaseScheduleDetail = await _FirebaseClient
                    .Child(getConnectedPi() + "/Sensor")
                    .OnceAsync<JObject>();

                if (firebaseScheduleDetail.Count == 0)
                    return new List<Sensor>();
                return firebaseScheduleDetail.Select(sensorInfoDetail =>
                    GetJsonSensorToObjectList(sensorInfoDetail.Object, sensorInfoDetail.Key)).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            
        }
        public Sensor GetJsonSensorToObjectList(JObject sensorDetailObject, string key)
        {
            try
            {
                var sensor = new Sensor();

                sensor.ID = key;
                if (sensorDetailObject.ContainsKey("NAME"))
                    sensor.NAME = sensorDetailObject["NAME"].ToString();
                if (sensorDetailObject.ContainsKey("GPIO"))
                    sensor.GPIO = sensorDetailObject["GPIO"].ToString();
                if (sensorDetailObject.ContainsKey("TYPE"))
                    sensor.TYPE = sensorDetailObject["TYPE"].ToString();

                if (sensorDetailObject.ContainsKey("AttachedPiController"))
                    sensor.AttachedPiController = sensorDetailObject["AttachedPiController"].ToString();
                if (sensorDetailObject.ContainsKey("LastReading"))
                    sensor.setSensorReading(sensorDetailObject["LastReading"].ToString());
                return sensor;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
            
        }
        
        //Status
        public ControllerStatus GetJsonStatusToObjectList(JObject sensorDetailObject, string key)
        {
            try
            {
                var status = new ControllerStatus
                {
                    ID = key,
                    Code = sensorDetailObject["Code"].ToString(),
                    Operation = sensorDetailObject["Operation"].ToString()
                };


                return status;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return  null;
            }
            
        }
        public async void SetLastOnRequest()
        {
            try
            {
                var lastOnJObject = new JObject {{ "RequestedTime", (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds}};
                await _FirebaseClient
                    .Child(getConnectedPi() + "/Alive/Status")
                    .PatchAsync(lastOnJObject);
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
    }
}