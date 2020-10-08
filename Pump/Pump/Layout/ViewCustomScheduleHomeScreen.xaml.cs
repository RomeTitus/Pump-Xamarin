using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database.Streaming;
using Newtonsoft.Json.Linq;
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
    public partial class ViewCustomScheduleHomeScreen : ContentPage
    {
        private List<Equipment> _equipmentList = null;
        private List<CustomSchedule> _customSchedulesList = null;
        private List<CustomSchedule> _oldCustomSchedulesList = null;
        readonly FloatingScreen _floatingScreen = new FloatingScreen();

        public ViewCustomScheduleHomeScreen()
        {
            InitializeComponent();
            new Thread(GetScheduleAndEquipmentFirebase).Start();
        }

        private void GetScheduleAndEquipmentFirebase()
        {
            var auth = new Authentication();


            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/CustomSchedule")
                .AsObservable<CustomSchedule>()
                .Subscribe(x =>
                {
                    if(_customSchedulesList == null)
                        _customSchedulesList = new List<CustomSchedule>();
                    _customSchedulesList.RemoveAll(y => y.ID == x.Key);
                    if (x.EventType != FirebaseEventType.Delete)
                    {
                        x.Object.ID = x.Key;
                        _customSchedulesList.Add(x.Object);
                    }
                        
                });


            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Equipment")
                .AsObservable<Equipment>()
                .Subscribe(x =>
                {
                    if (_equipmentList == null)
                        _equipmentList = new List<Equipment>();
                    _equipmentList.RemoveAll(y => y.ID == x.Key);
                    if (x.EventType != FirebaseEventType.Delete)
                    {
                        x.Object.ID = x.Key;
                        _equipmentList.Add(x.Object);
                    }
                        
                    _equipmentList = _equipmentList.OrderBy(equip => Convert.ToInt16(equip.GPIO)).ToList();
                });

            var databaseController = new DatabaseController();
            while (databaseController.IsRealtimeFirebaseSelected())
            {
                try
                {
                    
                    if (_equipmentList != null && _customSchedulesList != null && (_oldCustomSchedulesList == null || (!_customSchedulesList.All(_oldCustomSchedulesList.Contains) || _customSchedulesList.Count < _oldCustomSchedulesList.Count)))
                    {
                        if (_oldCustomSchedulesList == null)
                            _oldCustomSchedulesList = new List<CustomSchedule>();
                        var a = _customSchedulesList.All(_oldCustomSchedulesList.Contains) && _customSchedulesList.Count == _oldCustomSchedulesList.Count;
                        _oldCustomSchedulesList.Clear();
                        foreach (var customSchedules in _customSchedulesList)
                        {
                            _oldCustomSchedulesList.Add(customSchedules);
                        }

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            ScrollViewCustomScheduleDetail.Children.Clear();
                            var allScheduleList = GetCustomScheduleObject();
                            foreach (View view in allScheduleList) ScrollViewCustomScheduleDetail.Children.Add(view);
                        });
                    }
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ScrollViewCustomScheduleDetail.Children.Clear();
                        ScrollViewCustomScheduleDetail.Children.Add(new ViewNoConnection());
                    });
                }
                Thread.Sleep(2000);
            }

        }

        private List<object> GetCustomScheduleObject()
        {
            
            var scheduleListObject = new List<object>();
            try
            {
                if (_customSchedulesList.Count == 0)
                {
                    scheduleListObject.Add(new ViewEmptySchedule("No Custom Schedules Made"));
                    return scheduleListObject;
                }


                
                foreach (var viewSchedule in _customSchedulesList.Select(schedule => new ViewCustomSchedule(schedule, _equipmentList)))
                {
                    scheduleListObject.Add(viewSchedule);
                    viewSchedule.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                    viewSchedule.GetTapGestureRecognizer().Tapped += ViewScheduleScreen_Tapped;
                }
                return scheduleListObject;
            }
            catch
            {
                scheduleListObject = new List<object> { new ViewNoConnection() };
                return scheduleListObject;
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
                //new Thread(() => ChangeCustomScheduleState(scheduleSwitch, scheduleSwitch.AutomationId))
                //    .Start();
            }
            catch
            {
                DisplayAlert("Warning!!!", "This switch failed to parse it's ID \n COULD NOT CHANGE SCHEDULE STATE",
                    "Understood");
            }
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
                        var scheduleSummaryListObject = new List<object> { new ViewNoConnection() };
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

                ViewCustomScheduleSummary viewSchedule = null;
                viewSchedule = new ViewCustomScheduleSummary(schedule, _floatingScreen, _equipmentList);

                viewSchedule.GetButtonEdit().Clicked += EditButton_Tapped;
                viewSchedule.GetButtonDelete().Clicked += DeleteButton_Tapped;
                customScheduleSummaryListObject.Add(viewSchedule);
                var zoneAndTimeTapGesture = viewSchedule.GetZoneAndTimeGestureRecognizers();
                for (int i = 0; i < zoneAndTimeTapGesture.Count; i++)
                {
                    zoneAndTimeTapGesture[i].Tapped += SkipCustomSchedule_Tapped;
                }
                

                return customScheduleSummaryListObject;
            }
            catch
            {
                customScheduleSummaryListObject = new List<object> { new ViewNoConnection() };
                return customScheduleSummaryListObject;
            }
        }

        private void ButtonCreateCustomSchedule_OnClicked(object sender, EventArgs e)
        {

            if (new DatabaseController().IsRealtimeFirebaseSelected())
            {
                if (_equipmentList.Count > 0)
                    Navigation.PushModalAsync(new UpdateCustomSchedule(_equipmentList));
                else
                    DisplayAlert("Cannot Create a Schedule",
                        "You are missing the equipment that is needed to create a schedule", "Understood");
            }
        }

        private void EditButton_Tapped(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
            var edit = (Button) sender;
            var customSchedule = _customSchedulesList.First(schedule => schedule.ID == edit.AutomationId);
            Navigation.PushModalAsync(new UpdateCustomSchedule(_equipmentList, customSchedule));
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
            var equipment = _equipmentList.FirstOrDefault(x => x.ID == gridEquipmentAndTime.AutomationId);
            if(equipment == null)
                return;
            Device.BeginInvokeOnMainThread(() => { DisplayAlert("Are you sure?", "You have selected " + equipment.NAME +"\nConfirm to skip to this zone ?", "Confirm", "cancel"); });

        }
    }
}