using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using Pump.Class;
using Pump.Database.Table;
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
    public partial class CustomScheduleHomeScreen
    {
        private readonly FloatingScreen _floatingScreen = new FloatingScreen();

        private readonly KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation>
            _observableFilterKeyValuePair;

        private readonly SocketPicker _socketPicker;
        private ViewCustomScheduleSummary _viewSchedule;

        public CustomScheduleHomeScreen(
            KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation> observableFilterKeyValuePair,
            SocketPicker socketPicker)
        {
            InitializeComponent();
            _observableFilterKeyValuePair = observableFilterKeyValuePair;
            _socketPicker = socketPicker;
            _observableFilterKeyValuePair.Value.EquipmentList.CollectionChanged += PopulateCustomScheduleStatusEvent;
            _observableFilterKeyValuePair.Value.CustomScheduleList.CollectionChanged += PopulateCustomScheduleStatusEvent;
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
                if (!_observableFilterKeyValuePair.Value.LoadedData) return;
                
                foreach (var customSchedule in _observableFilterKeyValuePair.Value.CustomScheduleList)
                {
                    var viewSchedule = ScrollViewCustomScheduleDetail.Children.FirstOrDefault(x =>
                        x?.AutomationId == customSchedule.Id);
                    if (viewSchedule != null)
                    {
                        var viewScheduleStatus = (ViewCustomSchedule)viewSchedule;
                        viewScheduleStatus.Populate(customSchedule);
                    }
                    else
                    {
                        var viewScheduleSettingSummary = new ViewCustomSchedule(customSchedule,
                            _observableFilterKeyValuePair.Value.EquipmentList.FirstOrDefault(x =>
                                x?.Id == customSchedule.id_Pump));
                        ScrollViewCustomScheduleDetail.Children.Add(viewScheduleSettingSummary);
                        viewScheduleSettingSummary.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                        viewScheduleSettingSummary.GetTapGestureRecognizer().Tapped += ViewScheduleScreen_Tapped;
                    }
                }
            
            
                if (ScrollViewCustomScheduleDetail.Children.Count == 0)
                    ScrollViewCustomScheduleDetail.Children.Add(new ViewEmptySchedule("No Custom Schedules Here"));
        
            }
            catch (Exception e)
            {
                ScrollViewCustomScheduleDetail.Children.Add(new ViewException(e));
            }
        }

        private void ScreenCleanupForCustomSchedules()
        {
            if (_observableFilterKeyValuePair.Value.LoadedData)
            {
                var itemsThatAreOnDisplay = _observableFilterKeyValuePair.Value.CustomScheduleList
                    .Select(x => x?.Id).ToList();
                
                if (itemsThatAreOnDisplay.Count == 0)
                    itemsThatAreOnDisplay.Add(new ViewEmptySchedule().AutomationId);
                ScrollViewCustomScheduleDetail.RemoveUnusedViews(itemsThatAreOnDisplay);
            }
            else
            {
                ScrollViewCustomScheduleDetail.DisplayActivityLoading();
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
                    _observableFilterKeyValuePair.Value.CustomScheduleList.First(x =>
                        x?.Id == scheduleSwitch.AutomationId);

                updateSchedule.StartTime = scheduleSwitch.IsToggled ? ScheduleTime.GetUnixTimeStampUtcNow() : 0;

                ChangeCustomScheduleState(updateSchedule);
            }
            catch
            {
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
                    //viewCustomSchedule.Populate(schedule);
                    viewCustomSchedule.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                    
                    viewCustomSchedule.AddStatusActivityIndicator();
                    await _socketPicker.SendCommand(schedule, _observableFilterKeyValuePair.Key);
                }

                Device.BeginInvokeOnMainThread(Action);
            }
        }

        private void ViewScheduleSummary(string id)
        {
            if(PopupNavigation.Instance.PopupStack.FirstOrDefault(x => x.GetType() == _floatingScreen.GetType()) != null)
                return;
            PopupNavigation.Instance.PushAsync(_floatingScreen);
            new Thread(() => GetScheduleSummary(id)).Start();
        }

        private void GetScheduleSummary(string id)
        {
            var schedule = _observableFilterKeyValuePair.Value.CustomScheduleList.FirstOrDefault(x => x?.Id == id);
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


                _viewSchedule = new ViewCustomScheduleSummary(schedule,
                    _observableFilterKeyValuePair.Value.EquipmentList.ToList());

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
            if (Navigation.ModalStack.Any(x => x.GetType() == typeof(CustomScheduleUpdate)))
                return;
            if (_observableFilterKeyValuePair.Value.EquipmentList.Count > 0)
                Navigation.PushModalAsync(new CustomScheduleUpdate(_observableFilterKeyValuePair,
                    _socketPicker, this));
            else
                Application.Current.MainPage.DisplayAlert("Cannot Create a Custom Schedule",
                    "You are missing the equipment that is needed to create a custom schedule", "Understood");
        }
        
        
        public void AddLoadingScreenFromId(string id)
        {
            var viewCustomSchedule = (ViewCustomSchedule)
                ScrollViewCustomScheduleDetail.Children.FirstOrDefault(x => x.AutomationId == id);
            viewCustomSchedule?.AddStatusActivityIndicator();
        }

        private void EditButton_Tapped(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            if (Navigation.ModalStack.Any(x => x.GetType() == typeof(CustomScheduleUpdate)))
                return;
            var edit = (Button)sender;
            var customSchedule =
                _observableFilterKeyValuePair.Value.CustomScheduleList.First(schedule =>
                    schedule.Id == edit.AutomationId);
            Navigation.PushModalAsync(new CustomScheduleUpdate(_observableFilterKeyValuePair,
                _socketPicker, this, customSchedule));
        }

        private void DeleteButton_Tapped(object sender, EventArgs e)
        {
            var delete = (Button)sender;
            var customSchedule =
                _observableFilterKeyValuePair.Value.CustomScheduleList.First(schedule =>
                    schedule.Id == delete.AutomationId);
            var deleteConfirm = new ViewDeleteConfirmation(customSchedule);
            _floatingScreen.SetFloatingScreen(new List<object> { deleteConfirm });
            deleteConfirm.GetDeleteButton().Clicked += DeleteConfirmButton_Tapped;
        }

        private async void DeleteConfirmButton_Tapped(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
            var viewDeleteConfirm = (ViewDeleteConfirmation) ((Button)sender).Parent.Parent.Parent;

            var customSchedule = viewDeleteConfirm.GetCustomSchedule();
            customSchedule.DeleteAwaiting = true;
            
            var viewCustomSchedule = (ViewCustomSchedule)
                ScrollViewCustomScheduleDetail.Children.FirstOrDefault(x => x.AutomationId == customSchedule.Id);
            viewCustomSchedule?.AddStatusActivityIndicator();
            
            await _socketPicker.SendCommand(customSchedule,
                _observableFilterKeyValuePair.Key);
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
                    _observableFilterKeyValuePair.Value.EquipmentList
                        .First(x => x?.Id == selectedCustomScheduleDetails.id_Equipment)
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