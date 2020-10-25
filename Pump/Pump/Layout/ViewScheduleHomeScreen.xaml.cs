using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database.Streaming;
using Pump.Class;
using Pump.Database;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewScheduleHomeScreen : ContentPage
    {
        private ObservableCollection<Equipment> _equipmentList;
        private ObservableCollection<Schedule> _scheduleList;
        private readonly FloatingScreen _floatingScreen = new FloatingScreen();
        private ViewScheduleSummary _viewSchedule;

        public ViewScheduleHomeScreen()
        {
            InitializeComponent();

            new Thread(SubscribeToFirebase).Start();
        }

        private void SubscribeToFirebase()
        {
            var auth = new Authentication();

            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Equipment")
                .AsObservable<Equipment>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_equipmentList == null)
                            _equipmentList = new ObservableCollection<Equipment>();

                        var equipment = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _equipmentList.Count; i++)
                            {
                                if (_equipmentList[i].ID == x.Key)
                                    _equipmentList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingEquipment = _equipmentList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingEquipment != null)
                            {
                                FirebaseMerger.CopyValues(existingEquipment, equipment);
                            }
                            else
                            {
                                equipment.ID = x.Key;
                                _equipmentList.Add(equipment);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });

            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Schedule")
                .AsObservable<Schedule>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (_scheduleList == null)
                            _scheduleList = new ObservableCollection<Schedule>();
                        var schedule = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            for (int i = 0; i < _scheduleList.Count; i++)
                            {
                                if (_scheduleList[i].ID == x.Key)
                                    _scheduleList.RemoveAt(i);
                            }
                        }
                        else
                        {
                            var existingSchedule = _scheduleList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingSchedule != null)
                            {
                                FirebaseMerger.CopyValues(existingSchedule, schedule);
                                Device.InvokeOnMainThreadAsync(PopulateScheduleStatus);
                            }
                            else
                            {
                                schedule.ID = x.Key;
                                _scheduleList.Add(schedule);
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });


            LoadScheduleStatus();
        }

        private void LoadScheduleStatus()
        {
            var hasSubscribed = false;
            while (!hasSubscribed)
            {
                try
                {
                    if (_equipmentList != null && _scheduleList != null)
                    {
                        hasSubscribed = true;
                        _equipmentList.CollectionChanged += PopulateScheduleStatusEvent;
                        _scheduleList.CollectionChanged += PopulateScheduleStatusEvent;
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
                if (_scheduleList.Any())
                    foreach (var schedule in _scheduleList)
                    {
                        var viewSchedule = ScrollViewScheduleDetail.Children.FirstOrDefault(x =>
                            x.AutomationId == schedule.ID);
                        if (viewSchedule != null)
                        {
                            var equipment = _equipmentList.First(x => x.ID == schedule.id_Pump);
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
                                _equipmentList.First(x => x.ID == schedule.id_Pump));
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
                if (_scheduleList != null)
                {

                    var itemsThatAreOnDisplay = _scheduleList.Select(x => x.ID).ToList();
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
                    _floatingScreen.SetFloatingScreen(GetScheduleSummaryObject(_scheduleList.FirstOrDefault(x => x.ID == id)));
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


                _viewSchedule = new ViewScheduleSummary(schedule, _equipmentList.ToList());

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
            var schedule = _scheduleList.First(x => x.ID == edit.AutomationId);
            Navigation.PushModalAsync(new UpdateSchedule(_equipmentList.ToList(), schedule));
        }

        private void DeleteButton_Tapped(object sender, EventArgs e)
        {
            var delete = (Button)sender;
            var schedule = _scheduleList.First(x => x.ID == delete.AutomationId);
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
            if (new DatabaseController().IsRealtimeFirebaseSelected())
            {
                if (_equipmentList.Count > 0)
                    Navigation.PushModalAsync(new UpdateSchedule(_equipmentList.ToList()));
                else
                    DisplayAlert("Cannot Create a Schedule",
                        "You are missing the equipment that is needed to create a schedule", "Understood");
            }

        }

        private void ScheduleSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            var scheduleSwitch = (Switch)sender;
            try
            {
                var updateSchedule = _scheduleList.First(x => x.ID == scheduleSwitch.AutomationId);

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