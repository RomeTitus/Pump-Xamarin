using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    public partial class ScheduleHomeScreen
    {
        private readonly FloatingScreen _floatingScreen = new FloatingScreen();

        private readonly KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation>
            _observableFilterKeyValuePair;

        private readonly SocketPicker _socketPicker;
        private ViewScheduleSummary _viewSchedule;

        public ScheduleHomeScreen(
            KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation> observableFilterKeyValuePair,
            SocketPicker socketPicker)
        {
            InitializeComponent();
            _observableFilterKeyValuePair = observableFilterKeyValuePair;
            _socketPicker = socketPicker;
            _observableFilterKeyValuePair.Value.EquipmentList.CollectionChanged += PopulateScheduleStatusEvent;
            _observableFilterKeyValuePair.Value.ScheduleList.CollectionChanged += PopulateScheduleStatusEvent;
            PopulateScheduleStatus();
        }

        private void PopulateScheduleStatusEvent(object sender, NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateScheduleStatus);
        }

        private void PopulateScheduleStatus()
        {
            ScreenCleanupForSchedules();
            try
            {
                if (!_observableFilterKeyValuePair.Value.LoadedData) return;
                
                foreach (var schedule in _observableFilterKeyValuePair.Value.ScheduleList)
                {
                    var viewSchedule = ScrollViewScheduleDetail.Children.FirstOrDefault(x =>
                        x?.AutomationId == schedule.Id);
                    if (viewSchedule != null)
                    {
                        var viewScheduleStatus = (ViewSchedule)viewSchedule;
                        viewScheduleStatus.GetSwitch().Toggled -= ScheduleSwitch_Toggled;
                        viewScheduleStatus.Populate(schedule);
                        viewScheduleStatus.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                    }
                    else
                    {
                        var viewScheduleSettingSummary = new ViewSchedule(schedule,
                            _observableFilterKeyValuePair.Value.EquipmentList.FirstOrDefault(x =>
                                x?.Id == schedule.id_Pump));
                        ScrollViewScheduleDetail.Children.Add(viewScheduleSettingSummary);
                        viewScheduleSettingSummary.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                        viewScheduleSettingSummary.GetTapGestureRecognizer().Tapped += ViewScheduleScreen_Tapped;
                    }
                }
            
            
                if (ScrollViewScheduleDetail.Children.Count == 0)
                    ScrollViewScheduleDetail.Children.Add(new ViewEmptySchedule("No Custom Schedules Here"));
            }
            catch (Exception e)
            {
                ScrollViewScheduleDetail.Children.Add(new ViewException(e));
            }
        }

        private void ScreenCleanupForSchedules()
        {
            if (_observableFilterKeyValuePair.Value.LoadedData)
            {
                var itemsThatAreOnDisplay = _observableFilterKeyValuePair.Value.ScheduleList
                    .Select(x => x?.Id).ToList();
                
                if (itemsThatAreOnDisplay.Count == 0)
                    itemsThatAreOnDisplay.Add(new ViewEmptySchedule().AutomationId);
                ScrollViewScheduleDetail.RemoveUnusedViews(itemsThatAreOnDisplay);
            }
            else
            {
                ScrollViewScheduleDetail.DisplayActivityLoading();
            }
        }

        private void GetScheduleSummary(string id)
        {
            try
            {
                _floatingScreen.SetFloatingScreen(
                    GetScheduleSummaryObject(
                        _observableFilterKeyValuePair.Value.ScheduleList.FirstOrDefault(x => x?.Id == id)));
            }
            catch (Exception e)
            {
                var scheduleSummaryListObject = new List<object> { new ViewException(e) };
                _floatingScreen.SetFloatingScreen(scheduleSummaryListObject);
            }
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


                _viewSchedule =
                    new ViewScheduleSummary(schedule, _observableFilterKeyValuePair.Value.EquipmentList.ToList());

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

        private void ViewScheduleScreen_Tapped(object sender, EventArgs e)
        {
            if(PopupNavigation.Instance.PopupStack.FirstOrDefault(x => x.GetType() == _floatingScreen.GetType()) != null)
                return;
            var scheduleSwitch = ((View)sender).Parent;
            PopupNavigation.Instance.PushAsync(_floatingScreen);
            GetScheduleSummary(scheduleSwitch.AutomationId);
        }

        public void AddLoadingScreenFromId(string id)
        {
            var viewCustomSchedule = (ViewSchedule)
                ScrollViewScheduleDetail.Children.FirstOrDefault(x => x.AutomationId == id);
            viewCustomSchedule?.AddStatusActivityIndicator();
        }

        private void EditButton_Tapped(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            if (Navigation.ModalStack.Any(x => x.GetType() == typeof(ScheduleUpdate)))
                return;
            var edit = (Button)sender;
            var schedule = _observableFilterKeyValuePair.Value.ScheduleList.First(x => x?.Id == edit.AutomationId);
            Navigation.PushModalAsync(new ScheduleUpdate(_observableFilterKeyValuePair, _socketPicker, this,
                schedule));
        }

        private void DeleteButton_Tapped(object sender, EventArgs e)
        {
            var delete = (Button)sender;
            var schedule = _observableFilterKeyValuePair.Value.ScheduleList.First(x => x?.Id == delete.AutomationId);
            var deleteConfirm = new ViewDeleteConfirmation(schedule);
            _floatingScreen.SetFloatingScreen(new List<object> { deleteConfirm });
            deleteConfirm.GetDeleteButton().Clicked += DeleteConfirmButton_Tapped;
        }

        private async void DeleteConfirmButton_Tapped(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
            var delete = (Button)sender;
            var schedule = _observableFilterKeyValuePair.Value.ScheduleList.First(x => x?.Id == delete.AutomationId);
            schedule.DeleteAwaiting = true;
            var viewCustomSchedule = (ViewSchedule)
                ScrollViewScheduleDetail.Children.FirstOrDefault(x => x.AutomationId == schedule.Id);
            viewCustomSchedule?.AddStatusActivityIndicator();
            await _socketPicker.SendCommand(schedule, _observableFilterKeyValuePair.Key);
        }

        private async void ButtonCreateSchedule_OnClicked(object sender, EventArgs e)
        {
            if (Navigation.ModalStack.Any(x => x.GetType() == typeof(ScheduleUpdate)))
                return;
            if (_observableFilterKeyValuePair.Value.EquipmentList.Count > 0)
                await Navigation.PushModalAsync(new ScheduleUpdate(_observableFilterKeyValuePair,
                    _socketPicker, this));
            else
                await Application.Current.MainPage.DisplayAlert("Cannot Create a Schedule",
                    "You are missing the equipment that is needed to create a schedule", "Understood");
        }

        private async void ScheduleSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            var scheduleSwitch = (Switch)sender;
            try
            {
                var updateSchedule =
                    _observableFilterKeyValuePair.Value.ScheduleList.First(x => x?.Id == scheduleSwitch.Parent.Parent.Parent.AutomationId);

                updateSchedule.isActive = scheduleSwitch.IsToggled ? "1" : "0";

                await ChangeScheduleState(updateSchedule);
            }
            catch
            {
                await Application.Current.MainPage.DisplayAlert("Warning!!!",
                    "This switch failed to parse it's ID \n COULD NOT CHANGE SCHEDULE STATE",
                    "Understood");
            }
        }

        private async Task ChangeScheduleState(IrrigationController.Schedule schedule)
        {
            var viewScheduleScreen =
                ScrollViewScheduleDetail.Children.First(x =>
                    ((ViewSchedule)x).AutomationId == schedule.Id);
            
            var viewSchedule = (ViewSchedule)viewScheduleScreen;
            
            
            async void Action()
            {
                viewSchedule.GetSwitch().Toggled -= ScheduleSwitch_Toggled;
                viewSchedule.Populate(schedule);
                viewSchedule.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                viewSchedule.AddStatusActivityIndicator();
                await _socketPicker.SendCommand(schedule, _observableFilterKeyValuePair.Key);
            }

            Device.BeginInvokeOnMainThread(Action);
            
            
        }
    }
}