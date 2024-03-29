﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
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
    public partial class ScheduleHomeScreen : ContentView
    {
        private readonly ObservableSiteIrrigation _observableIrrigation;
        private readonly FloatingScreen _floatingScreen = new FloatingScreen();
        private ViewScheduleSummary _viewSchedule;
        private readonly SocketPicker _socketPicker;

        public ScheduleHomeScreen(ObservableSiteIrrigation observableIrrigation, SocketPicker socketPicker)
        {
            InitializeComponent();
            _observableIrrigation = observableIrrigation;
            _socketPicker = socketPicker;
            _observableIrrigation.EquipmentList.CollectionChanged += PopulateScheduleStatusEvent;
            _observableIrrigation.ScheduleList.CollectionChanged += PopulateScheduleStatusEvent;
            PopulateScheduleStatus();
        }
        private void PopulateScheduleStatusEvent(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateScheduleStatus);
        }

        private void PopulateScheduleStatus()
        {
            ScreenCleanupForSchedules();
            try
            {
                if(!_observableIrrigation.LoadedAllData())return;
                if (_observableIrrigation.ScheduleList.Any())
                    foreach (var schedule in _observableIrrigation.ScheduleList.ToList())
                    {
                        var viewSchedule = ScrollViewScheduleDetail.Children.FirstOrDefault(x =>
                            x.AutomationId == schedule.ID);
                        if (viewSchedule != null)
                        {
                            var equipment = _observableIrrigation.EquipmentList.FirstOrDefault(x => x?.ID == schedule.id_Pump);
                            var viewScheduleStatus = (ViewScheduleSettingSummary)viewSchedule;
                            viewScheduleStatus.Schedule.NAME = schedule.NAME;
                            viewScheduleStatus.Schedule.TIME = schedule.TIME;
                            viewScheduleStatus.Schedule.isActive = schedule.isActive;
                            viewScheduleStatus.Equipment.NAME = equipment?.NAME;
                            viewScheduleStatus.Populate();
                        }
                        else
                        {
                            var viewScheduleSettingSummary = new ViewScheduleSettingSummary(schedule,
                                _observableIrrigation.EquipmentList.FirstOrDefault(x => x?.ID == schedule.id_Pump));
                            ScrollViewScheduleDetail.Children.Add(viewScheduleSettingSummary);
                            viewScheduleSettingSummary.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                            viewScheduleSettingSummary.GetTapGestureRecognizer().Tapped += ViewScheduleScreen_Tapped;
                        }
                    }
                else
                {
                    if(ScrollViewScheduleDetail.Children.Count == 0)
                        ScrollViewScheduleDetail.Children.Add(new ViewEmptySchedule("No Schedules Here"));
                }
            }
            catch (Exception e)
            {
                ScrollViewScheduleDetail.Children.Add(new ViewException(e));
            }
        }

        private void ScreenCleanupForSchedules()
        {
            try
            {
                if (_observableIrrigation.LoadedAllData())
                {

                    var itemsThatAreOnDisplay = _observableIrrigation.ScheduleList.Select(x => x?.ID).ToList();
                    if (!itemsThatAreOnDisplay.Any())
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).AutomationId);


                    for (var index = 0; index < ScrollViewScheduleDetail.Children.Count; index++)
                    {
                        var existingItems = itemsThatAreOnDisplay.FirstOrDefault(x =>
                            x == ScrollViewScheduleDetail.Children[index].AutomationId);
                        if (existingItems != null) continue;
                        ScrollViewScheduleDetail.Children.RemoveAt(index);
                        index--;
                    }
                }
                else
                {
                    if (ScrollViewScheduleDetail.Children.Count == 1 && ScrollViewScheduleDetail.Children.First().AutomationId == "ActivityIndicatorSiteLoading") return;
                    
                    ScrollViewScheduleDetail.Children.Clear();
                    var loadingIcon = new ActivityIndicator
                    {
                        AutomationId = "ActivityIndicatorSiteLoading",
                        HorizontalOptions = LayoutOptions.Center,
                        IsEnabled = true,
                        IsRunning = true,
                        IsVisible = true,
                        VerticalOptions = LayoutOptions.Center
                    };
                    ScrollViewScheduleDetail.Children.Add(loadingIcon);
                }
            }
            catch (Exception e)
            {
                ScrollViewScheduleDetail.Children.Add(new ViewException(e));
            }
        }

        private void GetScheduleSummary(string id)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    _floatingScreen.SetFloatingScreen(GetScheduleSummaryObject(_observableIrrigation.ScheduleList.FirstOrDefault(x => x?.ID == id)));
                }
                catch (Exception e)
                {
                    var scheduleSummaryListObject = new List<object> { new ViewException(e) };
                    _floatingScreen.SetFloatingScreen(scheduleSummaryListObject);
                }
            });
        }

        private List<object> GetScheduleSummaryObject(IrrigationController.Schedule schedule)
        {
            var scheduleSummaryListObject = new List<object>();
            try
            {
                if (schedule == null)
                {
                    scheduleSummaryListObject.Add(new ViewEmptySchedule("No Schedules Details Found"));
                    return scheduleSummaryListObject;
                }


                _viewSchedule = new ViewScheduleSummary(schedule, _observableIrrigation.EquipmentList.ToList());

                _viewSchedule.GetButtonEdit().Clicked += EditButton_Tapped;
                _viewSchedule.GetButtonDelete().Clicked += DeleteButton_Tapped;
                scheduleSummaryListObject.Add(_viewSchedule);
                return scheduleSummaryListObject;
            }
            catch (Exception e)
            {
                scheduleSummaryListObject = new List<object> { new ViewException(e) };
                return scheduleSummaryListObject;
            }
        }

        private void ViewScheduleSummary(string id)
        {

            PopupNavigation.Instance.PushAsync(_floatingScreen);
            new Thread(() => GetScheduleSummary(id)).Start();
        }

        private void ViewScheduleScreen_Tapped(object sender, EventArgs e)
        {
            var scheduleSwitch = (View)sender;
            ViewScheduleSummary(scheduleSwitch.AutomationId);
        }


        private void EditButton_Tapped(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            var edit = (Button)sender;
            var schedule = _observableIrrigation.ScheduleList.First(x => x?.ID == edit.AutomationId);
            Navigation.PushModalAsync(new ScheduleUpdate(_observableIrrigation.EquipmentList.ToList(), _socketPicker, schedule));
        }

        private void DeleteButton_Tapped(object sender, EventArgs e)
        {
            var delete = (Button)sender;
            var schedule = _observableIrrigation.ScheduleList.First(x => x?.ID == delete.AutomationId);
            var deleteConfirm = new ViewDeleteConfirmation(schedule);
            _floatingScreen.SetFloatingScreen(new List<object> { deleteConfirm });
            deleteConfirm.GetDeleteButton().Clicked += DeleteConfirmButton_Tapped;
        }

        private async void DeleteConfirmButton_Tapped(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
            var delete = (Button)sender;
            var schedule = _observableIrrigation.ScheduleList.First(x => x?.ID == delete.AutomationId);
            schedule.DeleteAwaiting = true;
            await _socketPicker.SendCommand(schedule);
        }

        private async void ButtonCreateSchedule_OnClicked(object sender, EventArgs e)
        {
            if (_observableIrrigation.EquipmentList.Count > 0)
                await Navigation.PushModalAsync(new ScheduleUpdate(_observableIrrigation.EquipmentList.ToList(), _socketPicker));
            else
                await Application.Current.MainPage.DisplayAlert("Cannot Create a Schedule",
                    "You are missing the equipment that is needed to create a schedule", "Understood");
        }

        private async void ScheduleSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            var scheduleSwitch = (Switch)sender;
            try
            {
                var updateSchedule = _observableIrrigation.ScheduleList.First(x => x?.ID == scheduleSwitch.AutomationId);

                if (scheduleSwitch.IsToggled)
                    updateSchedule.isActive = "1";
                
                else
                    updateSchedule.isActive = "0";

                await ChangeScheduleState(updateSchedule);
            }
            catch
            { 
                await Application.Current.MainPage.DisplayAlert("Warning!!!", "This switch failed to parse it's ID \n COULD NOT CHANGE SCHEDULE STATE",
                    "Understood");
            }
        }

        private async Task ChangeScheduleState(IrrigationController.Schedule schedule)
        {
            var viewScheduleScreen = ScrollViewScheduleDetail.Children.First(x => (((ViewScheduleSettingSummary)x).Schedule.ID == schedule.ID));
            var viewSchedule = (ViewScheduleSettingSummary) viewScheduleScreen;
            
            viewSchedule.Schedule = schedule;

            Device.BeginInvokeOnMainThread(() =>
            {
                viewSchedule.GetSwitch().Toggled -= ScheduleSwitch_Toggled;
                viewSchedule.Populate();
                viewSchedule.GetSwitch().Toggled += ScheduleSwitch_Toggled;
            });
            
            //TODO Needs Confirmation that The Pi got it and its running :)
            await _socketPicker.SendCommand(schedule);

        }
    }
}