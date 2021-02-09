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
    public partial class ScheduleHomeScreen : ContentPage
    {
        private readonly ObservableIrrigation _observableIrrigation;
        private readonly FloatingScreen _floatingScreen = new FloatingScreen();
        private ViewScheduleSummary _viewSchedule;
        private readonly PumpConnection _pumpConnection;

        public ScheduleHomeScreen(ObservableIrrigation observableIrrigation)
        {
            _observableIrrigation = observableIrrigation;
            InitializeComponent();
            _pumpConnection = new DatabaseController().GetControllerConnectionSelection();
            new Thread(LoadScheduleStatus).Start();
        }

        private void LoadScheduleStatus()
        {
            var hasSubscribed = false;
            while (!hasSubscribed)
            {
                try
                {
                    if (_observableIrrigation.EquipmentList != null && _observableIrrigation.ScheduleList != null)
                    {
                        hasSubscribed = true;
                        _observableIrrigation.EquipmentList.CollectionChanged += PopulateScheduleStatusEvent;
                        _observableIrrigation.ScheduleList.CollectionChanged += PopulateScheduleStatusEvent;
                        Device.InvokeOnMainThreadAsync(PopulateScheduleStatus);
                    }
                }
                catch
                {
                    // ignored
                }
                Thread.Sleep(100);
            }
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
                if(_observableIrrigation.ScheduleList.Contains(null) || _observableIrrigation.EquipmentList.Contains(null))return;
                if (_observableIrrigation.ScheduleList.Any())
                    foreach (var schedule in _observableIrrigation.ScheduleList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.id_Pump)))
                    {
                        var viewSchedule = ScrollViewScheduleDetail.Children.FirstOrDefault(x =>
                            x.AutomationId == schedule.ID);
                        if (viewSchedule != null)
                        {
                            var equipment = _observableIrrigation.EquipmentList.First(x => x.ID == schedule.id_Pump);
                            var viewScheduleStatus = (ViewScheduleSettingSummary)viewSchedule;
                            viewScheduleStatus.Schedule.NAME = schedule.NAME;
                            viewScheduleStatus.Schedule.TIME = schedule.TIME;
                            viewScheduleStatus.Schedule.isActive = schedule.isActive;
                            viewScheduleStatus._equipment.NAME = equipment.NAME;
                            viewScheduleStatus.Populate();
                        }
                        else
                        {
                            var viewScheduleSettingSummary = new ViewScheduleSettingSummary(schedule,
                                _observableIrrigation.EquipmentList.First(x => x.ID == schedule.id_Pump));
                            ScrollViewScheduleDetail.Children.Add(viewScheduleSettingSummary);
                            viewScheduleSettingSummary.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                            viewScheduleSettingSummary.GetTapGestureRecognizer().Tapped += ViewScheduleScreen_Tapped;
                        }
                    }
                else
                {
                    ScrollViewScheduleDetail.Children.Add(new ViewEmptySchedule("No Schedules Here"));
                }
            }
            catch
            {
                // ignored
            }
        }

        private void ScreenCleanupForSchedules()
        {

            //CleanUp :)
            try
            {
                if (!_observableIrrigation.ScheduleList.Contains(null))
                {

                    var itemsThatAreOnDisplay = _observableIrrigation.ScheduleList.Select(x => x.ID).ToList();
                    if (itemsThatAreOnDisplay.Count == 0)
                        itemsThatAreOnDisplay.Add(new ViewEmptySchedule(string.Empty).ID);


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
            catch
            {
                // ignored
            }
        }

        private void GetScheduleSummary(string id)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    _floatingScreen.SetFloatingScreen(GetScheduleSummaryObject(_observableIrrigation.ScheduleList.FirstOrDefault(x => x.ID == id)));
                }
                catch
                {
                    var scheduleSummaryListObject = new List<object> { new ViewException() };
                    _floatingScreen.SetFloatingScreen(scheduleSummaryListObject);
                }
            });
            ;
        }

        private List<object> GetScheduleSummaryObject(Schedule schedule)
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
            catch
            {
                scheduleSummaryListObject = new List<object> { new ViewException() };
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
            var schedule = _observableIrrigation.ScheduleList.First(x => x.ID == edit.AutomationId);
            Navigation.PushModalAsync(new ScheduleUpdate(_observableIrrigation.EquipmentList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)).ToList(), schedule));
        }

        private void DeleteButton_Tapped(object sender, EventArgs e)
        {
            var delete = (Button)sender;
            var schedule = _observableIrrigation.ScheduleList.First(x => x.ID == delete.AutomationId);
            var deleteConfirm = new ViewDeleteConfirmation(schedule);
            _floatingScreen.SetFloatingScreen(new List<object> { deleteConfirm });
            deleteConfirm.GetDeleteButton().Clicked += DeleteConfirmButton_Tapped;
        }

        private void DeleteConfirmButton_Tapped(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            var delete = (Button)sender;
            new Authentication().DeleteSchedule(new Schedule { ID = delete.AutomationId });
        }

        private void ButtonCreateSchedule_OnClicked(object sender, EventArgs e)
        {
            if (_observableIrrigation.EquipmentList.Count > 0)
                Navigation.PushModalAsync(new ScheduleUpdate(_observableIrrigation.EquipmentList.Where(x => _observableIrrigation.SiteList.First(y => y.ID == _pumpConnection.SiteSelectedId).Attachments.Contains(x.ID)).ToList()));
            else
                DisplayAlert("Cannot Create a Schedule",
                    "You are missing the equipment that is needed to create a schedule", "Understood");
        }

        private void ScheduleSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            var scheduleSwitch = (Switch)sender;
            try
            {
                var updateSchedule = _observableIrrigation.ScheduleList.First(x => x.ID == scheduleSwitch.AutomationId);

                if (scheduleSwitch.IsToggled)
                    updateSchedule.isActive = "1";
                
                else
                    updateSchedule.isActive = "0";

                new Thread(() => ChangeScheduleState(updateSchedule))
                    .Start();
            }
            catch
            {
                DisplayAlert("Warning!!!", "This switch failed to parse it's ID \n COULD NOT CHANGE SCHEDULE STATE",
                    "Understood");
            }
        }

        private void ChangeScheduleState(Schedule _schedule)
        {
            var viewScheduleScreen = ScrollViewScheduleDetail.Children.First(x => (((ViewScheduleSettingSummary)x).Schedule.ID == _schedule.ID));
            var viewSchedule = (ViewScheduleSettingSummary) viewScheduleScreen;
            
            viewSchedule.Schedule = _schedule;

            Device.BeginInvokeOnMainThread(() =>
            {
                viewSchedule.GetSwitch().Toggled -= ScheduleSwitch_Toggled;
                viewSchedule.Populate();
                viewSchedule.GetSwitch().Toggled += ScheduleSwitch_Toggled;
            });
            
            //TODO Needs Confirmation that The Pi got it and its running :)
            var key = Task.Run(() => new Authentication().SetSchedule(_schedule)).Result;

        }
    }
}