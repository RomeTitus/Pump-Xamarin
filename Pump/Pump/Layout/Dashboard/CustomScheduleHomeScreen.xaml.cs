using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pump.Class;
using Pump.Database;
using Pump.Droid.Database.Table;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomScheduleHomeScreen : ContentPage
    {
        private readonly ObservableIrrigation _observableIrrigation;
        private readonly FloatingScreen _floatingScreen = new FloatingScreen();
        private ViewCustomScheduleSummary _viewSchedule;
        private readonly PumpConnection _pumpConnection;

        public CustomScheduleHomeScreen(ObservableIrrigation observableIrrigation)
        {
            _observableIrrigation = observableIrrigation;
            InitializeComponent();
            _pumpConnection = new DatabaseController().GetControllerConnectionSelection();
            new Thread(LoadCustomScheduleStatus).Start();
        }

        private void LoadCustomScheduleStatus()
        {
            var hasSubscribed = false;
            while (!hasSubscribed)
            {
                try
                {
                    if (!_observableIrrigation.EquipmentList.Contains(null) && !_observableIrrigation.CustomScheduleList.Contains(null))
                    {
                        hasSubscribed = true;
                        _observableIrrigation.EquipmentList.CollectionChanged += PopulateCustomScheduleStatusEvent;
                        _observableIrrigation.CustomScheduleList.CollectionChanged += PopulateCustomScheduleStatusEvent;
                        Device.InvokeOnMainThreadAsync(PopulateCustomScheduleStatus);
                    }
                }
                catch
                {
                    // ignored
                }
                Thread.Sleep(100);
            }
        }

        private void PopulateCustomScheduleStatusEvent(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateCustomScheduleStatus);
        }

        private void PopulateCustomScheduleStatus()
        {
            ScreenCleanupForCustomSchedules();

            try
            {
                if (_observableIrrigation.CustomScheduleList.Contains(null) || _observableIrrigation.EquipmentList.Contains(null)) return;
                if (_observableIrrigation.CustomScheduleList.Any())
                    foreach (var customSchedule in _observableIrrigation.CustomScheduleList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.id_Pump)))
                    {
                        var viewSchedule = ScrollViewCustomScheduleDetail.Children.FirstOrDefault(x =>
                            x.AutomationId == customSchedule.ID);
                        if (viewSchedule != null)
                        {
                            var equipment = _observableIrrigation.EquipmentList.First(x => x.ID == customSchedule.id_Pump);
                            var viewScheduleStatus = (ViewCustomSchedule)viewSchedule;
                            viewScheduleStatus.Schedule.NAME = customSchedule.NAME;
                            viewScheduleStatus.Schedule.StartTime = customSchedule.StartTime;
                            viewScheduleStatus.Schedule.Repeat = customSchedule.Repeat;
                            viewScheduleStatus.Equipment.NAME = equipment.NAME;
                            viewScheduleStatus.Equipment.NAME = equipment.NAME;
                            viewScheduleStatus.Populate();
                        }
                        else
                        {
                            var viewScheduleSettingSummary = new ViewCustomSchedule(customSchedule,
                                _observableIrrigation.EquipmentList.First(x => x.ID == customSchedule.id_Pump));
                            ScrollViewCustomScheduleDetail.Children.Add(viewScheduleSettingSummary);
                            viewScheduleSettingSummary.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                            viewScheduleSettingSummary.GetTapGestureRecognizer().Tapped += ViewScheduleScreen_Tapped;
                        }
                    }
                else
                {
                    ScrollViewCustomScheduleDetail.Children.Add(new ViewEmptySchedule("No Custom Schedules Here"));
                }
            }
            catch
            {
                // ignored
            }
        }

        private void ScreenCleanupForCustomSchedules()
        {

            try
            {
                if (!_observableIrrigation.CustomScheduleList.Contains(null))
                {

                    var itemsThatAreOnDisplay = _observableIrrigation.CustomScheduleList.Select(x => x.ID).ToList();
                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).ID);


                    for (var index = 0; index < ScrollViewCustomScheduleDetail.Children.Count; index++)
                    {
                        var existingItems = itemsThatAreOnDisplay.FirstOrDefault(x =>
                            x == ScrollViewCustomScheduleDetail.Children[index].AutomationId);
                        if (existingItems != null) continue;
                        ScrollViewCustomScheduleDetail.Children.RemoveAt(index);
                        index--;
                    }
                }
                else
                {
                    ScrollViewCustomScheduleDetail.Children.Clear();
                    var loadingIcon = new ActivityIndicator
                    {
                        AutomationId = "ActivityIndicatorSiteLoading",
                        HorizontalOptions = LayoutOptions.Center,
                        IsEnabled = true,
                        IsRunning = true,
                        IsVisible = true,
                        VerticalOptions = LayoutOptions.Center
                    };
                    ScrollViewCustomScheduleDetail.Children.Add(loadingIcon);
                }

            }
            catch
            {
                // ignored
            }
        }

        private void ViewScheduleScreen_Tapped(object sender, EventArgs e)
        {
            var scheduleSwitch = (View)sender;
            ViewScheduleSummary(scheduleSwitch.AutomationId);
        }

        private void ScheduleSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            var scheduleSwitch = (Switch)sender;
            try
            {
                var updateSchedule = _observableIrrigation.CustomScheduleList.First(x => x.ID == scheduleSwitch.AutomationId);

                if (scheduleSwitch.IsToggled)
                    updateSchedule.StartTime = ScheduleTime.GetUnixTimeStampUtcNow();
                else
                    updateSchedule.StartTime = 0;

                new Thread(() => ChangeCustomScheduleState(updateSchedule))
                    .Start();
            }
            catch
            {
                DisplayAlert("Warning!!!", "This switch failed to parse it's ID \n COULD NOT CHANGE SCHEDULE STATE",
                    "Understood");
            }
        }

        private void ChangeCustomScheduleState(CustomSchedule schedule)
        {


            foreach (var view in ScrollViewCustomScheduleDetail.Children)
            {
                var viewCustomSchedule = (ViewCustomSchedule)view;
                if (viewCustomSchedule.Schedule.ID != schedule.ID) continue;
                viewCustomSchedule.Schedule = schedule;
                
                Device.BeginInvokeOnMainThread(() =>
                {
                    viewCustomSchedule.GetSwitch().Toggled -= ScheduleSwitch_Toggled;
                    viewCustomSchedule.Populate();
                    viewCustomSchedule.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                });
            }
            //TODO Needs Confirmation that The Pi got it and its running :)
            var key = Task.Run(() => new Authentication().SetCustomSchedule(schedule)).Result;
        }

        private void ViewScheduleSummary(string id)
        {

            PopupNavigation.Instance.PushAsync(_floatingScreen);
            new Thread(() => GetScheduleSummary(id)).Start();
        }
        private void GetScheduleSummary(string id)
        {
            var schedule = _observableIrrigation.CustomScheduleList.FirstOrDefault(x => x.ID == id);
            var scheduleList = GetCustomScheduleSummaryObject(schedule);

            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    _floatingScreen.SetFloatingScreen(scheduleList);
                }
                catch
                {
                    var scheduleSummaryListObject = new List<object> { new ViewException() };
                    _floatingScreen.SetFloatingScreen(scheduleSummaryListObject);
                }
            });
            
        }

        private List<object> GetCustomScheduleSummaryObject(CustomSchedule schedule)
        {
            var customScheduleSummaryListObject = new List<object>();
            try
            {
                if (schedule == null)
                {
                    customScheduleSummaryListObject.Add(new ViewEmptySchedule("No Schedules Details Found"));
                    return customScheduleSummaryListObject;
                }


                _viewSchedule = new ViewCustomScheduleSummary(schedule, _observableIrrigation.EquipmentList.ToList());

                _viewSchedule.GetButtonEdit().Clicked += EditButton_Tapped;
                _viewSchedule.GetButtonDelete().Clicked += DeleteButton_Tapped;
                customScheduleSummaryListObject.Add(_viewSchedule);
                var zoneAndTimeTapGesture = _viewSchedule.GetZoneAndTimeGestureRecognizers();
                foreach (var t in zoneAndTimeTapGesture)
                {
                    t.Tapped += SkipCustomSchedule_Tapped;
                }


                return customScheduleSummaryListObject;
            }
            catch
            {
                customScheduleSummaryListObject = new List<object> { new ViewException() };
                return customScheduleSummaryListObject;
            }
        }

        private void ButtonCreateCustomSchedule_OnClicked(object sender, EventArgs e)
        {

                if (_observableIrrigation.EquipmentList.Count > 0)
                    Navigation.PushModalAsync(new CustomScheduleUpdate(_observableIrrigation.EquipmentList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)).ToList()));
                else
                    DisplayAlert("Cannot Create a Schedule",
                        "You are missing the equipment that is needed to create a schedule", "Understood");
            
        }

        private void EditButton_Tapped(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            var edit = (Button)sender;
            var customSchedule = _observableIrrigation.CustomScheduleList.First(schedule => schedule.ID == edit.AutomationId);
            Navigation.PushModalAsync(new CustomScheduleUpdate(_observableIrrigation.EquipmentList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)).ToList(), customSchedule));
        }

        private void DeleteButton_Tapped(object sender, EventArgs e)
        {
            var delete = (Button)sender;
            var customSchedule = _observableIrrigation.CustomScheduleList.First(schedule => schedule.ID == delete.AutomationId);
            var deleteConfirm = new ViewDeleteConfirmation(customSchedule);
            _floatingScreen.SetFloatingScreen(new List<object> { deleteConfirm });
            deleteConfirm.GetDeleteButton().Clicked += DeleteConfirmButton_Tapped;
        }

        private void DeleteConfirmButton_Tapped(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            var delete = (Button)sender;
            new Authentication().DeleteCustomSchedule(new CustomSchedule { ID = delete.AutomationId });
        }

        private void SkipCustomSchedule_Tapped(object sender, EventArgs e)
        {
            var gridEquipmentAndTime = (Grid)sender;
            int.TryParse(gridEquipmentAndTime.AutomationId, out var selectIndex);
            var customScheduleDetails = new List<ScheduleDetail>();
            for (int i = 0; i < _viewSchedule.CustomSchedule.Repeat+1; i++)
            {
                foreach (var scheduleDetail in _viewSchedule.CustomSchedule.ScheduleDetails)
                {
                    customScheduleDetails.Add(scheduleDetail);
                }
            }
            
            var selectedCustomScheduleDetails = customScheduleDetails[selectIndex];

            Device.BeginInvokeOnMainThread(async () =>
            {
                if (!await DisplayAlert("Are you sure?",
                    "You have selected " + _observableIrrigation.EquipmentList.First(x => x.ID == selectedCustomScheduleDetails.id_Equipment).NAME  + "\nConfirm to skip to this zone ?", "Confirm",
                    "cancel")) return;
                if (_viewSchedule == null) return;
                var nullableStartTime = RunningCustomSchedule.GetCustomScheduleRunningTimeForEquipment(_viewSchedule.CustomSchedule, selectIndex);
                if (nullableStartTime != null)
                {
                    var startTime = (DateTime)nullableStartTime;
                    _viewSchedule.CustomSchedule.StartTime =
                        (Int32)(startTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                    _viewSchedule.UpdateScheduleSummary();
                    ChangeCustomScheduleState(_viewSchedule.CustomSchedule);
                }
            });
        }
    }
}