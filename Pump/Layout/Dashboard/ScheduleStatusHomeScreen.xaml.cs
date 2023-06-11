using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Dashboard
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScheduleStatusHomeScreen
    {
        private const double PopUpSize = 1.5;

        private readonly KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation>
            _observableFilterKeyValuePair;

        public ScheduleStatusHomeScreen(
            KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation> observableFilterKeyValuePair)
        {
            InitializeComponent();
            _observableFilterKeyValuePair = observableFilterKeyValuePair;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _observableFilterKeyValuePair.Value.EquipmentList.CollectionChanged += ActiveAndQueScheduleEvent;
            _observableFilterKeyValuePair.Value.ScheduleList.CollectionChanged += ActiveAndQueScheduleEvent;
            _observableFilterKeyValuePair.Value.ManualScheduleList.CollectionChanged += ActiveAndQueScheduleEvent;
            _observableFilterKeyValuePair.Value.CustomScheduleList.CollectionChanged += ActiveAndQueScheduleEvent;
            ActiveAndQueSchedule();
            _observableFilterKeyValuePair.Value.SensorList.CollectionChanged += SensorStatusEvent;
            SensorStatus();
        }

        private void ActiveAndQueScheduleEvent(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(ActiveAndQueSchedule);
        }

        private void ActiveAndQueSchedule()
        {
            try
            {
                if (!_observableFilterKeyValuePair.Value.LoadedData) return;

                var runningScheduleList = GetActiveSchedules();
                var queScheduleList = GetQueSchedule();

                ScreenCleanupForSchedule(runningScheduleList, queScheduleList);
                
                PopulateManualScheduleView();
                
                PopulateScheduleSummaryView(ScrollViewScheduleStatus, runningScheduleList, "No Active Schedules");
                
                PopulateScheduleSummaryView(ScrollViewQueueStatus, queScheduleList, "No Queued Schedules");
            }
            catch (Exception e)
            {
                ScrollViewQueueStatus.Children.Add(new ViewException(e));
                ScrollViewScheduleStatus.Children.Add(new ViewException(e));
            }
        }

        private void PopulateScheduleSummaryView(StackLayout stackLayout, List<ActiveSchedule> schedules, string emptySchedule)
        {
            foreach (var runningSchedule in schedules)
            {
                var viewActiveSchedule = stackLayout.Children.FirstOrDefault(x =>
                    x.AutomationId == runningSchedule.Id);
                if (viewActiveSchedule != null)
                {
                    var viewActiveScheduleSummary = (ViewActiveScheduleSummary)viewActiveSchedule;
                    viewActiveScheduleSummary.ActiveSchedule = runningSchedule;
                    viewActiveScheduleSummary.PopulateSchedule();
                }
                else
                {
                    stackLayout.Children.Add(
                        new ViewActiveScheduleSummary(runningSchedule));
                }
            }
            
            if (schedules.Any() == false && stackLayout.Children.Count == 0)
                stackLayout.Children.Add(new ViewEmptySchedule(emptySchedule));
            
        }

        private void PopulateManualScheduleView()
        {
            var manualSchedule = _observableFilterKeyValuePair.Value.ManualScheduleList.FirstOrDefault();
            if (manualSchedule != null)
            {
                var existingManualSchedule =
                    ScrollViewScheduleStatus.Children.FirstOrDefault(x => x.AutomationId == manualSchedule.Id);
                if (existingManualSchedule == null)
                    ScrollViewScheduleStatus.Children.Insert(0, new ViewManualSchedule(manualSchedule));
            }

        }

        private void SensorStatusEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            SensorStatus();
        }

        private void SensorStatus()
        {
            ScreenCleanupForSensor();
            try
            {
                if (_observableFilterKeyValuePair.Value.LoadedData == false) 
                    return;
                
                
                foreach (var sensor in _observableFilterKeyValuePair.Value.SensorList)
                {
                    var viewSensor = ScrollViewSensorStatus.Children.FirstOrDefault(x =>
                        x.AutomationId == sensor.Id);
                    if (viewSensor != null)
                    {
                        var viewSensorStatus = (ViewSensorDetail)viewSensor;
                        viewSensorStatus.Sensor.NAME = sensor.NAME;
                        viewSensorStatus.Sensor.LastReading = sensor.LastReading;
                        viewSensorStatus.Sensor.TYPE = sensor.TYPE;
                        viewSensorStatus.Populate();
                    }
                    else
                    {
                        ScrollViewSensorStatus.Children.Add(
                            new ViewSensorDetail(sensor));
                    }
                }
                
                if (_observableFilterKeyValuePair.Value.SensorList.Any() == false && ScrollViewSensorStatus.Children.Any() == false)
                    ScrollViewSensorStatus.Children.Add(new ViewEmptySchedule("No Sensors Here"));
                
            }
            catch (Exception e)
            {
                ScrollViewSensorStatus.Children.Add(new ViewException(e));
            }
        }

        private void ScreenCleanupForSchedule(List<ActiveSchedule> activeSchedules, List<ActiveSchedule> queScheduleList)
        {
            if (_observableFilterKeyValuePair.Value.LoadedData)
            {
                //Active Schedule
                var activeScheduleOnDisplay = activeSchedules.Select(x => x.Id).ToList();
                    
                activeScheduleOnDisplay.AddRange(
                    _observableFilterKeyValuePair.Value.ManualScheduleList.Select(x => x?.Id));

                if (activeScheduleOnDisplay.Count == 0)
                    activeScheduleOnDisplay.Add(new ViewEmptySchedule(string.Empty).AutomationId);
                    
                ScrollViewScheduleStatus.RemoveUnusedViews(activeScheduleOnDisplay);

                //Que Schedule
                var queScheduleOnDisplay = queScheduleList.Select(x => x?.Id).ToList();
                if (queScheduleOnDisplay.Count == 0)
                    queScheduleOnDisplay.Add(new ViewEmptySchedule(string.Empty).AutomationId);

                ScrollViewQueueStatus.RemoveUnusedViews(queScheduleOnDisplay);
            }
            else
            {
                ScrollViewScheduleStatus.DisplayActivityLoading();

                ScrollViewQueueStatus.DisplayActivityLoading();
            }
        }

        private void ScreenCleanupForSensor()
        {
            if (_observableFilterKeyValuePair.Value.LoadedData)
            {
                var sensorsDisplay =
                    _observableFilterKeyValuePair.Value.SensorList.Select(x => x?.Id).ToList();
                if (sensorsDisplay.Count == 0)
                    sensorsDisplay.Add(new ViewEmptySchedule(string.Empty).AutomationId);

                ScrollViewSensorStatus.RemoveUnusedViews(sensorsDisplay);
            }
            else
            {
                ScrollViewSensorStatus.DisplayActivityLoading();
            }
        }
        private List<ActiveSchedule> GetActiveSchedules()
        {
            var runningScheduleList = _observableFilterKeyValuePair.Value.ScheduleList
                    .GetRunningSchedule(_observableFilterKeyValuePair.Value.EquipmentList).ToList();
            
            runningScheduleList.AddRange(_observableFilterKeyValuePair.Value.CustomScheduleList
                .GetRunningSchedule(_observableFilterKeyValuePair.Value.EquipmentList).ToList());

            return runningScheduleList;
        }

        private List<ActiveSchedule> GetQueSchedule()
        {
            var queScheduleList = _observableFilterKeyValuePair.Value.ScheduleList
                    .GetQueSchedule(_observableFilterKeyValuePair.Value.EquipmentList).ToList();
            
            queScheduleList.AddRange(_observableFilterKeyValuePair.Value.CustomScheduleList
                .GetQueSchedule(_observableFilterKeyValuePair.Value.EquipmentList).ToList());
            
            return queScheduleList;
        }
        
        



        private void ScrollViewScheduleStatusTap_OnTapped(object sender, EventArgs e)
        {
            if (_observableFilterKeyValuePair.Value.ScheduleList.Contains(null) &&
                _observableFilterKeyValuePair.Value.CustomScheduleList.Contains(null))
                return;
            var runningStatusViews = _observableFilterKeyValuePair.Value.ScheduleList
                    .GetRunningSchedule(_observableFilterKeyValuePair.Value.EquipmentList).ToList()
                    
                    .Select(queue => new ViewActiveScheduleSummary(queue, PopUpSize)).Cast<View>().ToList();

            var activeCustomScheduleList = _observableFilterKeyValuePair.Value.CustomScheduleList
                .GetRunningSchedule(_observableFilterKeyValuePair.Value.EquipmentList)
                .Select(queue => new ViewActiveScheduleSummary(queue, PopUpSize)).Cast<View>().ToList();
            runningStatusViews.AddRange(activeCustomScheduleList);

            var manualSchedule =
                _observableFilterKeyValuePair.Value.ManualScheduleList.FirstOrDefault(x => x.ManualDetails.Any());

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
            if (_observableFilterKeyValuePair.Value.ScheduleList.Contains(null))
                return;
            var queueStatusViews = _observableFilterKeyValuePair.Value.ScheduleList
                .GetQueSchedule(_observableFilterKeyValuePair.Value.EquipmentList).ToList()
                .Select(queue => new ViewActiveScheduleSummary(queue, PopUpSize)).Cast<View>().ToList();


            var queCustomScheduleList = _observableFilterKeyValuePair.Value.CustomScheduleList
                .GetQueSchedule(_observableFilterKeyValuePair.Value.EquipmentList)
                .Select(queue => new ViewActiveScheduleSummary(queue, PopUpSize)).Cast<View>().ToList();

            queueStatusViews.AddRange(queCustomScheduleList);

            if (queueStatusViews.Count == 0)
                queueStatusViews.Add(new ViewEmptySchedule("No Que Schedules", PopUpSize));

            var floatingScreen = new FloatingScreenScroll();
            floatingScreen.SetFloatingScreen(queueStatusViews);
            PopupNavigation.Instance.PushAsync(floatingScreen);
        }

        private void ScrollViewSensorStatusTap_OnTapped(object sender, EventArgs e)
        {
            if (_observableFilterKeyValuePair.Value.SensorList.Contains(null))
                return;
            var sensorStatusViews = _observableFilterKeyValuePair.Value.SensorList.ToList()
                .Select(sensor => new ViewSensorDetail(sensor, PopUpSize)).Cast<View>().ToList();

            if (sensorStatusViews.Count == 0)
                sensorStatusViews.Add(new ViewEmptySchedule("No Sensors Here", PopUpSize));

            var floatingScreen = new FloatingScreenScroll();
            floatingScreen.SetFloatingScreen(sensorStatusViews);
            PopupNavigation.Instance.PushAsync(floatingScreen);
        }
    }
}