using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Firebase.Database.Streaming;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Services;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScheduleStatusHomeScreen : ContentPage
    {
        private ObservableCollection<Equipment> _equipmentList;
        private ObservableCollection<ManualSchedule> _manualScheduleList = new ObservableCollection<ManualSchedule>();
        private ObservableCollection<Schedule> _scheduleList;
        private ObservableCollection<Sensor> _sensorList;
        private const double PopUpSize = 1.5;

        public ScheduleStatusHomeScreen()
        {
            InitializeComponent();
            Task.Run(SubscribeToFirebase);
            //SubscribeToFirebase();
        }

        private void SubscribeToFirebase()
        {
            var auth = new Authentication();

            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Equipment")
                .AsObservable<Equipment>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_equipmentList == null)
                            _equipmentList = new ObservableCollection<Equipment>();
                        
                        var equipment = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _equipmentList.Count; i++)
                            {
                                if(_equipmentList[i].ID == x.Key)
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
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
            
            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/ManualSchedule")
                .AsObservable<ManualSchedule>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_manualScheduleList == null)
                            _manualScheduleList = new ObservableCollection<ManualSchedule>();
                        Device.BeginInvokeOnMainThread(() =>
                        {

                            if (x.EventType == FirebaseEventType.Delete)
                            {
                                for (int i = 0; i < _manualScheduleList.Count; i++)
                                {
                                    if (_manualScheduleList[i].ID == x.Key)
                                        _manualScheduleList.RemoveAt(i);
                                }
                            }
                            if (x.Object != null)
                            {
                                var manualSchedule = x.Object;
                                manualSchedule.ID = x.Key;
                                _manualScheduleList.Add(manualSchedule);
                            }
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });

            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Schedule")
                .AsObservable<Schedule>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_scheduleList == null)
                            _scheduleList = new ObservableCollection<Schedule>();
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
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });

            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Sensor")
                .AsObservable<Sensor>()
                .Subscribe(x =>
                {

                    try
                    {
                        if (_sensorList == null)
                            _sensorList = new ObservableCollection<Sensor>();
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
                                Device.BeginInvokeOnMainThread(SensorStatus);
                            }
                            else
                            {
                                sensor.ID = x.Key;
                                _sensorList.Add(sensor);
                            }
                            
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                });


            PopulateScheduleStatusElements();
        }


        
        private void PopulateScheduleStatusElements()
        {
            bool hasSubscribed = false;
            bool scheduleHasRun = false;
            bool sensorHasRun = false;
            while (!hasSubscribed)
            {
                try
                {
                    if (_equipmentList != null && _scheduleList != null && _manualScheduleList != null && _scheduleList != null)
                    {
                        hasSubscribed = true;
                    }

                    if (_equipmentList != null && _scheduleList != null && _manualScheduleList != null &&
                        scheduleHasRun == false)
                    {
                        scheduleHasRun = true;
                        _equipmentList =
                            new ObservableCollection<Equipment>(_equipmentList
                                .OrderBy(equip => Convert.ToInt16(equip.GPIO)).ToList());
                        _equipmentList.CollectionChanged += ActiveAndQueScheduleEvent;
                        _scheduleList.CollectionChanged += ActiveAndQueScheduleEvent;
                        _manualScheduleList.CollectionChanged += ActiveAndQueScheduleEvent;
                        Device.InvokeOnMainThreadAsync(ActiveAndQueSchedule);
                    }
                    if (_sensorList != null && sensorHasRun == false)
                    {
                        sensorHasRun = true;
                        _sensorList.CollectionChanged += SensorStatusEvent;
                        Device.InvokeOnMainThreadAsync(SensorStatus);
                    }
                }
                catch
                {
                    // ignored
                }
                Thread.Sleep(100);
            }
        }

        private void ActiveAndQueScheduleEvent(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(ActiveAndQueSchedule);
        }
        private void ActiveAndQueSchedule()
        {
            ScreenCleanupForSchedule();

            //get All Manual Stuff
            if (_manualScheduleList.Count > 0)
                    foreach (var manual in from manual in _manualScheduleList
                        let existingManualSchedule =
                            ScrollViewScheduleStatus.Children.FirstOrDefault(x =>
                                x.AutomationId == manual.ID)
                        where existingManualSchedule == null
                        select manual)
                    {
                        ScrollViewScheduleStatus.Children.Add(new ViewManualSchedule(manual));
                    }
                
            var runningScheduleList = new RunningSchedule(_scheduleList, _equipmentList).GetRunningSchedule().ToList();
                
            if (runningScheduleList.Any())
                foreach (var runningSchedule in runningScheduleList)
                {
                    var viewActiveSchedule = ScrollViewScheduleStatus.Children.FirstOrDefault(x =>
                        x.AutomationId == runningSchedule.ID);
                    if (viewActiveSchedule != null)
                    {
                        var viewActiveScheduleSummary = (ViewActiveScheduleSummary)viewActiveSchedule;
                        viewActiveScheduleSummary.ActiveSchedule = runningSchedule;
                        viewActiveScheduleSummary.PopulateSchedule();
                    }
                    else
                    {
                        ScrollViewScheduleStatus.Children.Add(
                            new ViewActiveScheduleSummary(runningSchedule));
                    }
                }
            else
            {
                ScrollViewScheduleStatus.Children.Add(new ViewEmptySchedule("No Active Schedules"));
            }
                
                
            var queScheduleList = new RunningSchedule(_scheduleList, _equipmentList).GetQueSchedule().ToList();
            if (queScheduleList.Any())
                foreach (var runningSchedule in queScheduleList)
                {
                    var viewActiveSchedule = ScrollViewQueueStatus.Children.FirstOrDefault(x =>
                        x.AutomationId == runningSchedule.ID);
                    if (viewActiveSchedule != null)
                    {
                        var viewActiveScheduleSummary = (ViewActiveScheduleSummary)viewActiveSchedule;
                        viewActiveScheduleSummary.ActiveSchedule = runningSchedule;
                        viewActiveScheduleSummary.PopulateSchedule();
                    }
                    else
                    {
                        ScrollViewQueueStatus.Children.Add(
                            new ViewActiveScheduleSummary(runningSchedule));
                    }
                }
            else
            {
                ScrollViewQueueStatus.Children.Add(new ViewEmptySchedule("No Que Schedules"));
            }
                
                
        }

        private void SensorStatusEvent(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(SensorStatus);
        }
        private void SensorStatus()
        {
            ScreenCleanupForSensor();
            try
            {
                if (_sensorList.Any())
                    foreach (var sensor in _sensorList)
                    {
                        var viewSensor = ScrollViewSensorStatus.Children.FirstOrDefault(x =>
                            x.AutomationId == sensor.ID);
                        if (viewSensor != null)
                        {
                            var viewSensorStatus = (ViewSensorDetail)viewSensor;
                            viewSensorStatus.Sensor.NAME = sensor.NAME;
                            viewSensorStatus.Sensor.LastReading = sensor.LastReading;
                            viewSensorStatus.Sensor.TYPE = sensor.TYPE;
                            viewSensorStatus.PopulateSensor();
                        }
                        else
                        {
                            ScrollViewSensorStatus.Children.Add(
                                new ViewSensorDetail(sensor));
                        }
                    }
                else
                {
                    ScrollViewSensorStatus.Children.Add(new ViewEmptySchedule("No Sensors Here"));
                }
            }
            catch
            {
                // ignored
            }

            
        }

        private void ScreenCleanupForSchedule()
        {
            //CleanUp :)
                try
                {
                    if (_equipmentList == null || _scheduleList == null || _manualScheduleList == null) return;
                    var runningScheduleList = new RunningSchedule(_scheduleList, _equipmentList)
                        .GetRunningSchedule().ToList();
                    var queScheduleList = new RunningSchedule(_scheduleList, _equipmentList)
                        .GetQueSchedule().ToList();

                    var itemsThatAreOnDisplay = runningScheduleList.Select(x => x.ID).ToList();
                    itemsThatAreOnDisplay.AddRange(_manualScheduleList.Select(x => x.ID));
                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).ID);


                    for (var index = 0; index < ScrollViewScheduleStatus.Children.Count; index++)
                    {
                        var existingItems = itemsThatAreOnDisplay.FirstOrDefault(x =>
                            x == ScrollViewScheduleStatus.Children[index].AutomationId);
                        if (existingItems != null) continue;
                        ScrollViewScheduleStatus.Children.RemoveAt(index);
                        index--;
                    }


                    itemsThatAreOnDisplay = queScheduleList.Select(x => x.ID).ToList();
                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).ID);


                    for (var index = 0; index < ScrollViewQueueStatus.Children.Count; index++)
                    {
                        var existingItems = itemsThatAreOnDisplay.FirstOrDefault(x =>
                            x == ScrollViewQueueStatus.Children[index].AutomationId);
                        if (existingItems != null) continue;
                        ScrollViewQueueStatus.Children.RemoveAt(index);
                        index--;
                    }

                }
                catch
                {
                    // ignored
                }

            
        }

        private void ScreenCleanupForSensor()
        {

            //CleanUp :)
            try
            {
                if (_sensorList != null)
                {

                    var itemsThatAreOnDisplay = _sensorList.Select(x => x.ID).ToList();
                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).ID);


                    for (var index = 0; index < ScrollViewSensorStatus.Children.Count; index++)
                    {
                        var existingItems = itemsThatAreOnDisplay.FirstOrDefault(x =>
                            x == ScrollViewSensorStatus.Children[index].AutomationId);
                        if (existingItems != null) continue;
                        ScrollViewSensorStatus.Children.RemoveAt(index);
                        index--;
                    }
                }

            }
            catch
            {
                // ignored
            }


        }
        
        

        private void ScrollViewScheduleStatusTap_OnTapped(object sender, EventArgs e)
        {
            if (_scheduleList == null)
                return;
            var runningStatusViews = new RunningSchedule(_scheduleList, _equipmentList).GetRunningSchedule().ToList()
                .Select(queue => new ViewActiveScheduleSummary(queue, PopUpSize)).Cast<View>().ToList();
            if(_manualScheduleList.Count>0)
                foreach (var manual in _manualScheduleList)
                    runningStatusViews.Insert(0, new ViewManualSchedule(manual));
            
            if (runningStatusViews.Count == 0)
                runningStatusViews.Add(new ViewEmptySchedule("No Running Schedules", PopUpSize));

            var floatingScreen = new FloatingScreenScroll();
            floatingScreen.SetFloatingScreen(runningStatusViews);
            PopupNavigation.Instance.PushAsync(floatingScreen);
        }

        private void ScrollViewQueueStatusTap_OnTapped(object sender, EventArgs e)
        {
            if (_scheduleList == null)
                return;
            var queueStatusViews = new RunningSchedule(_scheduleList, _equipmentList).GetQueSchedule().ToList()
                .Select(queue => new ViewActiveScheduleSummary(queue, PopUpSize)).Cast<View>().ToList();
            if (queueStatusViews.Count == 0)
                queueStatusViews.Add(new ViewEmptySchedule("No Que Schedules", PopUpSize));
           
            var floatingScreen = new FloatingScreenScroll();
            floatingScreen.SetFloatingScreen(queueStatusViews);
            PopupNavigation.Instance.PushAsync(floatingScreen);
        }
        private void ScrollViewSensorStatusTap_OnTapped(object sender, EventArgs e)
        {
            if (_sensorList == null)
                return;
            var sensorStatusViews = _sensorList.Select(sensor => new ViewSensorDetail(sensor, PopUpSize)).Cast<View>().ToList();
            if (sensorStatusViews.Count == 0)
                sensorStatusViews.Add(new ViewEmptySchedule("No Sensors Here", PopUpSize));

            var floatingScreen = new FloatingScreenScroll();
            floatingScreen.SetFloatingScreen(sensorStatusViews);
            PopupNavigation.Instance.PushAsync(floatingScreen);
        }

        

        
    }
}