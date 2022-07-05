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
        private readonly ObservableFilteredIrrigation _observableFilteredIrrigation;
        private readonly SocketPicker _socketPicker;
        private ViewCustomScheduleSummary _viewSchedule;

        public CustomScheduleHomeScreen(ObservableFilteredIrrigation observableFilteredIrrigation, SocketPicker socketPicker)
        {
            InitializeComponent();
            _observableFilteredIrrigation = observableFilteredIrrigation;
            _socketPicker = socketPicker;
            _observableFilteredIrrigation.EquipmentList.CollectionChanged += PopulateCustomScheduleStatusEvent;
            _observableFilteredIrrigation.CustomScheduleList.CollectionChanged += PopulateCustomScheduleStatusEvent;
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
                if (!_observableFilteredIrrigation.LoadedAllData()) return;
                if (_observableFilteredIrrigation.CustomScheduleList.Any())
                {
                    foreach (var customSchedule in _observableFilteredIrrigation.CustomScheduleList)
                    {
                        var viewSchedule = ScrollViewCustomScheduleDetail.Children.FirstOrDefault(x =>
                            x?.AutomationId == customSchedule.Id);
                        if (viewSchedule != null)
                        {
                            var equipment =
                                _observableFilteredIrrigation.EquipmentList.FirstOrDefault(x =>
                                    x?.Id == customSchedule.id_Pump);
                            var viewScheduleStatus = (ViewCustomSchedule)viewSchedule;
                            viewScheduleStatus.Schedule.NAME = customSchedule.NAME;
                            viewScheduleStatus.Schedule.StartTime = customSchedule.StartTime;
                            viewScheduleStatus.Schedule.Repeat = customSchedule.Repeat;
                            if (viewScheduleStatus.Equipment != null)
                                viewScheduleStatus.Equipment.NAME = equipment?.NAME;
                            viewScheduleStatus.Populate();
                        }
                        else
                        {
                            var viewScheduleSettingSummary = new ViewCustomSchedule(customSchedule,
                                _observableFilteredIrrigation.EquipmentList.FirstOrDefault(x =>
                                    x?.Id == customSchedule.id_Pump));
                            ScrollViewCustomScheduleDetail.Children.Add(viewScheduleSettingSummary);
                            viewScheduleSettingSummary.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                            viewScheduleSettingSummary.GetTapGestureRecognizer().Tapped += ViewScheduleScreen_Tapped;
                        }
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
                if (_observableFilteredIrrigation.LoadedAllData())
                {
                    var itemsThatAreOnDisplay = _observableFilteredIrrigation.CustomScheduleList.Select(x => x?.Id).ToList();
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
                    _observableFilteredIrrigation.CustomScheduleList.First(x => x?.Id == scheduleSwitch.AutomationId);

                updateSchedule.StartTime = scheduleSwitch.IsToggled ? ScheduleTime.GetUnixTimeStampUtcNow() : 0;

                ChangeCustomScheduleState(updateSchedule);
            }
            catch
            {
                //TODO Able to display Messages on Main Thread
                await Application.Current.MainPage.DisplayAlert("Warning!!!",
                    "This switch failed to parse it's ID \n COULD NOT CHANGE SCHEDULE STATE",
                    "Understood");
            }
        }

        private void ChangeCustomScheduleState(CustomSchedule schedule)
        {
            foreach (var view in ScrollViewCustomScheduleDetail.Children)
            {
                if (view.GetType() == typeof(ViewException))
                    continue;
                var viewCustomSchedule = (ViewCustomSchedule)view;
                if (viewCustomSchedule.Schedule.Id != schedule.Id) continue;
                viewCustomSchedule.Schedule = schedule;

                async void Action()
                {
                    viewCustomSchedule.GetSwitch().Toggled -= ScheduleSwitch_Toggled;
                    viewCustomSchedule.Populate();
                    viewCustomSchedule.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                    //TODO Needs Confirmation that The Pi got it and its running :)
                    await _socketPicker.SendCommand(schedule);
                }

                Device.BeginInvokeOnMainThread(Action);
            }
        }

        private void ViewScheduleSummary(string id)
        {
            PopupNavigation.Instance.PushAsync(_floatingScreen);
            new Thread(() => GetScheduleSummary(id)).Start();
        }

        private void GetScheduleSummary(string id)
        {
            var schedule = _observableFilteredIrrigation.CustomScheduleList.FirstOrDefault(x => x?.Id == id);
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


                _viewSchedule = new ViewCustomScheduleSummary(schedule, _observableFilteredIrrigation.EquipmentList.ToList());

                _viewSchedule.GetButtonEdit().Clicked += EditButton_Tapped;
                _viewSchedule.GetButtonDelete().Clicked += DeleteButton_Tapped;
                customScheduleSummaryListObject.Add(_viewSchedule);
                var zoneAndTimeTapGesture = _viewSchedule.GetZoneAndTimeGestureRecognizers();
                foreach (var t in zoneAndTimeTapGesture) t.Tapped += SkipCustomSchedule_Tapped;


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
            if (_observableFilteredIrrigation.EquipmentList.Count > 0)
                Navigation.PushModalAsync(new CustomScheduleUpdate(_observableFilteredIrrigation.EquipmentList.ToList(),
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
                _observableFilteredIrrigation.CustomScheduleList.First(schedule => schedule.Id == edit.AutomationId);
            Navigation.PushModalAsync(new CustomScheduleUpdate(_observableFilteredIrrigation.EquipmentList.ToList(),
                _socketPicker, customSchedule));
        }

        private void DeleteButton_Tapped(object sender, EventArgs e)
        {
            var delete = (Button)sender;
            var customSchedule =
                _observableFilteredIrrigation.CustomScheduleList.First(schedule => schedule.Id == delete.AutomationId);
            var deleteConfirm = new ViewDeleteConfirmation(customSchedule);
            _floatingScreen.SetFloatingScreen(new List<object> { deleteConfirm });
            deleteConfirm.GetDeleteButton().Clicked += DeleteConfirmButton_Tapped;
        }

        private async void DeleteConfirmButton_Tapped(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
            var delete = (Button)sender;
            await _socketPicker.SendCommand(new CustomSchedule { Id = delete.AutomationId, DeleteAwaiting = true });
        }

        private async void SkipCustomSchedule_Tapped(object sender, EventArgs e)
        {
            var gridEquipmentAndTime = (Grid)sender;
            int.TryParse(gridEquipmentAndTime.AutomationId, out var selectIndex);
            var customScheduleDetails = new List<ScheduleDetail>();
            for (var i = 0; i < _viewSchedule.CustomSchedule.Repeat + 1; i++)
                foreach (var scheduleDetail in _viewSchedule.CustomSchedule.ScheduleDetails)
                    customScheduleDetails.Add(scheduleDetail);

            var selectedCustomScheduleDetails = customScheduleDetails[selectIndex];


            if (!await Application.Current.MainPage.DisplayAlert("Are you sure?",
                    "You have selected " +
                    _observableFilteredIrrigation.EquipmentList.First(x => x?.Id == selectedCustomScheduleDetails.id_Equipment)
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
                    (int)startTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
                _viewSchedule.UpdateScheduleSummary();
                ChangeCustomScheduleState(_viewSchedule.CustomSchedule);
            }
        }
    }
}