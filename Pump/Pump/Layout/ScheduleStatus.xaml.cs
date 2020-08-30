﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pump.Database;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScheduleStatus : ContentPage
    {
        private readonly SocketCommands _command = new SocketCommands();
        private readonly SocketMessage _socket = new SocketMessage();
        private readonly List<Equipment> _equipmentList = new List<Equipment>();
        private readonly List<IrrigationController.ManualSchedule> _manualScheduleList = new List<IrrigationController.ManualSchedule>();
        private string _oldActiveSchedule;
        private string _oldActiveSensorStatus;
        private string _oldQueueActiveSchedule;
        private readonly List<Schedule> _schedulesList = new List<Schedule>();
        private List<Sensor> _sensorList = new List<Sensor>();

        public ScheduleStatus()
        {
            InitializeComponent();
            new Thread(ThreadController).Start();
        }


        private void GetScheduleReadingFirebase(DatabaseController databaseController)
        {
            var auth = new Authentication();
            //_schedulesList = Task.Run(() => auth.GetAllSchedules()).Result;
            

            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Schedule")
                .AsObservable<JObject>()
                .Subscribe(x =>
                {
                    var schedule = auth.GetJsonSchedulesToObjectList(x.Object, x.Key);
                    _schedulesList.RemoveAll(y => y.ID == schedule.ID);
                    _schedulesList.Add(schedule);
                });


            //_equipmentList = Task.Run(() => auth.GetAllEquipment()).Result;

            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Equipment")
                .AsObservable<JObject>()
                .Subscribe(x =>
                {
                    var equipment = auth.GetJsonEquipmentToObjectList(x.Object, x.Key);
                    _equipmentList.RemoveAll(y => y.ID == equipment.ID);
                    _equipmentList.Add(equipment);
                });

            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/ManualSchedule")
                .AsObservable<JObject>()
                .Subscribe(x =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (x.Object != null)
                        {
                            var manualSchedule = auth.GetJsonManualSchedulesToObjectList(x.Object, x.Key);
                            _manualScheduleList.Clear();
                            _manualScheduleList.Add(manualSchedule);
                        }
                        else
                        {
                            _manualScheduleList.Clear();
                        }

                    });
                });

            var oldActiveScheduleString = "-999";
            var oldQueScheduleString = "-999";

            while (databaseController.IsRealtimeFirebaseSelected())
            {
                var runningSchedule = new RunningSchedule();

                var queSchedules =
                    runningSchedule.GetQueSchedule(runningSchedule.GetActiveSchedule(_schedulesList, _equipmentList));
                var activeSchedule =
                    runningSchedule.GetRunningSchedule(
                        runningSchedule.GetActiveSchedule(_schedulesList, _equipmentList));


                var activeScheduleString = activeSchedule.Aggregate("",
                    (current, schedule) => current + (schedule.ID + ',' + schedule.NAME + ',' + schedule.name_Pump +
                                                      ',' + schedule.name_Equipment + ',' + schedule.StartTime + ',' +
                                                      schedule.EndTime + '#'));

                if (_manualScheduleList.Count > 0)
                    activeScheduleString = _manualScheduleList[0].DURATION + ',' + _manualScheduleList[0].RunWithSchedule + '$' + activeScheduleString;
                var activeScheduleObjects = GetScheduleDetailObject(activeScheduleString);


                if (oldActiveScheduleString != activeScheduleString)
                {

                    _oldActiveSchedule = activeScheduleString;
                    oldActiveScheduleString = activeScheduleString;
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ScrollViewScheduleStatus.Children.Clear();
                        foreach (View view in activeScheduleObjects) ScrollViewScheduleStatus.Children.Add(view);
                    });
                }

                var queScheduleString = queSchedules.Aggregate("",
                    (current, schedule) => current + (schedule.ID + ',' + schedule.NAME + ',' + schedule.name_Pump +
                                                      ',' + schedule.name_Equipment + ',' + schedule.StartTime + ',' +
                                                      schedule.EndTime + '#'));
                var queScheduleObjects = GetQueueScheduleDetailObject(queScheduleString);
                if (oldQueScheduleString != queScheduleString)
                {
                    _oldQueueActiveSchedule = queScheduleString;
                    oldQueScheduleString = queScheduleString;
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ScrollViewQueueStatus.Children.Clear();
                        foreach (View view in queScheduleObjects) ScrollViewQueueStatus.Children.Add(view);
                    });
                }

                Thread.Sleep(2000);
            }

        }

        private void GetSensorReadingFirebase(DatabaseController databaseController)
        {
            var auth = new Authentication();
            _sensorList = Task.Run(() => auth.GetAllSensors()).Result;


            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Sensor")
                .AsObservable<JObject>()
                .Subscribe(x =>
                {
                    try
                    {
                        var sensor = auth.GetJsonSensorToObjectList(x.Object, x.Key);
                        
                        foreach (var oldSensor in _sensorList.Where(oldSensor => oldSensor.ID == x.Key))
                        {
                            var tempLastReading = sensor.LastReading;
                            sensor = oldSensor;
                            sensor.LastReading = tempLastReading;
                        }

                        _sensorList.RemoveAll(y => y.ID == sensor.ID);
                        _sensorList.Add(sensor);
                        UpdateSensorReading();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    
                });
            //UpdateSensorReading();
        }

        private void UpdateSensorReading()
        {
            var sensorDetailString = _sensorList.Aggregate("",
                (current, sensor) =>
                    current + (sensor.ID + ',' + sensor.TYPE + ',' + sensor.NAME + ',' + sensor.LastReading + '#'));

            _oldActiveSensorStatus = sensorDetailString;
            var sensorStatusObject = GetSensorStatusObject(sensorDetailString);
            Device.BeginInvokeOnMainThread(() =>
            {
                ScrollViewSensorStatus.Children.Clear();
                foreach (View view in sensorStatusObject) ScrollViewSensorStatus.Children.Add(view);
            });
        }

        private void ThreadController()
        {
            var started = false;
            var firebaseStarted = false;
            var databaseController = new DatabaseController();
            Thread scheduleDetail = null;
            Thread sensorStatus = null;
            Thread queueScheduleDetail = null;

            while (true)
            {
                if (databaseController.IsRealtimeFirebaseSelected())
                {
                    if (firebaseStarted == false)
                    {
                        firebaseStarted = true;
                        new Thread(() => GetScheduleReadingFirebase(databaseController)).Start();
                        GetSensorReadingFirebase(databaseController);
                    }
                }
                else
                {
                    firebaseStarted = false;
                    _schedulesList.Clear();
                    _equipmentList.Clear();
                    _sensorList.Clear();
                    if (started == false && databaseController.GetActivityStatus() != null &&
                        databaseController.GetActivityStatus().status)
                    {
                        //Start the threads
                        scheduleDetail = new Thread(GetScheduleDetail);
                        queueScheduleDetail = new Thread(GetQueueScheduleDetail);
                        sensorStatus = new Thread(GetSensorStatus);

                        scheduleDetail.Start();
                        queueScheduleDetail.Start();
                        sensorStatus.Start();
                        started = true;
                    }

                    if (scheduleDetail != null)
                        if (started && databaseController.GetActivityStatus() != null &&
                            databaseController.GetActivityStatus().status == false)
                        {
                            scheduleDetail.Abort();
                            queueScheduleDetail.Abort();
                            sensorStatus.Abort();
                            started = false;
                            //Stop the threads
                        }
                }


                Thread.Sleep(2000);
            }

            // ReSharper disable once FunctionNeverReturns
        }

        private void GetScheduleDetail()
        {
            var running = true;
            var stopwatch = new Stopwatch();
            while (running)
            {
                stopwatch.Start();
                try
                {
                    var schedules = _socket.Message(_command.getActiveSchedule());
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (_oldActiveSchedule == schedules)
                            return;

                        ScrollViewScheduleStatus.Children.Clear();
                        _oldActiveSchedule = schedules;

                        var scheduleList = GetScheduleDetailObject(schedules);
                        foreach (View view in scheduleList) ScrollViewScheduleStatus.Children.Add(view);
                    });
                }
                catch (ThreadAbortException)
                {
                    running = false;
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        _oldActiveSensorStatus = null;
                        ScrollViewScheduleStatus.Children.Clear();
                        ScrollViewScheduleStatus.Children.Add(new ViewNoConnection());
                    });
                }

                stopwatch.Stop();
                var timeLeft = stopwatch.Elapsed.Seconds - 5;
                stopwatch.Reset();
                if (timeLeft < 0)
                    Thread.Sleep(timeLeft * -1000);
            }
        }

        private static List<object> GetScheduleDetailObject(string schedules)
        {
            var scheduleListObject = new List<object>();
            try
            {
                if (schedules == "No Data" || schedules == "")
                {
                    scheduleListObject.Add(new ViewEmptySchedule("No Running Schedules"));
                    return scheduleListObject;
                }


                var scheduleList = new List<string>();
                if (schedules.Contains("$"))
                {
                    var scheduleWithManual = schedules.Split('$').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                    if (scheduleWithManual.Count > 1)
                    {
                        scheduleListObject.Add(new ViewManualSchedule(scheduleWithManual[0].Split(',').ToList(), true));
                        scheduleList = scheduleWithManual[scheduleWithManual.Count - 1].Split('#')
                            .Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                    }
                    else
                    {
                        scheduleListObject.Add(new ViewManualSchedule(scheduleWithManual[0].Split(',').ToList(),
                            false));
                    }
                }
                else
                {
                    scheduleList = schedules.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                }

                foreach (var schedule in scheduleList)
                    scheduleListObject.Add(new ViewScheduleDetail(schedule.Split(',').ToList()));
                return scheduleListObject;
            }
            catch
            {
                scheduleListObject = new List<object>();
                scheduleListObject.Add(new ViewNoConnection());
                return scheduleListObject;
            }
        }

        private void GetQueueScheduleDetail()
        {
            var stopwatch = new Stopwatch();
            var running = true;
            while (running)
            {
                stopwatch.Start();
                try
                {
                    var queueSchedules = _socket.Message(_command.getQueueSchedule());

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (_oldQueueActiveSchedule == queueSchedules)
                            return;
                        ScrollViewQueueStatus.Children.Clear();
                        _oldQueueActiveSchedule = queueSchedules;

                        var queueScheduleList = GetQueueScheduleDetailObject(queueSchedules);
                        foreach (View view in queueScheduleList) ScrollViewQueueStatus.Children.Add(view);
                    });
                }
                catch (ThreadAbortException)
                {
                    running = false;
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        _oldQueueActiveSchedule = null;
                        ScrollViewQueueStatus.Children.Clear();
                        ScrollViewQueueStatus.Children.Add(new ViewNoConnection());
                    });
                }

                stopwatch.Stop();
                var timeLeft = stopwatch.Elapsed.Seconds - 5;
                stopwatch.Reset();
                if (timeLeft < 0)
                    Thread.Sleep(timeLeft * -1000);
            }
        }

        private static List<object> GetQueueScheduleDetailObject(string queueSchedules)
        {
            var queueScheduleListObject = new List<object>();
            try
            {
                if (queueSchedules == "No Data" || queueSchedules == "")
                {
                    queueScheduleListObject.Add(new ViewEmptySchedule("No Queued Schedules"));
                    return queueScheduleListObject;
                }


                var queueScheduleList = queueSchedules.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                queueScheduleListObject.AddRange(queueScheduleList.Select(schedule =>
                    new ViewScheduleDetail(schedule.Split(',').ToList())));

                return queueScheduleListObject;
            }
            catch
            {
                queueScheduleListObject.Add(new ViewNoConnection());
                return queueScheduleListObject;
            }
        }

        private void GetSensorStatus()
        {
            var stopwatch = new Stopwatch();
            var running = true;
            while (running)
            {
                stopwatch.Start();
                try
                {
                    var activeSensorStatus = _socket.Message(_command.getActiveSensorStatus());

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (_oldActiveSensorStatus == activeSensorStatus)
                            return;
                        ScrollViewSensorStatus.Children.Clear();
                        _oldActiveSensorStatus = activeSensorStatus;

                        var sensorListObject = GetSensorStatusObject(activeSensorStatus);
                        foreach (View view in sensorListObject) ScrollViewSensorStatus.Children.Add(view);
                    });
                }
                catch (ThreadAbortException)
                {
                    running = false;
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        _oldActiveSensorStatus = null;
                        ScrollViewSensorStatus.Children.Clear();
                        ScrollViewSensorStatus.Children.Add(new ViewNoConnection());
                    });
                }

                stopwatch.Stop();
                var timeLeft = stopwatch.Elapsed.Seconds - 2;
                stopwatch.Reset();
                if (timeLeft < 0)
                    Thread.Sleep(timeLeft * -1000);
            }
        }

        private static List<object> GetSensorStatusObject(string activeSensorStatus)
        {
            var sensorListObject = new List<object>();
            try
            {
                if (activeSensorStatus == "No Data" || activeSensorStatus == "")
                {
                    sensorListObject.Add(new ViewEmptySchedule("No Sensors Found Here"));
                    return sensorListObject;
                }

                var sensorList = activeSensorStatus.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                sensorListObject.AddRange(sensorList.Select(sensor =>
                    new ViewSensorDetail(sensor.Split(',').ToList())));

                return sensorListObject;
            }
            catch
            {
                sensorListObject.Add(new ViewNoConnection());
                return sensorListObject;
            }
        }

        private void ScrollViewScheduleStatusTap_Tapped(object sender, EventArgs e)
        {
            if (_oldActiveSchedule == null)
                return;
            var floatingScreen = new FloatingScreenScroll();
            floatingScreen.setFloatingScreen(GetScheduleDetailObject(_oldActiveSchedule));
            PopupNavigation.Instance.PushAsync(floatingScreen);
        }

        private void ScrollViewQueueStatusTap_Tapped(object sender, EventArgs e)
        {
            if (_oldQueueActiveSchedule == null)
                return;
            var floatingScreen = new FloatingScreenScroll();
            floatingScreen.setFloatingScreen(GetQueueScheduleDetailObject(_oldQueueActiveSchedule));
            PopupNavigation.Instance.PushAsync(floatingScreen);
        }

        private void ScrollViewSensorStatusTap_Tapped(object sender, EventArgs e)
        {
            if (_oldActiveSensorStatus == null)
                return;
            var floatingScreen = new FloatingScreenScroll();
            floatingScreen.setFloatingScreen(GetSensorStatusObject(_oldActiveSensorStatus));
            PopupNavigation.Instance.PushAsync(floatingScreen);
        }
    }
}