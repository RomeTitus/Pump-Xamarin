using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
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

        public async Task<List<Equipment>> GetAllEquipment()
        {
            var firebaseEquipmentDetail = await _FirebaseClient
                .Child(getConnectedPi() + "/Equipment")
                .OnceAsync<JObject>();

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
    }
}