using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database.Streaming;
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
        private readonly ObservableCollection<Equipment> _equipmentList;
        private readonly ObservableCollection<CustomSchedule> _customSchedulesList;
        private readonly FloatingScreen _floatingScreen = new FloatingScreen();
        private ViewCustomScheduleSummary _viewSchedule;
        private readonly ObservableCollection<Site> _siteList;
        private readonly PumpConnection _pumpConnection;

        public CustomScheduleHomeScreen(ObservableCollection<CustomSchedule> customSchedulesList, ObservableCollection<Equipment> equipmentList,
            ObservableCollection<Site> siteList)
        {
            _equipmentList = equipmentList;
            _customSchedulesList = customSchedulesList;
            _siteList = siteList;
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
                    if (!_equipmentList.Contains(null) && !_customSchedulesList.Contains(null))
                    {
                        hasSubscribed = true;
                        _equipmentList.CollectionChanged += PopulateCustomScheduleStatusEvent;
                        _customSchedulesList.CollectionChanged += PopulateCustomScheduleStatusEvent;
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
                if (_customSchedulesList.Contains(null) || _equipmentList.Contains(null)) return;
                if (_customSchedulesList.Any())
                    foreach (var customSchedule in _customSchedulesList.Where(x => _siteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.id_Pump)))
                    {
                        var viewSchedule = ScrollViewCustomScheduleDetail.Children.FirstOrDefault(x =>
                            x.AutomationId == customSchedule.ID);
                        if (viewSchedule != null)
                        {
                            var equipment = _equipmentList.First(x => x.ID == customSchedule.id_Pump);
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
                                _equipmentList.First(x => x.ID == customSchedule.id_Pump));
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
                if (!_customSchedulesList.Contains(null))
                {

                    var itemsThatAreOnDisplay = _customSchedulesList.Select(x => x.ID).ToList();
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
                var updateSchedule = _customSchedulesList.First(x => x.ID == scheduleSwitch.AutomationId);

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
            if (new DatabaseController().IsRealtimeFirebaseSelected())
            {

                var schedule = _customSchedulesList.FirstOrDefault(x => x.ID == id);
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


                _viewSchedule = new ViewCustomScheduleSummary(schedule, _equipmentList.ToList());

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

            if (new DatabaseController().IsRealtimeFirebaseSelected())
            {
                if (_equipmentList.Count > 0)
                    Navigation.PushModalAsync(new CustomScheduleUpdate(_equipmentList.Where(x => _siteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)).ToList()));
                else
                    DisplayAlert("Cannot Create a Schedule",
                        "You are missing the equipment that is needed to create a schedule", "Understood");
            }
        }

        private void EditButton_Tapped(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            var edit = (Button)sender;
            var customSchedule = _customSchedulesList.First(schedule => schedule.ID == edit.AutomationId);
            Navigation.PushModalAsync(new CustomScheduleUpdate(_equipmentList.Where(x => _siteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)).ToList(), customSchedule));
        }

        private void DeleteButton_Tapped(object sender, EventArgs e)
        {
            var delete = (Button)sender;
            var customSchedule = _customSchedulesList.First(schedule => schedule.ID == delete.AutomationId);
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
                    "You have selected " + _equipmentList.First(x => x.ID == selectedCustomScheduleDetails.id_Equipment).NAME  + "\nConfirm to skip to this zone ?", "Confirm",
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