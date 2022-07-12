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
    public partial class ScheduleStatusHomeScreen : ContentView
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
                ScreenCleanupForSchedule();
                if (!_observableFilterKeyValuePair.Value.LoadedData) return;

                var manualSchedule = _observableFilterKeyValuePair.Value.ManualScheduleList.FirstOrDefault();

                if (manualSchedule != null)
                {
                    var existingManualSchedule =
                        ScrollViewScheduleStatus.Children.FirstOrDefault(x => x.AutomationId == manualSchedule.Id);
                    if (existingManualSchedule == null)
                        ScrollViewScheduleStatus.Children.Insert(0, new ViewManualSchedule(manualSchedule));
                }

                var runningScheduleList =
                    new RunningSchedule(_observableFilterKeyValuePair.Value.ScheduleList,
                            _observableFilterKeyValuePair.Value.EquipmentList)
                        .GetRunningSchedule().ToList();
                var activeCustomScheduleList = new RunningCustomSchedule().GetActiveCustomSchedule(
                    _observableFilterKeyValuePair.Value.CustomScheduleList.ToList(),
                    _observableFilterKeyValuePair.Value.EquipmentList.ToList());
                runningScheduleList.AddRange(
                    new RunningCustomSchedule().GetRunningCustomSchedule(activeCustomScheduleList));

                if (runningScheduleList.Any())
                {
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
                }
                else
                {
                    if (ScrollViewScheduleStatus.Children.Count == 0)
                        ScrollViewScheduleStatus.Children.Add(new ViewEmptySchedule("No Active Schedules"));
                }


                var queScheduleList =
                    new RunningSchedule(_observableFilterKeyValuePair.Value.ScheduleList,
                            _observableFilterKeyValuePair.Value.EquipmentList)
                        .GetQueSchedule().ToList();
                var queCustomScheduleList = new RunningCustomSchedule().GetActiveCustomSchedule(
                    _observableFilterKeyValuePair.Value.CustomScheduleList.ToList(),
                    _observableFilterKeyValuePair.Value.EquipmentList.ToList());
                queScheduleList.AddRange(new RunningCustomSchedule().GetQueCustomSchedule(queCustomScheduleList));

                if (queScheduleList.Any())
                {
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
                }
                else
                {
                    if (ScrollViewQueueStatus.Children.Count == 0)
                        ScrollViewQueueStatus.Children.Add(new ViewEmptySchedule("No Queued Schedules"));
                }
            }
            catch (Exception e)
            {
                ScrollViewQueueStatus.Children.Add(new ViewException(e));
                ScrollViewScheduleStatus.Children.Add(new ViewException(e));
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
                if (_observableFilterKeyValuePair.Value.SensorList.Contains(null)) return;
                if (_observableFilterKeyValuePair.Value.SensorList.Any())
                {
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
                            viewSensorStatus.PopulateSensor();
                        }
                        else
                        {
                            ScrollViewSensorStatus.Children.Add(
                                new ViewSensorDetail(sensor));
                        }
                    }
                }
                else
                {
                    if (ScrollViewSensorStatus.Children.Count == 0)
                        ScrollViewSensorStatus.Children.Add(new ViewEmptySchedule("No Sensors Here"));
                }
            }
            catch (Exception e)
            {
                ScrollViewSensorStatus.Children.Add(new ViewException(e));
            }
        }

        private void ScreenCleanupForSchedule()
        {
            try
            {
                if (_observableFilterKeyValuePair.Value.LoadedData)
                {
                    var runningScheduleList =
                        new RunningSchedule(_observableFilterKeyValuePair.Value.ScheduleList,
                                _observableFilterKeyValuePair.Value.EquipmentList)
                            .GetRunningSchedule().ToList();
                    var activeCustomScheduleList = new RunningCustomSchedule().GetActiveCustomSchedule(
                        _observableFilterKeyValuePair.Value.CustomScheduleList.ToList(),
                        _observableFilterKeyValuePair.Value.EquipmentList.ToList());
                    runningScheduleList.AddRange(
                        new RunningCustomSchedule().GetRunningCustomSchedule(activeCustomScheduleList));

                    var queScheduleList =
                        new RunningSchedule(_observableFilterKeyValuePair.Value.ScheduleList,
                                _observableFilterKeyValuePair.Value.EquipmentList)
                            .GetQueSchedule().ToList();
                    queScheduleList.AddRange(
                        new RunningCustomSchedule().GetQueCustomSchedule(activeCustomScheduleList));

                    var itemsThatAreOnDisplay = runningScheduleList.Select(x => x.Id).ToList();
                    itemsThatAreOnDisplay.AddRange(
                        _observableFilterKeyValuePair.Value.ManualScheduleList.Select(x => x?.Id));

                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).AutomationId);


                    for (var index = 0; index < ScrollViewScheduleStatus.Children.Count; index++)
                    {
                        var existingItems = itemsThatAreOnDisplay.FirstOrDefault(x =>
                            x == ScrollViewScheduleStatus.Children[index].AutomationId);
                        if (existingItems != null) continue;
                        ScrollViewScheduleStatus.Children.RemoveAt(index);
                        index--;
                    }

                    itemsThatAreOnDisplay = queScheduleList.Select(x => x?.Id).ToList();
                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).AutomationId);


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
                    var loadingIcon = new ActivityIndicator
                    {
                        AutomationId = "ActivityIndicatorSiteLoading",
                        HorizontalOptions = LayoutOptions.Center,
                        IsEnabled = true,
                        IsRunning = true,
                        IsVisible = true,
                        VerticalOptions = LayoutOptions.Center
                    };

                    if (ScrollViewScheduleStatus.Children.Count > 0 ||
                        ScrollViewScheduleStatus.Children.First().AutomationId != "ActivityIndicatorSiteLoading")
                    {
                        ScrollViewScheduleStatus.Children.Clear();
                        ScrollViewScheduleStatus.Children.Add(loadingIcon);
                    }

                    if (ScrollViewQueueStatus.Children.Count > 0 ||
                        ScrollViewQueueStatus.Children.First().AutomationId != "ActivityIndicatorSiteLoading")
                    {
                        ScrollViewQueueStatus.Children.Clear();
                        ScrollViewQueueStatus.Children.Add(loadingIcon);
                    }
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
                if (_observableFilterKeyValuePair.Value.LoadedData)
                {
                    var itemsThatAreOnDisplay =
                        _observableFilterKeyValuePair.Value.SensorList.Select(x => x?.Id).ToList();
                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).AutomationId);

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
                    if (ScrollViewSensorStatus.Children.Count == 1 &&
                        ScrollViewSensorStatus.Children.First().AutomationId == "ActivityIndicatorSiteLoading")
                        return;
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
            if (_observableFilterKeyValuePair.Value.ScheduleList.Contains(null) &&
                _observableFilterKeyValuePair.Value.CustomScheduleList.Contains(null))
                return;
            var runningStatusViews =
                new RunningSchedule(_observableFilterKeyValuePair.Value.ScheduleList,
                        _observableFilterKeyValuePair.Value.EquipmentList)
                    .GetRunningSchedule().ToList()
                    .Select(queue => new ViewActiveScheduleSummary(queue, PopUpSize)).Cast<View>().ToList();

            var activeCustomScheduleList = new RunningCustomSchedule().GetRunningCustomSchedule(
                    new RunningCustomSchedule().GetActiveCustomSchedule(
                        _observableFilterKeyValuePair.Value.CustomScheduleList.ToList(),
                        _observableFilterKeyValuePair.Value.EquipmentList.ToList()))
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
            var queueStatusViews =
                new RunningSchedule(_observableFilterKeyValuePair.Value.ScheduleList,
                        _observableFilterKeyValuePair.Value.EquipmentList)
                    .GetQueSchedule().ToList()
                    .Select(queue => new ViewActiveScheduleSummary(queue, PopUpSize)).Cast<View>().ToList();


            var queCustomScheduleList = new RunningCustomSchedule().GetQueCustomSchedule(
                    new RunningCustomSchedule().GetActiveCustomSchedule(
                        _observableFilterKeyValuePair.Value.CustomScheduleList.ToList(),
                        _observableFilterKeyValuePair.Value.EquipmentList.ToList()))
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