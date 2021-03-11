using System;
using System.Linq;
using Firebase.Database.Streaming;
using Pump.Database;
using Pump.IrrigationController;

namespace Pump.FirebaseDatabase
{
    class InitializeFirebase
    {
        private readonly ObservableIrrigation _observableIrrigation;

        private IDisposable _subscribeSensor;
        private IDisposable _subscribeEquipment;
        private IDisposable _subscribeSchedule;
        private IDisposable _subscribeCustomSchedule;
        private IDisposable _subscribeManualSchedule;
        private IDisposable _subscribeSite;
        private IDisposable _subscribeSubController;
        private IDisposable _subscribeAlive;
        public InitializeFirebase(ObservableIrrigation observableIrrigation)
        {
            _observableIrrigation = observableIrrigation;
        }
        public void SubscribeFirebase()
        {
            if(new DatabaseController().GetControllerConnectionSelection() == null)
                return;
            var auth = new Authentication();

            _subscribeSensor = auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Sensor")
                .AsObservable<Sensor>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_observableIrrigation.SensorList.Count> 0 && _observableIrrigation.SensorList[0] == null)
                            _observableIrrigation.SensorList.Clear();
                        
                        if (x.Object == null && string.IsNullOrEmpty(x.Key))
                        {
                            _observableIrrigation.EquipmentList.Clear();
                            return;
                        }
                        var sensor = x.Object;
                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _observableIrrigation.SensorList.Count; i++)
                            {
                                if (_observableIrrigation.SensorList[i].ID == x.Key)
                                    _observableIrrigation.SensorList.RemoveAt(i);
                            }
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
                    catch
                    {
                        // ignored
                    }
                });

            _subscribeEquipment = auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Equipment")
                .AsObservable<Equipment>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_observableIrrigation.EquipmentList.Count > 0 && _observableIrrigation.EquipmentList[0] == null)
                            _observableIrrigation.EquipmentList.Clear();
                        if (x.Object == null && string.IsNullOrEmpty(x.Key))
                        {
                            _observableIrrigation.EquipmentList.Clear();
                            return;
                        }
                        var equipment = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _observableIrrigation.EquipmentList.Count; i++)
                            {
                                if (_observableIrrigation.EquipmentList[i].ID == x.Key)
                                    _observableIrrigation.EquipmentList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingEquipment = _observableIrrigation.EquipmentList.FirstOrDefault(y => y.ID == x.Key);
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
                    catch
                    {
                        // ignored
                    }
                });

            _subscribeSchedule = auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Schedule")
                .AsObservable<Schedule>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_observableIrrigation.ScheduleList.Count > 0 && _observableIrrigation.ScheduleList[0] == null)
                            _observableIrrigation.ScheduleList.Clear();
                        if (x.Object == null && string.IsNullOrEmpty(x.Key))
                        {
                            _observableIrrigation.ScheduleList.Clear();
                            return;
                        }
                        var schedule = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _observableIrrigation.ScheduleList.Count; i++)
                            {
                                if (_observableIrrigation.ScheduleList[i].ID == x.Key)
                                    _observableIrrigation.ScheduleList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingSchedule = _observableIrrigation.ScheduleList.FirstOrDefault(y => y.ID == x.Key);
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
                    catch
                    {
                        // ignored
                    }
                });

            _subscribeCustomSchedule = auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/CustomSchedule")
                .AsObservable<CustomSchedule>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_observableIrrigation.CustomScheduleList.Count > 0 && _observableIrrigation.CustomScheduleList[0] == null)
                            _observableIrrigation.CustomScheduleList.Clear();
                        if (x.Object == null && string.IsNullOrEmpty(x.Key))
                        {
                            _observableIrrigation.CustomScheduleList.Clear();
                            return;
                        }
                        var customSchedule = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _observableIrrigation.CustomScheduleList.Count; i++)
                            {
                                if (_observableIrrigation.CustomScheduleList[i].ID == x.Key)
                                    _observableIrrigation.CustomScheduleList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingCustomSchedule = _observableIrrigation.CustomScheduleList.FirstOrDefault(y => y.ID == x.Key);
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
                    catch
                    {
                        // ignored
                    }
                });

            _subscribeManualSchedule = auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/ManualSchedule")
                .AsObservable<ManualSchedule>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_observableIrrigation.ManualScheduleList.Count > 0 && _observableIrrigation.ManualScheduleList[0] == null)
                            _observableIrrigation.ManualScheduleList.Clear();
                        if (x.Object == null && string.IsNullOrEmpty(x.Key))
                        {
                            _observableIrrigation.ManualScheduleList.Clear();
                            return;
                        }
                        var manualSchedule = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _observableIrrigation.ManualScheduleList.Count; i++)
                            {
                                if (_observableIrrigation.ManualScheduleList[i].ID == x.Key)
                                    _observableIrrigation.ManualScheduleList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingManualSchedule = _observableIrrigation.ManualScheduleList.FirstOrDefault(y => y.ID == x.Key);
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
                    catch
                    {
                        // ignored
                    }
                });

            _subscribeSite = auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Site")
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
                            for (int i = 0; i < _observableIrrigation.SiteList.Count; i++)
                            {
                                if (_observableIrrigation.SiteList[i].ID == x.Key)
                                    _observableIrrigation.SiteList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingSite = _observableIrrigation.SiteList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingSite != null)
                            {
                                FirebaseMerger.CopyValues(existingSite, site);
                                var index = _observableIrrigation.SiteList.IndexOf(existingSite);
                                _observableIrrigation.SiteList[index] = existingSite;
                            }
                            else
                            {
                                site.ID = x.Key;
                                _observableIrrigation.SiteList.Add(site);
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                });

            _subscribeSubController = auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/SubController")
                .AsObservable<SubController>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_observableIrrigation.SubControllerList.Count > 0 && _observableIrrigation.SubControllerList[0] == null)
                            _observableIrrigation.SubControllerList.Clear();
                        if (x.Object == null && string.IsNullOrEmpty(x.Key))
                        {
                            _observableIrrigation.SubControllerList.Clear();
                            return;
                        }
                        var subController = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _observableIrrigation.SubControllerList.Count; i++)
                            {
                                if (_observableIrrigation.SubControllerList[i].ID == x.Key)
                                    _observableIrrigation.SubControllerList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingSubController = _observableIrrigation.SubControllerList.FirstOrDefault(y => y.ID == x.Key);
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
                    catch
                    {
                        // ignored
                    }
                });

            _subscribeAlive = auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Alive")
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
        }

        public void Disposable()
        {
            try
            {
                _subscribeSensor?.Dispose();
                _subscribeEquipment?.Dispose();
                _subscribeSchedule?.Dispose();
                _subscribeCustomSchedule?.Dispose();
                _subscribeManualSchedule?.Dispose();
                _subscribeSite?.Dispose();
                _subscribeSubController?.Dispose();
                _subscribeAlive?.Dispose();
            }
            catch
            {
                // ignored
            }

            _observableIrrigation.EquipmentList.Clear();
            _observableIrrigation.SensorList.Clear();
            _observableIrrigation.ManualScheduleList.Clear();
            _observableIrrigation.ScheduleList.Clear();
            _observableIrrigation.CustomScheduleList.Clear();
            _observableIrrigation.SiteList.Clear();
            _observableIrrigation.SubControllerList.Clear();
            _observableIrrigation.AliveList.Clear();

            _observableIrrigation.EquipmentList.Add(null);
            _observableIrrigation.SensorList.Add(null);
            _observableIrrigation.ManualScheduleList.Add(null);
            _observableIrrigation.ScheduleList.Add(null);
            _observableIrrigation.CustomScheduleList.Add(null);
            _observableIrrigation.SiteList.Add(null);
            _observableIrrigation.SubControllerList.Add(null);
            _observableIrrigation.AliveList.Add(null);
        }

    }
}
