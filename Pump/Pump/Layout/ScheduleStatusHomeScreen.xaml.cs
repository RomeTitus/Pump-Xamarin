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
using Pump.Database;
using Pump.Droid.Database.Table;
using Rg.Plugins.Popup.Services;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScheduleStatusHomeScreen : ContentPage
    {
        private readonly ObservableCollection<Equipment> _equipmentList;
        private readonly ObservableCollection<Sensor> _sensorList;
        private readonly ObservableCollection<ManualSchedule> _manualScheduleList;
        private readonly ObservableCollection<Schedule> _scheduleList;
        private readonly ObservableCollection<Site> _siteList;
        private readonly PumpConnection PumpConnection;


        private const double PopUpSize = 1.5;

        public ScheduleStatusHomeScreen(ObservableCollection<Equipment> equipmentList, ObservableCollection<ManualSchedule> manualScheduleList, 
            ObservableCollection<Schedule> scheduleList, ObservableCollection<Sensor> sensorList, ObservableCollection<Site> siteList)
        {
            _equipmentList = equipmentList;
            _manualScheduleList = manualScheduleList;
            _scheduleList = scheduleList;
            _sensorList = sensorList;
            _siteList = siteList;
            InitializeComponent(); 
            PumpConnection = new DatabaseController().GetControllerConnectionSelection();
            Task.Run(PopulateScheduleStatusElements);
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
                    if (!_equipmentList.Contains(null) && !_scheduleList.Contains(null) && !_manualScheduleList.Contains(null) && !_scheduleList.Contains(null))
                    {
                        hasSubscribed = true;
                    }

                    if (!_equipmentList.Contains(null) && !_scheduleList.Contains(null) && !_manualScheduleList.Contains(null) &&
                        scheduleHasRun == false)
                    {
                        scheduleHasRun = true;
                        _equipmentList.CollectionChanged += ActiveAndQueScheduleEvent;
                        _scheduleList.CollectionChanged += ActiveAndQueScheduleEvent;
                        _manualScheduleList.CollectionChanged += ActiveAndQueScheduleEvent;
                        Device.InvokeOnMainThreadAsync(ActiveAndQueSchedule);
                    }
                    if (!_sensorList.Contains(null) && sensorHasRun == false)
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
            if (_manualScheduleList.Contains(null) || _equipmentList.Contains(null) || _scheduleList.Contains(null)) return;
            var manualSchedule = _manualScheduleList.FirstOrDefault(x => x.ManualDetails.Any(z => _siteList.First(r => r.ID == PumpConnection.SiteSelectedId).Attachments.Contains(z.id_Equipment)));

            if (manualSchedule != null)
            {
                var existingManualSchedule = ScrollViewScheduleStatus.Children.FirstOrDefault(x => x.AutomationId == manualSchedule.ID);
                if (existingManualSchedule == null)

                {
                    ScrollViewScheduleStatus.Children.Add(new ViewManualSchedule(manualSchedule));
                }
            }

            var runningScheduleList = new RunningSchedule(_scheduleList.Where(x => _siteList.First(y => y.ID == PumpConnection.SiteSelectedId).Attachments.Contains(x.id_Pump)), _equipmentList).GetRunningSchedule().ToList();
                
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
                
                
            var queScheduleList = new RunningSchedule(_scheduleList.Where(x => _siteList.First(y => y.ID == PumpConnection.SiteSelectedId).Attachments.Contains(x.id_Pump)), _equipmentList).GetQueSchedule().ToList();
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
                if (_sensorList.Contains(null)) return;
                if (_sensorList.Any())
                    foreach (var sensor in _sensorList.Where(x => _siteList.First(y => y.ID == PumpConnection.SiteSelectedId).Attachments.Contains(x.ID)))
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
            try
            {
                if (!_scheduleList.Contains(null) && !_equipmentList.Contains(null))
                {
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
                else
                {
                    ScrollViewScheduleStatus.Children.Clear();
                    ScrollViewQueueStatus.Children.Clear();
                    var loadingIcon = new ActivityIndicator
                    {
                        AutomationId = "ActivityIndicatorSiteLoading",
                        HorizontalOptions = LayoutOptions.Center,
                        IsEnabled = true,
                        IsRunning = true,
                        IsVisible = true,
                        VerticalOptions = LayoutOptions.Center
                    };
                    ScrollViewScheduleStatus.Children.Add(loadingIcon);
                    ScrollViewQueueStatus.Children.Add(loadingIcon);
                }

            }
            catch
            {
                // ignored
            }
        }

        private void ScreenCleanupForSensor()
        {
            try
            {
                if (!_sensorList.Contains(null))
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
                else
                {
                    ScrollViewSensorStatus.Children.Clear();
                    var loadingIcon = new ActivityIndicator
                    {
                        AutomationId = "ActivityIndicatorSiteLoading",
                        HorizontalOptions = LayoutOptions.Center,
                        IsEnabled = true,
                        IsRunning = true,
                        IsVisible = true,
                        VerticalOptions = LayoutOptions.Center
                    };
                    ScrollViewSensorStatus.Children.Add(loadingIcon);
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