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
        public Authentication()
        {
            _FirebaseClient = new FirebaseClient("https://pump-25eee.firebaseio.com/");
        }

        public FirebaseClient _FirebaseClient { get; }


        public string getConnectedPi()
        {
            var pumpDetail = new DatabaseController().GetPumpSelection();
            return "DC:A6:32:33:63:CA";
        }

        public async Task<List<Schedule>> GetAllSchedules()
        {
            var firebaseScheduleDetail = await _FirebaseClient
                .Child(getConnectedPi() + "/Schedule")
                .OnceAsync<JObject>();
            if (firebaseScheduleDetail.Count == 0)
                return new List<Schedule>();
            return firebaseScheduleDetail.Select(scheduleInfoDetail =>
                GetJsonSchedulesToObjectList(scheduleInfoDetail.Object, scheduleInfoDetail.Key)).ToList();
        }


        public Schedule GetJsonSchedulesToObjectList(JObject scheduleDetailObject, string key)
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

            var scheduleDetailList = new List<ScheduleDetail>();
            foreach (var scheduleDuration in (JObject) scheduleDetailObject["ScheduleDetails"])
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
            return schedule;
        }

        public async Task<string> SetSchedule(Schedule schedule)
        {
            var scheduleJObject = new JObject
            {
                {"NAME", schedule.NAME}, {"TIME", schedule.TIME}, {"WEEK", schedule.WEEK},
                {"id_Pump", schedule.id_Pump}, {"isActive", schedule.isActive}
            };

            scheduleJObject["ScheduleDetails"] = new JObject();
            foreach (var scheduleDetails in schedule.ScheduleDetails)
            {
                if (scheduleDetails.ID == null)
                    scheduleDetails.ID = Guid.NewGuid().ToString().GetHashCode().ToString("x");
                scheduleJObject["ScheduleDetails"][scheduleDetails.ID] = new JObject
                    {{"id_Equipment", scheduleDetails.id_Equipment}, {"DURATION", scheduleDetails.DURATION}};
            }

            if (schedule.ID == null)
            {
                var result = await _FirebaseClient
                    .Child(getConnectedPi() + "/Schedule")
                    .PostAsync(scheduleJObject);
                return result.Key;
            }

            await _FirebaseClient
                .Child(getConnectedPi() + "/Schedule/" + schedule.ID)
                .PutAsync(scheduleJObject);
            return schedule.ID;
        }

        public async Task<List<ManualSchedule>> GetManualSchedule()
        {
            var firebaseManualScheduleDetail = await _FirebaseClient
                .Child(getConnectedPi() + "/ManualSchedule")
                .OnceAsync<JObject>();

            if (firebaseManualScheduleDetail.Count == 0)
                return new List<ManualSchedule>();
            return firebaseManualScheduleDetail.Select(scheduleInfoDetail =>
                GetJsonManualSchedulesToObjectList(scheduleInfoDetail.Object, scheduleInfoDetail.Key)).ToList();
        }

        public ManualSchedule GetJsonManualSchedulesToObjectList(JObject scheduleDetailObject, string key)
        {
            var manualSchedule = new ManualSchedule()
            {
                DURATION = scheduleDetailObject["DURATION"].ToString(),
                EndTime = long.Parse(scheduleDetailObject["EndTime"].ToString()),
                RunWithSchedule = scheduleDetailObject["RunWithSchedule"].ToString() == "1"
            };

            var manualScheduleDetailList = new List<ManualScheduleEquipment>();
            foreach (var scheduleDuration in (JObject) scheduleDetailObject["ManualDetails"])
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

        public async Task<string> SetManualSchedule(IrrigationController.ManualSchedule manual)
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

        public async Task<string> DeleteManualSchedule()
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

        public async Task<List<Equipment>> GetAllEquipment()
        {
            var firebaseEquipmentDetail = await _FirebaseClient
                .Child(getConnectedPi() + "/Equipment")
                .OnceAsync<JObject>();

            if (firebaseEquipmentDetail.Count == 0)
                return new List<Equipment>();
            return firebaseEquipmentDetail.Select(equipmentInfoDetail =>
                GetJsonEquipmentToObjectList(equipmentInfoDetail.Object, equipmentInfoDetail.Key)).ToList();
        }

        public Equipment GetJsonEquipmentToObjectList(JObject equipmentDetailObject, string key)
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

        public async Task<List<Sensor>> GetAllSensors()
        {
            var firebaseScheduleDetail = await _FirebaseClient
                .Child(getConnectedPi() + "/Sensor")
                .OnceAsync<JObject>();

            if (firebaseScheduleDetail.Count == 0)
                return new List<Sensor>();
            return firebaseScheduleDetail.Select(sensorInfoDetail =>
                GetJsonSensorToObjectList(sensorInfoDetail.Object, sensorInfoDetail.Key)).ToList();
        }

        public Sensor GetJsonSensorToObjectList(JObject sensorDetailObject, string key)
        {
            var sensor = new Sensor();

            sensor.ID = key;
            sensor.NAME = sensorDetailObject["NAME"].ToString();
            sensor.GPIO = sensorDetailObject["GPIO"].ToString();
            sensor.TYPE = sensorDetailObject["TYPE"].ToString();

            if (sensorDetailObject.ContainsKey("AttachedPiController"))
                sensor.AttachedPiController = sensorDetailObject["AttachedPiController"].ToString();
            if (sensorDetailObject.ContainsKey("LastReading"))
                sensor.setSensorReading(sensorDetailObject["LastReading"].ToString());
            return sensor;
        }

        public ControllerStatus GetJsonStatusToObjectList(JObject sensorDetailObject, string key)
        {
            var status = new ControllerStatus
            {
                ID = key,
                Code = sensorDetailObject["Code"].ToString(),
                Operation = sensorDetailObject["Operation"].ToString()
            };


            return status;
        }

    }
}