using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database.Streaming;
using Newtonsoft.Json.Linq;
using Pump.FirebaseDatabase;

namespace Pump.IrrigationController
{
    static class IrrigationConvert
    {
        public static Tuple<List<CustomSchedule>, List<Schedule>, List<Equipment>, List<ManualSchedule>, List<Sensor>, List<Site>, List<SubController>>  IrrigationJObjectToList(JObject irrigationJObject)
        {                
            var customScheduleList = new List<CustomSchedule>();
            var scheduleList = new List<Schedule>();
            var equipmentList = new List<Equipment>();
            var manualScheduleList = new List<ManualSchedule>();
            var sensorList = new List<Sensor>();
            var siteList = new List<Site>();
            var subControllerList = new List<SubController>();

            if (irrigationJObject.ContainsKey("CustomSchedule"))
            {
                foreach (var jToken in irrigationJObject["CustomSchedule"])
                {
                    var customScheduleJToken = (JProperty)jToken;
                    var customScheduleObject = (CustomSchedule)irrigationJObject["CustomSchedule"][customScheduleJToken.Name].ToObject(typeof(CustomSchedule));
                    customScheduleObject.ID = customScheduleJToken.Name;
                    customScheduleList.Add(customScheduleObject);
                }
            }

            if (irrigationJObject.ContainsKey("Schedule"))
            {
                foreach (var jToken in irrigationJObject["Schedule"])
                {
                    var scheduleJToken = (JProperty)jToken;
                    var scheduleObject = (Schedule)irrigationJObject["Schedule"][scheduleJToken.Name].ToObject(typeof(Schedule));
                    scheduleObject.ID = scheduleJToken.Name;
                    scheduleList.Add(scheduleObject);
                }
            }

            if (irrigationJObject.ContainsKey("Equipment"))
            {
                foreach (var jToken in irrigationJObject["Equipment"])
                {
                    var equipmentJToken = (JProperty)jToken;
                    var equipmentObject = (Equipment)irrigationJObject["Equipment"][equipmentJToken.Name].ToObject(typeof(Equipment));
                    equipmentObject.ID = equipmentJToken.Name;
                    equipmentList.Add(equipmentObject);
                }
            }

            if (irrigationJObject.ContainsKey("ManualSchedule"))
            {
                foreach (var jToken in irrigationJObject["ManualSchedule"])
                {
                    var manualScheduleJToken = (JProperty)jToken;
                    var manualScheduleObject = (ManualSchedule)irrigationJObject["ManualSchedule"][manualScheduleJToken.Name].ToObject(typeof(ManualSchedule));
                    manualScheduleObject.ID = manualScheduleJToken.Name;
                    manualScheduleList.Add(manualScheduleObject);
                }
            }

            if (irrigationJObject.ContainsKey("Sensor"))
            {
                foreach (var jToken in irrigationJObject["Sensor"])
                {
                    var sensorJToken = (JProperty)jToken;
                    var sensorObject = (Sensor)irrigationJObject["Sensor"][sensorJToken.Name].ToObject(typeof(Sensor));
                    sensorObject.ID = sensorJToken.Name;
                    sensorList.Add(sensorObject);
                }
            }


            if (irrigationJObject.ContainsKey("Site"))
            {
                foreach (var jToken in irrigationJObject["Site"])
                {
                    var siteJToken = (JProperty)jToken;
                    var siteObject = (Site)irrigationJObject["Site"][siteJToken.Name].ToObject(typeof(Site));
                    siteObject.ID = siteJToken.Name;
                    siteList.Add(siteObject);
                }
            }

            if (irrigationJObject.ContainsKey("SubController"))
            {
                foreach (var jToken in irrigationJObject["SubController"])
                {
                    var subControllerJToken = (JProperty)jToken;
                    var subControllerObject = (SubController)irrigationJObject["SubController"][subControllerJToken.Name].ToObject(typeof(SubController));
                    subControllerObject.ID = subControllerJToken.Name;
                    subControllerList.Add(subControllerObject);
                }
            }
            
            return new Tuple<List<CustomSchedule>, List<Schedule>, List<Equipment>, List<ManualSchedule>, List<Sensor>, List<Site>, List<SubController>>
                (customScheduleList, scheduleList, equipmentList, manualScheduleList, sensorList, siteList, subControllerList);
        }

        public static void UpdateObservableIrrigation(ObservableIrrigation observableIrrigation, Tuple<List<Dictionary<EditState, CustomSchedule>>, List<Dictionary<EditState, Schedule>>, List<Dictionary<EditState, Equipment>>,
            List<Dictionary<EditState, ManualSchedule>>, List<Dictionary<EditState, Sensor>>, List<Dictionary<EditState, Site>>, List<Dictionary<EditState, SubController>>> irrigationTupleEditState)
        {

            if (observableIrrigation.CustomScheduleList.Count > 0 && observableIrrigation.CustomScheduleList[0] == null)
                observableIrrigation.CustomScheduleList.Clear();

            foreach (var customScheduleEditSate in irrigationTupleEditState.Item1)
            {
                if (customScheduleEditSate.ContainsKey(EditState.Deleted))
                {
                    for (int i = 0; i < observableIrrigation.CustomScheduleList.Count; i++)
                    {
                        if (observableIrrigation.CustomScheduleList[i].ID == customScheduleEditSate.Values.First().ID)
                            observableIrrigation.CustomScheduleList.RemoveAt(i);
                    }
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
            }

            if (observableIrrigation.ScheduleList.Count > 0 && observableIrrigation.ScheduleList[0] == null)
                observableIrrigation.ScheduleList.Clear();
            foreach (var scheduleEditSate in irrigationTupleEditState.Item2)
            {
                if (scheduleEditSate.ContainsKey(EditState.Deleted))
                {
                    for (int i = 0; i < observableIrrigation.ScheduleList.Count; i++)
                    {
                        if (observableIrrigation.ScheduleList[i].ID == scheduleEditSate.Values.First().ID)
                            observableIrrigation.ScheduleList.RemoveAt(i);
                    }
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
            }

            if (observableIrrigation.EquipmentList.Count > 0 && observableIrrigation.EquipmentList[0] == null)
                observableIrrigation.EquipmentList.Clear();
            foreach (var equipmentEditSate in irrigationTupleEditState.Item3)
            {
                if (equipmentEditSate.ContainsKey(EditState.Deleted))
                {
                    for (int i = 0; i < observableIrrigation.EquipmentList.Count; i++)
                    {
                        if (observableIrrigation.EquipmentList[i].ID == equipmentEditSate.Values.First().ID)
                            observableIrrigation.EquipmentList.RemoveAt(i);
                    }
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
            }

            if (observableIrrigation.ManualScheduleList.Count > 0 && observableIrrigation.ManualScheduleList[0] == null)
                observableIrrigation.ManualScheduleList.Clear();
            foreach (var manualScheduleEditSate in irrigationTupleEditState.Item4)
            {
                if (manualScheduleEditSate.ContainsKey(EditState.Deleted))
                {
                    for (int i = 0; i < observableIrrigation.ManualScheduleList.Count; i++)
                    {
                        if (observableIrrigation.ManualScheduleList[i].ID == manualScheduleEditSate.Values.First().ID)
                            observableIrrigation.ManualScheduleList.RemoveAt(i);
                    }
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
            }

            if (observableIrrigation.SensorList.Count > 0 && observableIrrigation.SensorList[0] == null)
                observableIrrigation.SensorList.Clear();
            foreach (var sensorEditSate in irrigationTupleEditState.Item5)
            {
                if (sensorEditSate.ContainsKey(EditState.Deleted))
                {
                    for (int i = 0; i < observableIrrigation.SensorList.Count; i++)
                    {
                        if (observableIrrigation.SensorList[i].ID == sensorEditSate.Values.First().ID)
                            observableIrrigation.SensorList.RemoveAt(i);
                    }
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
            }

            if (observableIrrigation.SiteList.Count > 0 && observableIrrigation.SiteList[0] == null)
                observableIrrigation.SiteList.Clear();
            foreach (var siteEditSate in irrigationTupleEditState.Item6)
            {
                if (siteEditSate.ContainsKey(EditState.Deleted))
                {
                    for (int i = 0; i < observableIrrigation.SiteList.Count; i++)
                    {
                        if (observableIrrigation.SiteList[i].ID == siteEditSate.Values.First().ID)
                            observableIrrigation.SiteList.RemoveAt(i);
                    }
                }
                else if (siteEditSate.ContainsKey(EditState.Created))
                {
                    observableIrrigation.SiteList.Add(siteEditSate.Values.First());
                }
                else if (siteEditSate.ContainsKey(EditState.Updated))
                {
                    var index = observableIrrigation.SiteList.IndexOf(siteEditSate.Values.First());
                    observableIrrigation.SiteList[index] = siteEditSate.Values.First();
                }
            }

            if (observableIrrigation.SubControllerList.Count > 0 && observableIrrigation.SubControllerList[0] == null)
                observableIrrigation.SubControllerList.Clear();
            foreach (var subControllerEditSate in irrigationTupleEditState.Item7)
            {
                if (subControllerEditSate.ContainsKey(EditState.Deleted))
                {
                    for (int i = 0; i < observableIrrigation.SubControllerList.Count; i++)
                    {
                        if (observableIrrigation.SubControllerList[i].ID == subControllerEditSate.Values.First().ID)
                            observableIrrigation.SubControllerList.RemoveAt(i);
                    }
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
        }

        public static Tuple<List<Dictionary<EditState, CustomSchedule>>, List<Dictionary<EditState, Schedule>>, List<Dictionary<EditState, Equipment>>, 
            List<Dictionary<EditState, ManualSchedule>>, List<Dictionary<EditState, Sensor>>, List<Dictionary<EditState, Site>>, List<Dictionary<EditState, SubController>>> 
            CheckUpdatedStatus(Tuple<List<CustomSchedule>, List<Schedule>, List<Equipment>, List<ManualSchedule>, List<Sensor>, List<Site>, List<SubController>> newIrrigationTuple, Tuple<List<CustomSchedule>, List<Schedule>, List<Equipment>, List<ManualSchedule>, List<Sensor>, List<Site>, List<SubController>> oldIrrigationTuple)
        {
            var customScheduleEditState = CheckUpdatedStatus(newIrrigationTuple.Item1, oldIrrigationTuple.Item1);
            var scheduleEditState = CheckUpdatedStatus(newIrrigationTuple.Item2, oldIrrigationTuple.Item2);
            var equipmentEditState = CheckUpdatedStatus(newIrrigationTuple.Item3, oldIrrigationTuple.Item3);
            var manualScheduleEditState = CheckUpdatedStatus(newIrrigationTuple.Item4, oldIrrigationTuple.Item4);
            var sensorEditState = CheckUpdatedStatus(newIrrigationTuple.Item5, oldIrrigationTuple.Item5);
            var siteEditState = CheckUpdatedStatus(newIrrigationTuple.Item6, oldIrrigationTuple.Item6);
            var subControllerEditState = CheckUpdatedStatus(newIrrigationTuple.Item7, oldIrrigationTuple.Item7);

            return new Tuple<List<Dictionary<EditState, CustomSchedule>>, List<Dictionary<EditState, Schedule>>, List<Dictionary<EditState, Equipment>>,
                List<Dictionary<EditState, ManualSchedule>>, List<Dictionary<EditState, Sensor>>, List<Dictionary<EditState, Site>>, List<Dictionary<EditState, SubController>>>
                (customScheduleEditState, scheduleEditState, equipmentEditState, manualScheduleEditState, sensorEditState, siteEditState, subControllerEditState);
        }


        private static List<Dictionary<EditState, CustomSchedule>> CheckUpdatedStatus(List<CustomSchedule> newCustomScheduleList, List<CustomSchedule> oldCustomScheduleList)
        {
            var CustomScheduleState = new List<Dictionary<EditState, CustomSchedule>>();
            
            foreach (var customSchedules in oldCustomScheduleList.Where(x => newCustomScheduleList.Select(y => y.ID).Contains(x.ID) == false))
            {
                CustomScheduleState.Add(new Dictionary<EditState, CustomSchedule> {{EditState.Deleted, customSchedules}});
            }

            foreach (var customSchedules in newCustomScheduleList.Where(x => oldCustomScheduleList.Select(y => y.ID).Contains(x.ID) == false))
            {
                CustomScheduleState.Add(new Dictionary<EditState, CustomSchedule> { { EditState.Created, customSchedules } });
            }

            foreach (var newCustomSchedule in newCustomScheduleList)
            {
                foreach (var oldCustomSchedule in oldCustomScheduleList.Where(x => x.ID == newCustomSchedule.ID))
                {
                    if (!string.Equals(JObject.FromObject(oldCustomSchedule).ToString(),
                        JObject.FromObject(oldCustomSchedule).ToString(), StringComparison.Ordinal))
                    {
                        CustomScheduleState.Add(new Dictionary<EditState, CustomSchedule> {{ EditState.Updated, newCustomSchedule }});
                    }
                }
            }

            return CustomScheduleState;
        }

        private static List<Dictionary<EditState, Schedule>> CheckUpdatedStatus(List<Schedule> newScheduleList, List<Schedule> oldScheduleList)
        {
            var ScheduleState = new List<Dictionary<EditState, Schedule>>();

            foreach (var schedules in oldScheduleList.Where(x => newScheduleList.Select(y => y.ID).Contains(x.ID) == false))
            {
                ScheduleState.Add(new Dictionary<EditState, Schedule> { { EditState.Deleted, schedules } });
            }

            foreach (var schedules in newScheduleList.Where(x => oldScheduleList.Select(y => y.ID).Contains(x.ID) == false))
            {
                ScheduleState.Add(new Dictionary<EditState, Schedule> { { EditState.Created, schedules } });
            }

            foreach (var newSchedule in newScheduleList)
            {
                foreach (var oldSchedule in oldScheduleList.Where(x => x.ID == newSchedule.ID))
                {
                    if (!string.Equals(JObject.FromObject(oldSchedule).ToString(),
                        JObject.FromObject(oldSchedule).ToString(), StringComparison.Ordinal))
                    {
                        ScheduleState.Add(new Dictionary<EditState, Schedule> { { EditState.Updated, newSchedule } });
                    }
                }
            }

            return ScheduleState;
        }

        private static List<Dictionary<EditState, Equipment>> CheckUpdatedStatus(List<Equipment> newEquipmentList, List<Equipment> oldEquipmentList)
        {
            var EquipmentState = new List<Dictionary<EditState, Equipment>>();

            foreach (var Equipments in oldEquipmentList.Where(x => newEquipmentList.Select(y => y.ID).Contains(x.ID) == false))
            {
                EquipmentState.Add(new Dictionary<EditState, Equipment> { { EditState.Deleted, Equipments } });
            }

            foreach (var Equipments in newEquipmentList.Where(x => oldEquipmentList.Select(y => y.ID).Contains(x.ID) == false))
            {
                EquipmentState.Add(new Dictionary<EditState, Equipment> { { EditState.Created, Equipments } });
            }

            foreach (var newEquipment in newEquipmentList)
            {
                foreach (var oldEquipment in oldEquipmentList.Where(x => x.ID == newEquipment.ID))
                {
                    if (!string.Equals(JObject.FromObject(oldEquipment).ToString(),
                        JObject.FromObject(oldEquipment).ToString(), StringComparison.Ordinal))
                    {
                        EquipmentState.Add(new Dictionary<EditState, Equipment> { { EditState.Updated, newEquipment } });
                    }
                }
            }

            return EquipmentState;
        }

        private static List<Dictionary<EditState, ManualSchedule>> CheckUpdatedStatus(List<ManualSchedule> newManualScheduleList, List<ManualSchedule> oldManualScheduleList)
        {
            var ManualScheduleState = new List<Dictionary<EditState, ManualSchedule>>();

            foreach (var ManualSchedules in oldManualScheduleList.Where(x => newManualScheduleList.Select(y => y.ID).Contains(x.ID) == false))
            {
                ManualScheduleState.Add(new Dictionary<EditState, ManualSchedule> { { EditState.Deleted, ManualSchedules } });
            }

            foreach (var ManualSchedules in newManualScheduleList.Where(x => oldManualScheduleList.Select(y => y.ID).Contains(x.ID) == false))
            {
                ManualScheduleState.Add(new Dictionary<EditState, ManualSchedule> { { EditState.Created, ManualSchedules } });
            }

            foreach (var newManualSchedule in newManualScheduleList)
            {
                foreach (var oldManualSchedule in oldManualScheduleList.Where(x => x.ID == newManualSchedule.ID))
                {
                    if (!string.Equals(JObject.FromObject(oldManualSchedule).ToString(),
                        JObject.FromObject(oldManualSchedule).ToString(), StringComparison.Ordinal))
                    {
                        ManualScheduleState.Add(new Dictionary<EditState, ManualSchedule> { { EditState.Updated, newManualSchedule } });
                    }
                }
            }

            return ManualScheduleState;
        }

        private static List<Dictionary<EditState, Sensor>> CheckUpdatedStatus(List<Sensor> newSensorList, List<Sensor> oldSensorList)
        {
            var SensorState = new List<Dictionary<EditState, Sensor>>();

            foreach (var Sensors in oldSensorList.Where(x => newSensorList.Select(y => y.ID).Contains(x.ID) == false))
            {
                SensorState.Add(new Dictionary<EditState, Sensor> { { EditState.Deleted, Sensors } });
            }

            foreach (var Sensors in newSensorList.Where(x => oldSensorList.Select(y => y.ID).Contains(x.ID) == false))
            {
                SensorState.Add(new Dictionary<EditState, Sensor> { { EditState.Created, Sensors } });
            }

            foreach (var newSensor in newSensorList)
            {
                foreach (var oldSensor in oldSensorList.Where(x => x.ID == newSensor.ID))
                {
                    if (!string.Equals(JObject.FromObject(oldSensor).ToString(),
                        JObject.FromObject(oldSensor).ToString(), StringComparison.Ordinal))
                    {
                        SensorState.Add(new Dictionary<EditState, Sensor> { { EditState.Updated, newSensor } });
                    }
                }
            }

            return SensorState;
        }

        private static List<Dictionary<EditState, Site>> CheckUpdatedStatus(List<Site> newSiteList, List<Site> oldSiteList)
        {
            var SiteState = new List<Dictionary<EditState, Site>>();

            foreach (var Sites in oldSiteList.Where(x => newSiteList.Select(y => y.ID).Contains(x.ID) == false))
            {
                SiteState.Add(new Dictionary<EditState, Site> { { EditState.Deleted, Sites } });
            }

            foreach (var Sites in newSiteList.Where(x => oldSiteList.Select(y => y.ID).Contains(x.ID) == false))
            {
                SiteState.Add(new Dictionary<EditState, Site> { { EditState.Created, Sites } });
            }

            foreach (var newSite in newSiteList)
            {
                foreach (var oldSite in oldSiteList.Where(x => x.ID == newSite.ID))
                {
                    if (!string.Equals(JObject.FromObject(oldSite).ToString(),
                        JObject.FromObject(oldSite).ToString(), StringComparison.Ordinal))
                    {
                        SiteState.Add(new Dictionary<EditState, Site> { { EditState.Updated, newSite } });
                    }
                }
            }

            return SiteState;
        }
        private static List<Dictionary<EditState, SubController>> CheckUpdatedStatus(List<SubController> newSubControllerList, List<SubController> oldSubControllerList)
        {
            var SubControllerState = new List<Dictionary<EditState, SubController>>();

            foreach (var SubControllers in oldSubControllerList.Where(x => newSubControllerList.Select(y => y.ID).Contains(x.ID) == false))
            {
                SubControllerState.Add(new Dictionary<EditState, SubController> { { EditState.Deleted, SubControllers } });
            }

            foreach (var SubControllers in newSubControllerList.Where(x => oldSubControllerList.Select(y => y.ID).Contains(x.ID) == false))
            {
                SubControllerState.Add(new Dictionary<EditState, SubController> { { EditState.Created, SubControllers } });
            }

            foreach (var newSubController in newSubControllerList)
            {
                foreach (var oldSubController in oldSubControllerList.Where(x => x.ID == newSubController.ID))
                {
                    if (!string.Equals(JObject.FromObject(oldSubController).ToString(),
                        JObject.FromObject(oldSubController).ToString(), StringComparison.Ordinal))
                    {
                        SubControllerState.Add(new Dictionary<EditState, SubController> { { EditState.Updated, newSubController } });
                    }
                }
            }

            return SubControllerState;
        }
    }

    internal enum EditState
    {
        Created,
        Updated,
        Deleted
    }
}

