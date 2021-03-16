using System;
using System.Linq;
using System.Threading;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Threading.Tasks;
using Pump.Database;
using Pump.Droid.Database.Table;
using Rg.Plugins.Popup.Services;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScheduleStatusHomeScreen : ContentPage
    {
        private readonly PumpConnection _pumpConnection;
        private readonly ObservableIrrigation _observableIrrigation;
        private const double PopUpSize = 1.5;

        public ScheduleStatusHomeScreen(ObservableIrrigation observableIrrigation)
        {
            _observableIrrigation = observableIrrigation;
            InitializeComponent(); 
            _pumpConnection = new DatabaseController().GetControllerConnectionSelection();
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
                    if (!_observableIrrigation.EquipmentList.Contains(null) && !_observableIrrigation.ScheduleList.Contains(null) && !_observableIrrigation.ManualScheduleList.Contains(null) && !_observableIrrigation.ScheduleList.Contains(null) && !_observableIrrigation.CustomScheduleList.Contains(null))
                    {
                        hasSubscribed = true;
                    }

                    if (!_observableIrrigation.EquipmentList.Contains(null) && !_observableIrrigation.ScheduleList.Contains(null) && !_observableIrrigation.ManualScheduleList.Contains(null)
                        && !_observableIrrigation.CustomScheduleList.Contains(null) && scheduleHasRun == false)
                    {
                        scheduleHasRun = true;
                        _observableIrrigation.EquipmentList.CollectionChanged += ActiveAndQueScheduleEvent;
                        _observableIrrigation.ScheduleList.CollectionChanged += ActiveAndQueScheduleEvent;
                        _observableIrrigation.ManualScheduleList.CollectionChanged += ActiveAndQueScheduleEvent;
                        _observableIrrigation.CustomScheduleList.CollectionChanged += ActiveAndQueScheduleEvent;
                        Device.InvokeOnMainThreadAsync(ActiveAndQueSchedule);
                    }
                    if (!_observableIrrigation.SensorList.Contains(null) && sensorHasRun == false)
                    {
                        sensorHasRun = true;
                        _observableIrrigation.SensorList.CollectionChanged += SensorStatusEvent;
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
            try
            { 
                if (_observableIrrigation.ManualScheduleList.Contains(null) || _observableIrrigation.EquipmentList.Contains(null) || _observableIrrigation.ScheduleList.Contains(null)) return;
                ScreenCleanupForSchedule();
                var manualSchedule = _observableIrrigation.ManualScheduleList.FirstOrDefault(x => x.ManualDetails.Any(z => _observableIrrigation.SiteList.First(r => r.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(z.id_Equipment)));

                if (manualSchedule != null)
                {
                    var existingManualSchedule = ScrollViewScheduleStatus.Children.FirstOrDefault(x => x.AutomationId == manualSchedule.ID);
                    if (existingManualSchedule == null)

                    {
                        ScrollViewScheduleStatus.Children.Insert(0,new ViewManualSchedule(manualSchedule));
                    }
                }

                var runningScheduleList = new RunningSchedule(_observableIrrigation.ScheduleList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.id_Pump)), _observableIrrigation.EquipmentList).GetRunningSchedule().ToList();
                var activeCustomScheduleList = new RunningCustomSchedule().GetActiveCustomSchedule(_observableIrrigation.CustomScheduleList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.id_Pump)).ToList(), _observableIrrigation.EquipmentList.ToList());
                runningScheduleList.AddRange(new RunningCustomSchedule().GetRunningCustomSchedule(activeCustomScheduleList));

                if (runningScheduleList.Any())
                    foreach (var runningSchedule in runningScheduleList)
                    {
                        var viewActiveSchedule = ScrollViewScheduleStatus.Children.FirstOrDefault(x =>
                            x.AutomationId == runningSchedule.Id);
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
                
                
                var queScheduleList = new RunningSchedule(_observableIrrigation.ScheduleList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.id_Pump)), _observableIrrigation.EquipmentList).GetQueSchedule().ToList();
                var queCustomScheduleList = new RunningCustomSchedule().GetActiveCustomSchedule(_observableIrrigation.CustomScheduleList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.id_Pump)).ToList(), _observableIrrigation.EquipmentList.ToList());
                queScheduleList.AddRange(new RunningCustomSchedule().GetQueCustomSchedule(queCustomScheduleList));

                if (queScheduleList.Any())
                    foreach (var runningSchedule in queScheduleList)
                    {
                        var viewActiveSchedule = ScrollViewQueueStatus.Children.FirstOrDefault(x =>
                            x.AutomationId == runningSchedule.Id);
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
                    ScrollViewQueueStatus.Children.Add(new ViewEmptySchedule("No Queued Schedules"));
                }

            }
            catch
            {

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
                if (_observableIrrigation.SensorList.Contains(null)) return;
                if (_observableIrrigation.SensorList.Any(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)))
                    foreach (var sensor in _observableIrrigation.SensorList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)))
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
                if (!_observableIrrigation.ScheduleList.Contains(null) && !_observableIrrigation.EquipmentList.Contains(null))
                {
                    var runningScheduleList = new RunningSchedule(_observableIrrigation.ScheduleList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.id_Pump)), _observableIrrigation.EquipmentList).GetRunningSchedule().ToList();
                    var activeCustomScheduleList = new RunningCustomSchedule().GetActiveCustomSchedule(_observableIrrigation.CustomScheduleList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.id_Pump)).ToList(), _observableIrrigation.EquipmentList.ToList());
                    runningScheduleList.AddRange(new RunningCustomSchedule().GetRunningCustomSchedule(activeCustomScheduleList));


                    var queScheduleList = new RunningSchedule(_observableIrrigation.ScheduleList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.id_Pump)), _observableIrrigation.EquipmentList).GetQueSchedule().ToList();
                    queScheduleList.AddRange(new RunningCustomSchedule().GetQueCustomSchedule(activeCustomScheduleList));

                    var itemsThatAreOnDisplay = runningScheduleList.Select(x => x.Id).ToList();
                    itemsThatAreOnDisplay.AddRange(_observableIrrigation.ManualScheduleList.Select(x => x.ID));
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

                    itemsThatAreOnDisplay = queScheduleList.Select(x => x.Id).ToList();
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
                if (!_observableIrrigation.SensorList.Contains(null))
                {
                    var itemsThatAreOnDisplay = _observableIrrigation.SensorList.Select(x => x.ID).ToList();
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
            if (_observableIrrigation.ScheduleList.Contains(null))
                return;
            var runningStatusViews = new RunningSchedule(_observableIrrigation.ScheduleList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.id_Pump)), _observableIrrigation.EquipmentList).GetRunningSchedule().ToList()
                .Select(queue => new ViewActiveScheduleSummary(queue, PopUpSize)).Cast<View>().ToList();
            var manualSchedule = _observableIrrigation.ManualScheduleList.FirstOrDefault(x => x.ManualDetails.Any(z => _observableIrrigation.SiteList.First(r => r.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(z.id_Equipment)));
            
            if (manualSchedule != null)
                runningStatusViews.Insert(0, new ViewManualSchedule(manualSchedule));
            
            if (runningStatusViews.Count == 0)
                runningStatusViews.Add(new ViewEmptySchedule("No Running Schedules", PopUpSize));

            var floatingScreen = new FloatingScreenScroll();
            floatingScreen.SetFloatingScreen(runningStatusViews);
            PopupNavigation.Instance.PushAsync(floatingScreen);
        }

        private void ScrollViewQueueStatusTap_OnTapped(object sender, EventArgs e)
        {
            if (_observableIrrigation.ScheduleList.Contains(null))
                return;
            var queueStatusViews = new RunningSchedule(_observableIrrigation.ScheduleList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.id_Pump)), _observableIrrigation.EquipmentList).GetQueSchedule().ToList()
                .Select(queue => new ViewActiveScheduleSummary(queue, PopUpSize)).Cast<View>().ToList(); 
            if (queueStatusViews.Count == 0)
                queueStatusViews.Add(new ViewEmptySchedule("No Que Schedules", PopUpSize));
           
            var floatingScreen = new FloatingScreenScroll();
            floatingScreen.SetFloatingScreen(queueStatusViews);
            PopupNavigation.Instance.PushAsync(floatingScreen);
        }
        private void ScrollViewSensorStatusTap_OnTapped(object sender, EventArgs e)
        {
            if (_observableIrrigation.SensorList.Contains(null))
                return;
            var sensorStatusViews = _observableIrrigation.SensorList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)).ToList().Select(sensor => new ViewSensorDetail(sensor, PopUpSize)).Cast<View>().ToList();

            if (sensorStatusViews.Count == 0)
                sensorStatusViews.Add(new ViewEmptySchedule("No Sensors Here", PopUpSize));

            var floatingScreen = new FloatingScreenScroll();
            floatingScreen.SetFloatingScreen(sensorStatusViews);
            PopupNavigation.Instance.PushAsync(floatingScreen);
        }

        

        
    }
}