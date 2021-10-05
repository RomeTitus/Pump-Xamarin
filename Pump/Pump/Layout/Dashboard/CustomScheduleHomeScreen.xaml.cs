using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pump.Class;
using Pump.IrrigationController;
using Pump.Layout.Schedule;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Dashboard
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomScheduleHomeScreen : ContentView
    {
        private readonly FloatingScreen _floatingScreen = new FloatingScreen();
        private readonly ObservableSiteIrrigation _observableIrrigation;
        private readonly SocketPicker _socketPicker;
        private ViewCustomScheduleSummary _viewSchedule;

        public CustomScheduleHomeScreen(ObservableSiteIrrigation observableIrrigation, SocketPicker socketPicker)
        {
            InitializeComponent();
            _observableIrrigation = observableIrrigation;
            _socketPicker = socketPicker;
            _observableIrrigation.EquipmentList.CollectionChanged += PopulateCustomScheduleStatusEvent;
            _observableIrrigation.CustomScheduleList.CollectionChanged += PopulateCustomScheduleStatusEvent;
            PopulateCustomScheduleStatus();
        }

        private void PopulateCustomScheduleStatusEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateCustomScheduleStatus);
        }

        private void PopulateCustomScheduleStatus()
        {
            ScreenCleanupForCustomSchedules();

            try
            {
                if (!_observableIrrigation.LoadedAllData()) return;
                if (_observableIrrigation.CustomScheduleList.Any())
                    foreach (var customSchedule in _observableIrrigation.CustomScheduleList)
                    {
                        var viewSchedule = ScrollViewCustomScheduleDetail.Children.FirstOrDefault(x =>
                            x?.AutomationId == customSchedule.ID);
                        if (viewSchedule != null)
                        {
                            var equipment =
                                _observableIrrigation.EquipmentList.FirstOrDefault(x =>
                                    x?.ID == customSchedule.id_Pump);
                            var viewScheduleStatus = (ViewCustomSchedule)viewSchedule;
                            viewScheduleStatus.Schedule.NAME = customSchedule.NAME;
                            viewScheduleStatus.Schedule.StartTime = customSchedule.StartTime;
                            viewScheduleStatus.Schedule.Repeat = customSchedule.Repeat;
                            viewScheduleStatus.Equipment.NAME = equipment?.NAME;
                            viewScheduleStatus.Populate();
                        }
                        else
                        {
                            var viewScheduleSettingSummary = new ViewCustomSchedule(customSchedule,
                                _observableIrrigation.EquipmentList.FirstOrDefault(x =>
                                    x?.ID == customSchedule.id_Pump));
                            ScrollViewCustomScheduleDetail.Children.Add(viewScheduleSettingSummary);
                            viewScheduleSettingSummary.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                            viewScheduleSettingSummary.GetTapGestureRecognizer().Tapped += ViewScheduleScreen_Tapped;
                        }
                    }
                else
                {
                    if (ScrollViewCustomScheduleDetail.Children.Count == 0)
                        ScrollViewCustomScheduleDetail.Children.Add(new ViewEmptySchedule("No Custom Schedules Here"));
                }
            }
            catch (Exception e)
            {
                ScrollViewCustomScheduleDetail.Children.Add(new ViewException(e));
            }
        }

        private void ScreenCleanupForCustomSchedules()
        {
            try
            {
                if (_observableIrrigation.LoadedAllData())
                {
                    var itemsThatAreOnDisplay = _observableIrrigation.CustomScheduleList.Select(x => x?.ID).ToList();
                    if (!itemsThatAreOnDisplay.Any())
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).AutomationId);


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
                    if (ScrollViewCustomScheduleDetail.Children.Count == 1 &&
                        ScrollViewCustomScheduleDetail.Children.First().AutomationId ==
                        "ActivityIndicatorSiteLoading") return;

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

        private async void ScheduleSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            var scheduleSwitch = (Switch)sender;
            try
            {
                var updateSchedule =
                    _observableIrrigation.CustomScheduleList.First(x => x?.ID == scheduleSwitch.AutomationId);

                updateSchedule.StartTime = scheduleSwitch.IsToggled ? ScheduleTime.GetUnixTimeStampUtcNow() : 0;

                await ChangeCustomScheduleState(updateSchedule);
            }
            catch
            {
                //TODO Able to display Messages on Main Thread
                await Application.Current.MainPage.DisplayAlert("Warning!!!",
                    "This switch failed to parse it's ID \n COULD NOT CHANGE SCHEDULE STATE",
                    "Understood");
            }
        }

        private async Task ChangeCustomScheduleState(CustomSchedule schedule)
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
            await _socketPicker.SendCommand(schedule);
        }

        private void ViewScheduleSummary(string id)
        {
            PopupNavigation.Instance.PushAsync(_floatingScreen);
            new Thread(() => GetScheduleSummary(id)).Start();
        }

        private void GetScheduleSummary(string id)
        {
            var schedule = _observableIrrigation.CustomScheduleList.FirstOrDefault(x => x?.ID == id);
            var scheduleList = GetCustomScheduleSummaryObject(schedule);

            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    _floatingScreen.SetFloatingScreen(scheduleList);
                }
                catch (Exception e)
                {
                    var scheduleSummaryListObject = new List<object> { new ViewException(e) };
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
            catch (Exception e)
            {
                customScheduleSummaryListObject = new List<object> { new ViewException(e) };
                return customScheduleSummaryListObject;
            }
        }

        private void ButtonCreateCustomSchedule_OnClicked(object sender, EventArgs e)
        {
            if (_observableIrrigation.EquipmentList.Count > 0)
                Navigation.PushModalAsync(new CustomScheduleUpdate(_observableIrrigation.EquipmentList.ToList(),
                    _socketPicker));
            else
                Application.Current.MainPage.DisplayAlert("Cannot Create a Schedule",
                    "You are missing the equipment that is needed to create a schedule", "Understood");
        }

        private void EditButton_Tapped(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            var edit = (Button)sender;
            var customSchedule =
                _observableIrrigation.CustomScheduleList.First(schedule => schedule.ID == edit.AutomationId);
            Navigation.PushModalAsync(new CustomScheduleUpdate(_observableIrrigation.EquipmentList.ToList(),
                _socketPicker, customSchedule));
        }

        private void DeleteButton_Tapped(object sender, EventArgs e)
        {
            var delete = (Button)sender;
            var customSchedule =
                _observableIrrigation.CustomScheduleList.First(schedule => schedule.ID == delete.AutomationId);
            var deleteConfirm = new ViewDeleteConfirmation(customSchedule);
            _floatingScreen.SetFloatingScreen(new List<object> { deleteConfirm });
            deleteConfirm.GetDeleteButton().Clicked += DeleteConfirmButton_Tapped;
        }

        private async void DeleteConfirmButton_Tapped(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
            var delete = (Button)sender;
            await _socketPicker.SendCommand(new CustomSchedule { ID = delete.AutomationId, DeleteAwaiting = true });
        }

        private async void SkipCustomSchedule_Tapped(object sender, EventArgs e)
        {
            var gridEquipmentAndTime = (Grid)sender;
            int.TryParse(gridEquipmentAndTime.AutomationId, out var selectIndex);
            var customScheduleDetails = new List<ScheduleDetail>();
            for (int i = 0; i < _viewSchedule.CustomSchedule.Repeat + 1; i++)
            {
                foreach (var scheduleDetail in _viewSchedule.CustomSchedule.ScheduleDetails)
                {
                    customScheduleDetails.Add(scheduleDetail);
                }
            }

            var selectedCustomScheduleDetails = customScheduleDetails[selectIndex];


            if (!await Application.Current.MainPage.DisplayAlert("Are you sure?",
                "You have selected " +
                _observableIrrigation.EquipmentList.First(x => x?.ID == selectedCustomScheduleDetails.id_Equipment)
                    .NAME + "\nConfirm to skip to this zone ?", "Confirm",
                "cancel")) return;
            if (_viewSchedule == null) return;
            var nullableStartTime =
                RunningCustomSchedule.GetCustomScheduleRunningTimeForEquipment(_viewSchedule.CustomSchedule,
                    selectIndex);
            if (nullableStartTime != null)
            {
                var startTime = (DateTime)nullableStartTime;
                _viewSchedule.CustomSchedule.StartTime =
                    (Int32)(startTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                _viewSchedule.UpdateScheduleSummary();
                await ChangeCustomScheduleState(_viewSchedule.CustomSchedule);
            }
        }
    }
}