using System;
using System.Collections.ObjectModel;
using System.Linq;
using Firebase.Database.Streaming;
using Newtonsoft.Json.Linq;
using Pump.IrrigationController;

namespace Pump.FirebaseDatabase
{
    class InitializeFirebase
    {
        private readonly ObservableCollection<Equipment> _equipmentList;
        private readonly ObservableCollection<Alive> _aliveList;
        private readonly ObservableCollection<Sensor> _sensorList;
        private readonly ObservableCollection<ManualSchedule> _manualScheduleList;
        private readonly ObservableCollection<Schedule> _scheduleList;
        private readonly ObservableCollection<CustomSchedule> _customScheduleList;
        private readonly ObservableCollection<Site> _siteList;
        private readonly ObservableCollection<SubController> _subController;

        private IDisposable _subscribeSensor;
        private IDisposable _subscribeEquipment;
        private IDisposable _subscribeSchedule;
        private IDisposable _subscribeCustomSchedule;
        private IDisposable _subscribeManualSchedule;
        private IDisposable _subscribeSite;
        private IDisposable _subscribeSubController;
        private IDisposable _subscribeAlive;
        public InitializeFirebase(ObservableCollection<Equipment> equipmentList ,ObservableCollection<Sensor> sensorList, 
            ObservableCollection<ManualSchedule> manualScheduleList, ObservableCollection<Schedule> scheduleList,
            ObservableCollection<CustomSchedule> customScheduleList, ObservableCollection<Site> siteList, ObservableCollection<Alive> aliveList,
                ObservableCollection<SubController> subController)
        {
            _equipmentList = equipmentList;
            _sensorList = sensorList;
            _manualScheduleList = manualScheduleList;
            _scheduleList = scheduleList;
            _customScheduleList = customScheduleList;
            _siteList = siteList;
            _aliveList = aliveList;
            _subController = subController;
            SubscribeFirebase();
        }
        public void SubscribeFirebase()
        {
            var auth = new Authentication();

            _subscribeSensor = auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Sensor")
                .AsObservable<Sensor>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_sensorList.Count> 0 && _sensorList[0] == null)
                            _sensorList.Clear();
                        var sensor = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _sensorList.Count; i++)
                            {
                                if (_sensorList[i].ID == x.Key)
                                    _sensorList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingSensor = _sensorList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingSensor != null)
                            {
                                FirebaseMerger.CopyValues(existingSensor, sensor);
                            }
                            else
                            {
                                sensor.ID = x.Key;
                                _sensorList.Add(sensor);
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
                        if (_equipmentList.Count > 0 && _equipmentList[0] == null)
                            _equipmentList.Clear();
                        var equipment = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _equipmentList.Count; i++)
                            {
                                if (_equipmentList[i].ID == x.Key)
                                    _equipmentList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingEquipment = _equipmentList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingEquipment != null)
                            {
                                FirebaseMerger.CopyValues(existingEquipment, equipment);
                            }
                            else
                            {
                                equipment.ID = x.Key;
                                _equipmentList.Add(equipment);
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
                        if (_scheduleList.Count > 0 && _scheduleList[0] == null)
                            _scheduleList.Clear();
                        var schedule = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _scheduleList.Count; i++)
                            {
                                if (_scheduleList[i].ID == x.Key)
                                    _scheduleList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingSchedule = _scheduleList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingSchedule != null)
                            {
                                FirebaseMerger.CopyValues(existingSchedule, schedule);
                            }
                            else
                            {
                                schedule.ID = x.Key;
                                _scheduleList.Add(schedule);
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
                        if (_customScheduleList.Count > 0 && _customScheduleList[0] == null)
                            _customScheduleList.Clear();
                        var customSchedule = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _customScheduleList.Count; i++)
                            {
                                if (_customScheduleList[i].ID == x.Key)
                                    _customScheduleList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingCustomSchedule = _customScheduleList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingCustomSchedule != null)
                            {
                                FirebaseMerger.CopyValues(existingCustomSchedule, customSchedule);
                            }
                            else
                            {
                                customSchedule.ID = x.Key;
                                _customScheduleList.Add(customSchedule);
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
                        if (_manualScheduleList.Count > 0 && _manualScheduleList[0] == null)
                            _manualScheduleList.Clear();
                        var manualSchedule = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _manualScheduleList.Count; i++)
                            {
                                if (_manualScheduleList[i].ID == x.Key)
                                    _manualScheduleList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingManualSchedule = _manualScheduleList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingManualSchedule != null)
                            {
                                FirebaseMerger.CopyValues(existingManualSchedule, manualSchedule);
                            }
                            else
                            {
                                manualSchedule.ID = x.Key;
                                _manualScheduleList.Add(manualSchedule);
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
                        if (_siteList.Count > 0 && _siteList[0] == null)
                            _siteList.Clear();
                        var site = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _siteList.Count; i++)
                            {
                                if (_siteList[i].ID == x.Key)
                                    _siteList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingSite = _siteList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingSite != null)
                            {
                                FirebaseMerger.CopyValues(existingSite, site);
                            }
                            else
                            {
                                site.ID = x.Key;
                                _siteList.Add(site);
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
                        if (_subController.Count > 0 && _subController[0] == null)
                            _subController.Clear();
                        var subController = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _subController.Count; i++)
                            {
                                if (_subController[i].ID == x.Key)
                                    _subController.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingSubController = _subController.FirstOrDefault(y => y.ID == x.Key);
                            if (existingSubController != null)
                            {
                                FirebaseMerger.CopyValues(existingSubController, subController);
                            }
                            else
                            {
                                subController.ID = x.Key;
                                _subController.Add(subController);
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
                        if (x.Object == null) return;
                        var test = x.Key;
                        //_aliveList[0] = x.Object;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
        }

        public void Disposable()
        {
            _subscribeSensor.Dispose();
            _subscribeEquipment.Dispose();
            _subscribeSchedule.Dispose();
            _subscribeCustomSchedule.Dispose();
            _subscribeManualSchedule.Dispose();
            _subscribeSite.Dispose();
            _subscribeSubController.Dispose();
            _subscribeAlive.Dispose();

            _equipmentList.Clear();
            _sensorList.Clear();
            _manualScheduleList.Clear();
            _scheduleList.Clear();
            _customScheduleList.Clear();
            _siteList.Clear();
            _subController.Clear();
            _aliveList.Clear();

            _equipmentList.Add(null);
            _sensorList.Add(null);
            _manualScheduleList.Add(null);
            _scheduleList.Add(null);
            _customScheduleList.Add(null);
            _siteList.Add(null);
            _subController.Add(null);
            _aliveList.Add(null);

        }

    }
}
