using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Pump.Class;

namespace Pump.IrrigationController
{
    internal static class IrrigationConvert
    {
        public static
            Tuple<List<CustomSchedule>, List<Schedule>, List<Equipment>, List<ManualSchedule>, List<Sensor>, List<SubController>> IrrigationJObjectToList(JObject irrigationJObject)
        {
            var customScheduleList = new List<CustomSchedule>();
            var scheduleList = new List<Schedule>();
            var equipmentList = new List<Equipment>();
            var manualScheduleList = new List<ManualSchedule>();
            var sensorList = new List<Sensor>();
            var subControllerList = new List<SubController>();

            if (irrigationJObject.ContainsKey(nameof(CustomSchedule)))
                foreach (var jToken in irrigationJObject[nameof(CustomSchedule)])
                {
                    var customScheduleJToken = (JProperty)jToken;
                    var customScheduleObject =
                        (CustomSchedule)irrigationJObject[nameof(CustomSchedule)][customScheduleJToken.Name]
                            .ToObject(typeof(CustomSchedule));
                    customScheduleObject.Id = customScheduleJToken.Name;
                    customScheduleList.Add(customScheduleObject);
                }

            if (irrigationJObject.ContainsKey(nameof(Schedule)))
                foreach (var jToken in irrigationJObject[nameof(Schedule)])
                {
                    var scheduleJToken = (JProperty)jToken;
                    var scheduleObject = (Schedule)irrigationJObject[nameof(Schedule)][scheduleJToken.Name]
                        .ToObject(typeof(Schedule));
                    scheduleObject.Id = scheduleJToken.Name;
                    scheduleList.Add(scheduleObject);
                }

            if (irrigationJObject.ContainsKey(nameof(Equipment)))
                foreach (var jToken in irrigationJObject[nameof(Equipment)])
                {
                    var equipmentJToken = (JProperty)jToken;
                    var equipmentObject = (Equipment)irrigationJObject[nameof(Equipment)][equipmentJToken.Name]
                        .ToObject(typeof(Equipment));
                    equipmentObject.Id = equipmentJToken.Name;
                    equipmentList.Add(equipmentObject);
                }

            if (irrigationJObject.ContainsKey(nameof(ManualSchedule)))
                foreach (var jToken in irrigationJObject[nameof(ManualSchedule)])
                {
                    var manualScheduleJToken = (JProperty)jToken;
                    var manualScheduleObject =
                        (ManualSchedule)irrigationJObject[nameof(ManualSchedule)][manualScheduleJToken.Name]
                            .ToObject(typeof(ManualSchedule));
                    manualScheduleObject.Id = manualScheduleJToken.Name;
                    manualScheduleList.Add(manualScheduleObject);
                }

            if (irrigationJObject.ContainsKey(nameof(Sensor)))
                foreach (var jToken in irrigationJObject[nameof(Sensor)])
                {
                    var sensorJToken = (JProperty)jToken;
                    var sensorObject =
                        (Sensor)irrigationJObject[nameof(Sensor)][sensorJToken.Name].ToObject(typeof(Sensor));
                    sensorObject.Id = sensorJToken.Name;
                    sensorList.Add(sensorObject);
                }


            if (irrigationJObject.ContainsKey(nameof(SubController)))
                foreach (var jToken in irrigationJObject[nameof(SubController)])
                {
                    var subControllerJToken = (JProperty)jToken;
                    var subControllerObject =
                        (SubController)irrigationJObject[nameof(SubController)][subControllerJToken.Name]
                            .ToObject(typeof(SubController));
                    subControllerObject.Id = subControllerJToken.Name;
                    subControllerList.Add(subControllerObject);
                }

            return new Tuple<List<CustomSchedule>, List<Schedule>, List<Equipment>, List<ManualSchedule>, List<Sensor>,
                List<SubController>>
            (customScheduleList, scheduleList, equipmentList, manualScheduleList, sensorList,
                subControllerList);
        }


        public static void UpdateObservableIrrigation(ObservableIrrigation observableIrrigation,
            Tuple<List<Dictionary<EditState, CustomSchedule>>, List<Dictionary<EditState, Schedule>>,
                List<Dictionary<EditState, Equipment>>,
                List<Dictionary<EditState, ManualSchedule>>, List<Dictionary<EditState, Sensor>>,
                List<Dictionary<EditState, SubController>>> irrigationTupleEditState)
        {
            if (observableIrrigation.AliveList.Count > 0)
            {
                if (observableIrrigation.AliveList[0] == null)
                {
                    observableIrrigation.AliveList.Clear();
                    observableIrrigation.AliveList.Add(new Alive
                    {
                        RequestedTime = ScheduleTime.GetUnixTimeStampUtcNow() - 1,
                        ResponseTime = ScheduleTime.GetUnixTimeStampUtcNow()
                    });
                }
                else
                {
                    observableIrrigation.AliveList[0] = new Alive
                    {
                        RequestedTime = ScheduleTime.GetUnixTimeStampUtcNow() - 1,
                        ResponseTime = ScheduleTime.GetUnixTimeStampUtcNow()
                    };
                }
            }

            if (observableIrrigation.CustomScheduleList.Count > 0 && observableIrrigation.CustomScheduleList[0] == null)
                observableIrrigation.CustomScheduleList.Clear();

            foreach (var customScheduleEditSate in irrigationTupleEditState.Item1)
                if (customScheduleEditSate.ContainsKey(EditState.Deleted))
                {
                    for (var i = 0; i < observableIrrigation.CustomScheduleList.Count; i++)
                        if (observableIrrigation.CustomScheduleList[i].Id == customScheduleEditSate.Values.First().Id)
                            observableIrrigation.CustomScheduleList.RemoveAt(i);
                }
                else if (customScheduleEditSate.ContainsKey(EditState.Created))
                {
                    observableIrrigation.CustomScheduleList.Add(customScheduleEditSate.Values.First());
                }
                else if (customScheduleEditSate.ContainsKey(EditState.Updated))
                {
                    var index = observableIrrigation.CustomScheduleList.IndexOf(customScheduleEditSate.Values.First());
                    observableIrrigation.CustomScheduleList[index] = customScheduleEditSate.Values.First();
                }

            if (observableIrrigation.ScheduleList.Count > 0 && observableIrrigation.ScheduleList[0] == null)
                observableIrrigation.ScheduleList.Clear();
            foreach (var scheduleEditSate in irrigationTupleEditState.Item2)
                if (scheduleEditSate.ContainsKey(EditState.Deleted))
                {
                    for (var i = 0; i < observableIrrigation.ScheduleList.Count; i++)
                        if (observableIrrigation.ScheduleList[i].Id == scheduleEditSate.Values.First().Id)
                            observableIrrigation.ScheduleList.RemoveAt(i);
                }
                else if (scheduleEditSate.ContainsKey(EditState.Created))
                {
                    observableIrrigation.ScheduleList.Add(scheduleEditSate.Values.First());
                }
                else if (scheduleEditSate.ContainsKey(EditState.Updated))
                {
                    var index = observableIrrigation.ScheduleList.IndexOf(scheduleEditSate.Values.First());
                    observableIrrigation.ScheduleList[index] = scheduleEditSate.Values.First();
                }

            if (observableIrrigation.EquipmentList.Count > 0 && observableIrrigation.EquipmentList[0] == null)
                observableIrrigation.EquipmentList.Clear();
            foreach (var equipmentEditSate in irrigationTupleEditState.Item3)
                if (equipmentEditSate.ContainsKey(EditState.Deleted))
                {
                    for (var i = 0; i < observableIrrigation.EquipmentList.Count; i++)
                        if (observableIrrigation.EquipmentList[i].Id == equipmentEditSate.Values.First().Id)
                            observableIrrigation.EquipmentList.RemoveAt(i);
                }
                else if (equipmentEditSate.ContainsKey(EditState.Created))
                {
                    observableIrrigation.EquipmentList.Add(equipmentEditSate.Values.First());
                }
                else if (equipmentEditSate.ContainsKey(EditState.Updated))
                {
                    var index = observableIrrigation.EquipmentList.IndexOf(equipmentEditSate.Values.First());
                    observableIrrigation.EquipmentList[index] = equipmentEditSate.Values.First();
                }

            if (observableIrrigation.ManualScheduleList.Count > 0 && observableIrrigation.ManualScheduleList[0] == null)
                observableIrrigation.ManualScheduleList.Clear();
            foreach (var manualScheduleEditSate in irrigationTupleEditState.Item4)
                if (manualScheduleEditSate.ContainsKey(EditState.Deleted))
                {
                    for (var i = 0; i < observableIrrigation.ManualScheduleList.Count; i++)
                        if (observableIrrigation.ManualScheduleList[i].Id == manualScheduleEditSate.Values.First().Id)
                            observableIrrigation.ManualScheduleList.RemoveAt(i);
                }
                else if (manualScheduleEditSate.ContainsKey(EditState.Created))
                {
                    observableIrrigation.ManualScheduleList.Add(manualScheduleEditSate.Values.First());
                }
                else if (manualScheduleEditSate.ContainsKey(EditState.Updated))
                {
                    var index = observableIrrigation.ManualScheduleList.IndexOf(manualScheduleEditSate.Values.First());
                    observableIrrigation.ManualScheduleList[index] = manualScheduleEditSate.Values.First();
                }

            if (observableIrrigation.SensorList.Count > 0 && observableIrrigation.SensorList[0] == null)
                observableIrrigation.SensorList.Clear();
            foreach (var sensorEditSate in irrigationTupleEditState.Item5)
                if (sensorEditSate.ContainsKey(EditState.Deleted))
                {
                    for (var i = 0; i < observableIrrigation.SensorList.Count; i++)
                        if (observableIrrigation.SensorList[i].Id == sensorEditSate.Values.First().Id)
                            observableIrrigation.SensorList.RemoveAt(i);
                }
                else if (sensorEditSate.ContainsKey(EditState.Created))
                {
                    observableIrrigation.SensorList.Add(sensorEditSate.Values.First());
                }
                else if (sensorEditSate.ContainsKey(EditState.Updated))
                {
                    var index = observableIrrigation.SensorList.IndexOf(sensorEditSate.Values.First());
                    observableIrrigation.SensorList[index] = sensorEditSate.Values.First();
                }
            
            if (observableIrrigation.SubControllerList.Count > 0 && observableIrrigation.SubControllerList[0] == null)
                observableIrrigation.SubControllerList.Clear();
            
            foreach (var subControllerEditSate in irrigationTupleEditState.Item6)
                if (subControllerEditSate.ContainsKey(EditState.Deleted))
                {
                    for (var i = 0; i < observableIrrigation.SubControllerList.Count; i++)
                        if (observableIrrigation.SubControllerList[i].Id == subControllerEditSate.Values.First().Id)
                            observableIrrigation.SubControllerList.RemoveAt(i);
                }
                else if (subControllerEditSate.ContainsKey(EditState.Created))
                {
                    observableIrrigation.SubControllerList.Add(subControllerEditSate.Values.First());
                }
                else if (subControllerEditSate.ContainsKey(EditState.Updated))
                {
                    var index = observableIrrigation.SubControllerList.IndexOf(subControllerEditSate.Values.First());
                    observableIrrigation.SubControllerList[index] = subControllerEditSate.Values.First();
                }
        }

        public static Tuple<List<Dictionary<EditState, CustomSchedule>>, List<Dictionary<EditState, Schedule>>,
                List<Dictionary<EditState, Equipment>>,
                List<Dictionary<EditState, ManualSchedule>>, List<Dictionary<EditState, Sensor>>,
                List<Dictionary<EditState, SubController>>>
            CheckUpdatedStatus(
                Tuple<List<CustomSchedule>, List<Schedule>, List<Equipment>, List<ManualSchedule>, List<Sensor>,
                    List<SubController>> newIrrigationTuple,
                Tuple<List<CustomSchedule>, List<Schedule>, List<Equipment>, List<ManualSchedule>, List<Sensor>,
                    List<SubController>> oldIrrigationTuple)
        {
            var customScheduleEditState = CheckUpdatedStatus(newIrrigationTuple.Item1, oldIrrigationTuple.Item1);
            var scheduleEditState = CheckUpdatedStatus(newIrrigationTuple.Item2, oldIrrigationTuple.Item2);
            var equipmentEditState = CheckUpdatedStatus(newIrrigationTuple.Item3, oldIrrigationTuple.Item3);
            var manualScheduleEditState = CheckUpdatedStatus(newIrrigationTuple.Item4, oldIrrigationTuple.Item4);
            var sensorEditState = CheckUpdatedStatus(newIrrigationTuple.Item5, oldIrrigationTuple.Item5);
            var subControllerEditState = CheckUpdatedStatus(newIrrigationTuple.Item6, oldIrrigationTuple.Item6);
            return new Tuple<List<Dictionary<EditState, CustomSchedule>>, List<Dictionary<EditState, Schedule>>,
                List<Dictionary<EditState, Equipment>>,
                List<Dictionary<EditState, ManualSchedule>>, List<Dictionary<EditState, Sensor>>,
                 List<Dictionary<EditState, SubController>>>
            (customScheduleEditState, scheduleEditState, equipmentEditState, manualScheduleEditState,
                sensorEditState, subControllerEditState);
        }


        private static List<Dictionary<EditState, CustomSchedule>> CheckUpdatedStatus(
            List<CustomSchedule> newCustomScheduleList, List<CustomSchedule> oldCustomScheduleList)
        {
            var customScheduleState = new List<Dictionary<EditState, CustomSchedule>>();

            foreach (var customSchedules in oldCustomScheduleList.Where(x =>
                         newCustomScheduleList.Select(y => y.Id).Contains(x.Id) == false))
                customScheduleState.Add(new Dictionary<EditState, CustomSchedule>
                    { { EditState.Deleted, customSchedules } });

            foreach (var customSchedules in newCustomScheduleList.Where(x =>
                         oldCustomScheduleList.Select(y => y.Id).Contains(x.Id) == false))
                customScheduleState.Add(new Dictionary<EditState, CustomSchedule>
                    { { EditState.Created, customSchedules } });

            foreach (var newCustomSchedule in newCustomScheduleList)
            foreach (var oldCustomSchedule in oldCustomScheduleList.Where(x => x.Id == newCustomSchedule.Id))
                if (!string.Equals(JObject.FromObject(oldCustomSchedule).ToString(),
                        JObject.FromObject(oldCustomSchedule).ToString(), StringComparison.Ordinal))
                    customScheduleState.Add(new Dictionary<EditState, CustomSchedule>
                        { { EditState.Updated, newCustomSchedule } });

            return customScheduleState;
        }

        private static List<Dictionary<EditState, Schedule>> CheckUpdatedStatus(List<Schedule> newScheduleList,
            List<Schedule> oldScheduleList)
        {
            var scheduleState = new List<Dictionary<EditState, Schedule>>();

            foreach (var schedules in oldScheduleList.Where(x =>
                         newScheduleList.Select(y => y.Id).Contains(x.Id) == false))
                scheduleState.Add(new Dictionary<EditState, Schedule> { { EditState.Deleted, schedules } });

            foreach (var schedules in newScheduleList.Where(x =>
                         oldScheduleList.Select(y => y.Id).Contains(x.Id) == false))
                scheduleState.Add(new Dictionary<EditState, Schedule> { { EditState.Created, schedules } });

            foreach (var newSchedule in newScheduleList)
            foreach (var oldSchedule in oldScheduleList.Where(x => x.Id == newSchedule.Id))
                if (!string.Equals(JObject.FromObject(oldSchedule).ToString(),
                        JObject.FromObject(oldSchedule).ToString(), StringComparison.Ordinal))
                    scheduleState.Add(new Dictionary<EditState, Schedule> { { EditState.Updated, newSchedule } });

            return scheduleState;
        }

        private static List<Dictionary<EditState, Equipment>> CheckUpdatedStatus(List<Equipment> newEquipmentList,
            List<Equipment> oldEquipmentList)
        {
            var equipmentState = new List<Dictionary<EditState, Equipment>>();

            foreach (var equipments in oldEquipmentList.Where(x =>
                         newEquipmentList.Select(y => y.Id).Contains(x.Id) == false))
                equipmentState.Add(new Dictionary<EditState, Equipment> { { EditState.Deleted, equipments } });

            foreach (var equipments in newEquipmentList.Where(x =>
                         oldEquipmentList.Select(y => y.Id).Contains(x.Id) == false))
                equipmentState.Add(new Dictionary<EditState, Equipment> { { EditState.Created, equipments } });

            foreach (var newEquipment in newEquipmentList)
            foreach (var oldEquipment in oldEquipmentList.Where(x => x.Id == newEquipment.Id))
                if (!string.Equals(JObject.FromObject(oldEquipment).ToString(),
                        JObject.FromObject(oldEquipment).ToString(), StringComparison.Ordinal))
                    equipmentState.Add(new Dictionary<EditState, Equipment>
                        { { EditState.Updated, newEquipment } });

            return equipmentState;
        }

        private static List<Dictionary<EditState, ManualSchedule>> CheckUpdatedStatus(
            List<ManualSchedule> newManualScheduleList, List<ManualSchedule> oldManualScheduleList)
        {
            var manualScheduleState = new List<Dictionary<EditState, ManualSchedule>>();

            foreach (var manualSchedules in oldManualScheduleList.Where(x =>
                         newManualScheduleList.Select(y => y.Id).Contains(x.Id) == false))
                manualScheduleState.Add(new Dictionary<EditState, ManualSchedule>
                    { { EditState.Deleted, manualSchedules } });

            foreach (var manualSchedules in newManualScheduleList.Where(x =>
                         oldManualScheduleList.Select(y => y.Id).Contains(x.Id) == false))
                manualScheduleState.Add(new Dictionary<EditState, ManualSchedule>
                    { { EditState.Created, manualSchedules } });

            foreach (var newManualSchedule in newManualScheduleList)
            foreach (var oldManualSchedule in oldManualScheduleList.Where(x => x.Id == newManualSchedule.Id))
                if (!string.Equals(JObject.FromObject(oldManualSchedule).ToString(),
                        JObject.FromObject(oldManualSchedule).ToString(), StringComparison.Ordinal))
                    manualScheduleState.Add(new Dictionary<EditState, ManualSchedule>
                        { { EditState.Updated, newManualSchedule } });

            return manualScheduleState;
        }

        private static List<Dictionary<EditState, Sensor>> CheckUpdatedStatus(List<Sensor> newSensorList,
            List<Sensor> oldSensorList)
        {
            var sensorState = new List<Dictionary<EditState, Sensor>>();

            foreach (var sensors in oldSensorList.Where(x => newSensorList.Select(y => y.Id).Contains(x.Id) == false))
                sensorState.Add(new Dictionary<EditState, Sensor> { { EditState.Deleted, sensors } });

            foreach (var sensors in newSensorList.Where(x => oldSensorList.Select(y => y.Id).Contains(x.Id) == false))
                sensorState.Add(new Dictionary<EditState, Sensor> { { EditState.Created, sensors } });

            foreach (var newSensor in newSensorList)
            foreach (var oldSensor in oldSensorList.Where(x => x.Id == newSensor.Id))
                if (!string.Equals(JObject.FromObject(oldSensor).ToString(),
                        JObject.FromObject(oldSensor).ToString(), StringComparison.Ordinal))
                    sensorState.Add(new Dictionary<EditState, Sensor> { { EditState.Updated, newSensor } });

            return sensorState;
        }

        private static List<Dictionary<EditState, SubController>> CheckUpdatedStatus(
            List<SubController> newSubControllerList, List<SubController> oldSubControllerList)
        {
            var subControllerState = new List<Dictionary<EditState, SubController>>();

            foreach (var subControllers in oldSubControllerList.Where(x =>
                         newSubControllerList.Select(y => y.Id).Contains(x.Id) == false))
                subControllerState.Add(new Dictionary<EditState, SubController>
                    { { EditState.Deleted, subControllers } });

            foreach (var subControllers in newSubControllerList.Where(x =>
                         oldSubControllerList.Select(y => y.Id).Contains(x.Id) == false))
                subControllerState.Add(new Dictionary<EditState, SubController>
                    { { EditState.Created, subControllers } });

            foreach (var newSubController in newSubControllerList)
            foreach (var oldSubController in oldSubControllerList.Where(x => x.Id == newSubController.Id))
                if (!string.Equals(JObject.FromObject(oldSubController).ToString(),
                        JObject.FromObject(oldSubController).ToString(), StringComparison.Ordinal))
                    subControllerState.Add(new Dictionary<EditState, SubController>
                        { { EditState.Updated, newSubController } });

            return subControllerState;
        }
    }

    internal enum EditState
    {
        Created,
        Updated,
        Deleted
    }
}