using System;
using System.Linq;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using Pump.Database;
using Pump.IrrigationController;

namespace Pump.SocketController.Firebase
{
    internal class InitializeFirebase
    {
        private readonly ObservableIrrigation _observableIrrigation;
        /*
        private IDisposable _subscribeAlive;
        private IDisposable _subscribeCustomSchedule;
        private IDisposable _subscribeEquipment;
        private IDisposable _subscribeManualSchedule;
        private IDisposable _subscribeSchedule;
        private IDisposable _subscribeSensor;
        private IDisposable _subscribeSite;
        private IDisposable _subscribeSubController;
*/
        private IDisposable _subscribeFirebase;
        private readonly DatabaseController _database;
        private readonly FirebaseManager _manager;
        private bool _alreadySubscribed;
        
        public InitializeFirebase(FirebaseManager manager, ObservableIrrigation observableIrrigation)
        {
            _observableIrrigation = observableIrrigation;
            _manager = manager;
            _database = new DatabaseController();
        }

        public void SubscribeFirebase()
        {
            if(_alreadySubscribed)
                return;
            _alreadySubscribed = true;
            var configuration = _database.GetControllerConfigurationList();
            _subscribeFirebase = _manager.FirebaseQuery
                .Child(configuration.First().Path)
                .AsObservable<JObject>()
                .Subscribe(x =>
                {
                    /*
                    try
                    {
                        if (_observableIrrigation.SensorList.Count > 0 && _observableIrrigation.SensorList[0] == null)
                            _observableIrrigation.SensorList.Clear();

                        if (x.Object == null && string.IsNullOrEmpty(x.Key))
                        {
                            _observableIrrigation.EquipmentList.Clear();
                            return;
                        }

                        var sensor = x.Object;
                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (var i = 0; i < _observableIrrigation.SensorList.Count; i++)
                                if (_observableIrrigation.SensorList[i].ID == x.Key)
                                    _observableIrrigation.SensorList.RemoveAt(i);
                        }
                        else
                        {
                            var existingSensor = _observableIrrigation.SensorList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingSensor != null)
                            {
                                FirebaseMerger.CopyValues(existingSensor, sensor);
                                var index = _observableIrrigation.SensorList.IndexOf(existingSensor);
                                _observableIrrigation.SensorList[index] = existingSensor;
                            }
                            else
                            {
                                sensor.ID = x.Key;
                                _observableIrrigation.SensorList.Add(sensor);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Sensor Error : {0}", e);
                    }
                    */
                });
        }

        /*
        public void SubscribeFirebase()
        {
            if (new DatabaseController().GetControllerConnectionSelection() == null)
                return;
            var auth = new Authentication();

            _subscribeSensor = auth.FirebaseClient
                .Child(auth.GetConnectedPi() + "/Sensor")
                .AsObservable<Sensor>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_observableIrrigation.SensorList.Count > 0 && _observableIrrigation.SensorList[0] == null)
                            _observableIrrigation.SensorList.Clear();

                        if (x.Object == null && string.IsNullOrEmpty(x.Key))
                        {
                            _observableIrrigation.EquipmentList.Clear();
                            return;
                        }

                        var sensor = x.Object;
                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (var i = 0; i < _observableIrrigation.SensorList.Count; i++)
                                if (_observableIrrigation.SensorList[i].ID == x.Key)
                                    _observableIrrigation.SensorList.RemoveAt(i);
                        }
                        else
                        {
                            var existingSensor = _observableIrrigation.SensorList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingSensor != null)
                            {
                                FirebaseMerger.CopyValues(existingSensor, sensor);
                                var index = _observableIrrigation.SensorList.IndexOf(existingSensor);
                                _observableIrrigation.SensorList[index] = existingSensor;
                            }
                            else
                            {
                                sensor.ID = x.Key;
                                _observableIrrigation.SensorList.Add(sensor);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Sensor Error : {0}", e);
                    }
                });

            _subscribeEquipment = auth.FirebaseClient
                .Child(auth.GetConnectedPi() + "/Equipment")
                .AsObservable<Equipment>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_observableIrrigation.EquipmentList.Count > 0 &&
                            _observableIrrigation.EquipmentList[0] == null)
                            _observableIrrigation.EquipmentList.Clear();
                        if (x.Object == null && string.IsNullOrEmpty(x.Key))
                        {
                            _observableIrrigation.EquipmentList.Clear();
                            return;
                        }

                        var equipment = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (var i = 0; i < _observableIrrigation.EquipmentList.Count; i++)
                                if (_observableIrrigation.EquipmentList[i].ID == x.Key)
                                    _observableIrrigation.EquipmentList.RemoveAt(i);
                        }
                        else
                        {
                            var existingEquipment =
                                _observableIrrigation.EquipmentList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingEquipment != null)
                            {
                                FirebaseMerger.CopyValues(existingEquipment, equipment);
                                var index = _observableIrrigation.EquipmentList.IndexOf(existingEquipment);
                                _observableIrrigation.EquipmentList[index] = existingEquipment;
                            }
                            else
                            {
                                equipment.ID = x.Key;
                                _observableIrrigation.EquipmentList.Add(equipment);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Equipment Error : {0}", e);
                    }
                });

            _subscribeSchedule = auth.FirebaseClient
                .Child(auth.GetConnectedPi() + "/Schedule")
                .AsObservable<Schedule>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_observableIrrigation.ScheduleList.Count > 0 &&
                            _observableIrrigation.ScheduleList[0] == null)
                            _observableIrrigation.ScheduleList.Clear();
                        if (x.Object == null && string.IsNullOrEmpty(x.Key))
                        {
                            _observableIrrigation.ScheduleList.Clear();
                            return;
                        }

                        var schedule = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (var i = 0; i < _observableIrrigation.ScheduleList.Count; i++)
                                if (_observableIrrigation.ScheduleList[i].ID == x.Key)
                                    _observableIrrigation.ScheduleList.RemoveAt(i);
                        }
                        else
                        {
                            var existingSchedule =
                                _observableIrrigation.ScheduleList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingSchedule != null)
                            {
                                FirebaseMerger.CopyValues(existingSchedule, schedule);
                                var index = _observableIrrigation.ScheduleList.IndexOf(existingSchedule);
                                _observableIrrigation.ScheduleList[index] = existingSchedule;
                            }
                            else
                            {
                                schedule.ID = x.Key;
                                _observableIrrigation.ScheduleList.Add(schedule);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Schedule Error : {0}", e);
                    }
                });

            _subscribeCustomSchedule = auth.FirebaseClient
                .Child(auth.GetConnectedPi() + "/CustomSchedule")
                .AsObservable<CustomSchedule>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_observableIrrigation.CustomScheduleList.Count > 0 &&
                            _observableIrrigation.CustomScheduleList[0] == null)
                            _observableIrrigation.CustomScheduleList.Clear();
                        if (x.Object == null && string.IsNullOrEmpty(x.Key))
                        {
                            _observableIrrigation.CustomScheduleList.Clear();
                            return;
                        }

                        var customSchedule = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (var i = 0; i < _observableIrrigation.CustomScheduleList.Count; i++)
                                if (_observableIrrigation.CustomScheduleList[i].ID == x.Key)
                                    _observableIrrigation.CustomScheduleList.RemoveAt(i);
                        }
                        else
                        {
                            var existingCustomSchedule =
                                _observableIrrigation.CustomScheduleList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingCustomSchedule != null)
                            {
                                FirebaseMerger.CopyValues(existingCustomSchedule, customSchedule);
                                var index = _observableIrrigation.CustomScheduleList.IndexOf(existingCustomSchedule);
                                _observableIrrigation.CustomScheduleList[index] = existingCustomSchedule;
                            }
                            else
                            {
                                customSchedule.ID = x.Key;
                                _observableIrrigation.CustomScheduleList.Add(customSchedule);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("CustomSchedule Error : {0}", e);
                    }
                });

            _subscribeManualSchedule = auth.FirebaseClient
                .Child(auth.GetConnectedPi() + "/ManualSchedule")
                .AsObservable<ManualSchedule>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_observableIrrigation.ManualScheduleList.Count > 0 &&
                            _observableIrrigation.ManualScheduleList[0] == null)
                            _observableIrrigation.ManualScheduleList.Clear();
                        if (x.Object == null && string.IsNullOrEmpty(x.Key))
                        {
                            _observableIrrigation.ManualScheduleList.Clear();
                            return;
                        }

                        var manualSchedule = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (var i = 0; i < _observableIrrigation.ManualScheduleList.Count; i++)
                                if (_observableIrrigation.ManualScheduleList[i].ID == x.Key)
                                    _observableIrrigation.ManualScheduleList.RemoveAt(i);
                        }
                        else
                        {
                            var existingManualSchedule =
                                _observableIrrigation.ManualScheduleList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingManualSchedule != null)
                            {
                                FirebaseMerger.CopyValues(existingManualSchedule, manualSchedule);
                                var index = _observableIrrigation.ManualScheduleList.IndexOf(existingManualSchedule);
                                _observableIrrigation.ManualScheduleList[index] = existingManualSchedule;
                            }
                            else
                            {
                                manualSchedule.ID = x.Key;
                                _observableIrrigation.ManualScheduleList.Add(manualSchedule);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("ManualSchedule Error : {0}", e);
                    }
                });

            _subscribeSubController = auth.FirebaseClient
                .Child(auth.GetConnectedPi() + "/SubController")
                .AsObservable<SubController>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_observableIrrigation.SubControllerList.Count > 0 &&
                            _observableIrrigation.SubControllerList[0] == null)
                            _observableIrrigation.SubControllerList.Clear();
                        if (x.Object == null && string.IsNullOrEmpty(x.Key))
                        {
                            _observableIrrigation.SubControllerList.Clear();
                            return;
                        }

                        var subController = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (var i = 0; i < _observableIrrigation.SubControllerList.Count; i++)
                                if (_observableIrrigation.SubControllerList[i].ID == x.Key)
                                    _observableIrrigation.SubControllerList.RemoveAt(i);
                        }
                        else
                        {
                            var existingSubController =
                                _observableIrrigation.SubControllerList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingSubController != null)
                            {
                                FirebaseMerger.CopyValues(existingSubController, subController);
                                var index = _observableIrrigation.SubControllerList.IndexOf(existingSubController);
                                _observableIrrigation.SubControllerList[index] = existingSubController;
                            }
                            else
                            {
                                subController.ID = x.Key;
                                _observableIrrigation.SubControllerList.Add(subController);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("SubController Error : {0}", e);
                    }
                });

            _subscribeAlive = auth.FirebaseClient
                .Child(auth.GetConnectedPi() + "/Alive")
                .AsObservable<Alive>()
                .Subscribe(x =>
                {
                    try
                    {
                        _observableIrrigation.AliveList[0] = x.Object;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });

            _subscribeSite = auth.FirebaseClient
                .Child(auth.GetConnectedPi() + "/Site")
                .AsObservable<Site>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_observableIrrigation.SiteList.Count > 0 && _observableIrrigation.SiteList[0] == null)
                            _observableIrrigation.SiteList.Clear();
                        if (x.Object == null && string.IsNullOrEmpty(x.Key))
                        {
                            _observableIrrigation.SiteList.Clear();
                            return;
                        }

                        var site = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (var i = 0; i < _observableIrrigation.SiteList.Count; i++)
                                if (_observableIrrigation.SiteList[i].Id == x.Key)
                                    _observableIrrigation.SiteList.RemoveAt(i);
                        }
                        else
                        {
                            var existingSite = _observableIrrigation.SiteList.FirstOrDefault(y => y.Id == x.Key);
                            if (existingSite != null)
                            {
                                FirebaseMerger.CopyValues(existingSite, site);
                                var index = _observableIrrigation.SiteList.IndexOf(existingSite);
                                _observableIrrigation.SiteList[index] = existingSite;
                            }
                            else
                            {
                                if (site != null)
                                {
                                    site.Id = x.Key;
                                    _observableIrrigation.SiteList.Add(site);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Site Error : {0}", e);
                    }
                });
        }
*/
        /*
        public void SubscribeFirebase()
        {
            var auth = new Authentication();
            _observableIrrigation.CustomScheduleList.CollectionChanged += CustomScheduleListOnCollectionChanged;
            _subscribeAlive = auth.FirebaseClient
                .Child(auth.GetConnectedPi())
                .AsObservable<object>()
                .Subscribe(x =>
                {
                    try
                    {
                        var irrigationJObject = new JObject{{  x.Key, JObject.FromObject(x.Object)}};
                        var irrigationTuple = IrrigationConvert.IrrigationJObjectToList(irrigationJObject);

                        if (x.Key == nameof(CustomSchedule))
                        {
                            PopulateList(irrigationTuple.Item1);
                        }
                        else if (x.Key == nameof(Schedule))
                        {
                            PopulateList(irrigationTuple.Item2);
                        }
                        else if (x.Key == nameof(Equipment))
                        {
                            PopulateList(irrigationTuple.Item3);
                        }
                        else if (x.Key == nameof(ManualSchedule))
                        {
                            PopulateList(irrigationTuple.Item4);
                        }
                        else if (x.Key == nameof(Sensor))
                        {
                            PopulateList(irrigationTuple.Item5);
                        }
                        else if (x.Key == nameof(Site))
                        {
                            PopulateList(irrigationTuple.Item6);
                        }
                        else if (x.Key == nameof(SubController))
                        {
                            PopulateList(irrigationTuple.Item7);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
        }
        

        private void CustomScheduleListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;
            foreach (CustomSchedule newItem in e.NewItems)
            {
                var test2 = newItem;
            }
        }

        private void PopulateList(List<CustomSchedule> customSchedules)
            {
                if (_observableIrrigation.CustomScheduleList.Contains(null))
                    _observableIrrigation.CustomScheduleList.Clear();
                foreach (var customSchedule in customSchedules)
                {
                    _observableIrrigation.CustomScheduleList.Add(customSchedule);
                }
               
            }
            
            private void PopulateList(List<Schedule> schedules)
            {
                if (_observableIrrigation.ScheduleList.Contains(null))
                    _observableIrrigation.ScheduleList.Clear();
                foreach (var schedule in schedules)
                {
                    _observableIrrigation.ScheduleList.Add(schedule);
                }
            
            }
            
            private void PopulateList(List<Equipment> equipments)
            {
                if (_observableIrrigation.EquipmentList.Contains(null))
                    _observableIrrigation.EquipmentList.Clear();

                foreach (var equipment in equipments)
                {
                    _observableIrrigation.EquipmentList.Add(equipment);
                }
            
            }
            
            private void PopulateList(List<ManualSchedule> manualSchedules)
            {
                if (_observableIrrigation.ManualScheduleList.Contains(null))
                    _observableIrrigation.ManualScheduleList.Clear();
                foreach (var manualSchedule in manualSchedules)
                {
                    _observableIrrigation.ManualScheduleList.Add(manualSchedule);
                }
            
            }
            
            private void PopulateList(List<Sensor> sensors)
            {
                if (_observableIrrigation.SensorList.Contains(null))
                    _observableIrrigation.SensorList.Clear();
                foreach (var sensor in sensors)
                {
                    _observableIrrigation.SensorList.Add(sensor);
                }
            }
            
            private void PopulateList(List<Site> sites)
            {
                if (_observableIrrigation.SiteList.Contains(null))
                    _observableIrrigation.SiteList.Clear();
                foreach (var site in sites)
                {
                    _observableIrrigation.SiteList.Add(site);
                }
            
            }
            
            private void PopulateList(List<SubController> subControllers)
            {
                if (_observableIrrigation.SubControllerList.Contains(null))
                    _observableIrrigation.SubControllerList.Clear();
                foreach (var subController in subControllers)
                {
                    _observableIrrigation.SubControllerList.Add(subController);
                }
            }
        */
        public void Disposable()
        {
            if(!_alreadySubscribed)
                return;
            _alreadySubscribed = false;

            try
            {
                _subscribeFirebase.Dispose();
                /*
                _subscribeSensor?.Dispose();
                _subscribeEquipment?.Dispose();
                _subscribeSchedule?.Dispose();
                _subscribeCustomSchedule?.Dispose();
                _subscribeManualSchedule?.Dispose();
                _subscribeSite?.Dispose();
                _subscribeSubController?.Dispose();
                _subscribeAlive?.Dispose();
                */
            }
            catch
            {
                // ignored
            }

            _observableIrrigation.SensorList.Clear();
            _observableIrrigation.EquipmentList.Clear();
            _observableIrrigation.ManualScheduleList.Clear();
            _observableIrrigation.ScheduleList.Clear();
            _observableIrrigation.CustomScheduleList.Clear();
            _observableIrrigation.SiteList.Clear();
            _observableIrrigation.SubControllerList.Clear();
            _observableIrrigation.AliveList.Clear();

            _observableIrrigation.SensorList.Add(null);
            _observableIrrigation.EquipmentList.Add(null);
            _observableIrrigation.ManualScheduleList.Add(null);
            _observableIrrigation.ScheduleList.Add(null);
            _observableIrrigation.CustomScheduleList.Add(null);
            _observableIrrigation.SiteList.Add(null);
            _observableIrrigation.SubControllerList.Add(null);
            _observableIrrigation.AliveList.Add(null);
        }
    }
}