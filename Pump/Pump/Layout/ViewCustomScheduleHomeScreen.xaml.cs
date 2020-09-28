using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
                .AsObservable<JObject>()
                .Subscribe(x =>
                {
                    if(_customSchedulesList == null)
                        _customSchedulesList = new List<CustomSchedule>();
                    var schedule = auth.GetJsonCustomSchedulesToObjectList(x.Object, x.Key);
                    _customSchedulesList.RemoveAll(y => y.ID == schedule.ID);
                    _customSchedulesList.Add(schedule);
                });


            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Equipment")
                .AsObservable<JObject>()
                .Subscribe(x =>
                {
                    if (_equipmentList == null)
                        _equipmentList = new List<Equipment>();
                    var equipment = auth.GetJsonEquipmentToObjectList(x.Object, x.Key);
                    _equipmentList.RemoveAll(y => y.ID == equipment.ID);
                    _equipmentList.Add(equipment);
                });

            var databaseController = new DatabaseController();
            while (databaseController.IsRealtimeFirebaseSelected())
            {
                try
                {
                    
                    if (_equipmentList != null && _customSchedulesList != null && (_oldCustomSchedulesList == null || !_customSchedulesList.All(_oldCustomSchedulesList.Contains)))
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
            var floatingScreen = new FloatingScreen();
            PopupNavigation.Instance.PushAsync(floatingScreen);
            new Thread(() => GetScheduleSummary(id, floatingScreen)).Start();
        }
        private void GetScheduleSummary(string id, FloatingScreen floatingScreen)
        {
            if (new DatabaseController().IsRealtimeFirebaseSelected())
            {

                var schedule = _customSchedulesList.FirstOrDefault(x => x.ID == id);
                
                var scheduleList = GetCustomScheduleSummaryObject(schedule, floatingScreen);

                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    {
                        floatingScreen.SetFloatingScreen(scheduleList);
                    }
                    catch
                    {
                        var scheduleSummaryListObject = new List<object> { new ViewNoConnection() };
                        floatingScreen.SetFloatingScreen(scheduleSummaryListObject);
                    }
                });
            }
        }

        private List<object> GetCustomScheduleSummaryObject(CustomSchedule schedule, FloatingScreen floatingScreen)
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
                viewSchedule = new ViewCustomScheduleSummary(schedule, floatingScreen, _equipmentList);

                customScheduleSummaryListObject.Add(viewSchedule);


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
    }
}