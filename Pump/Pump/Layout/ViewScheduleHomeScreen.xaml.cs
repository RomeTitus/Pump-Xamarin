using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Database.Streaming;
using Newtonsoft.Json.Linq;
using Pump.Database;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Switch = Xamarin.Forms.Switch;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewScheduleHomeScreen : ContentPage
    {
        private List<Equipment> _equipmentList = new List<Equipment>();
        private readonly List<Schedule> _schedulesList = new List<Schedule>();
        private string _oldAllSchedule = "";
        private readonly SocketCommands _command = new SocketCommands();
        private readonly SocketMessage _socket = new SocketMessage();

        public ViewScheduleHomeScreen()
        {
            InitializeComponent();

            if (new DatabaseController().IsRealtimeFirebaseSelected())
                new Thread(() => GetScheduleAndEquipmentFirebase(new DatabaseController())).Start();
            else
                new Thread(GetSchedules).Start();
        }

        private void GetScheduleAndEquipmentFirebase(DatabaseController databaseController)
        {
            var auth = new Authentication();


            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Schedule")
                .AsObservable<JObject>()
                .Subscribe(x =>
                {
                    var schedule = auth.GetJsonSchedulesToObjectList(x.Object, x.Key);
                    _schedulesList.RemoveAll(y => y.ID == schedule.ID);
                    if (x.EventType != FirebaseEventType.Delete)
                        _schedulesList.Add(schedule);
                });


            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Equipment")
                .AsObservable<JObject>()
                .Subscribe(x =>
                {
                    try
                    {
                        var equipment = auth.GetJsonEquipmentToObjectList(x.Object, x.Key);
                        _equipmentList.RemoveAll(y => y.ID == equipment.ID);
                        if (x.EventType != FirebaseEventType.Delete)
                            _equipmentList.Add(equipment);
                        _equipmentList = _equipmentList.OrderBy(equip => Convert.ToInt16(equip.GPIO)).ToList();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                });

            
            while (databaseController.IsRealtimeFirebaseSelected())
            {
                try
                {
                    var allSchedule = "";
                    foreach (var schedule in _schedulesList)
                    {
                        var pumpEquipment = _equipmentList.First(x => x.ID == schedule.id_Pump);
                        allSchedule += schedule.ID + ',' + schedule.NAME + ',' + schedule.TIME + ',' +
                                       schedule.isActive + ',' + pumpEquipment.NAME + ',' + schedule.WEEK + '#';
                    }


                    if (_oldAllSchedule != allSchedule && string.IsNullOrWhiteSpace(_oldAllSchedule))
                    {
                        _oldAllSchedule = allSchedule;

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            ScrollViewScheduleDetail.Children.Clear();
                            var allScheduleList = GetScheduleObject(allSchedule);
                            foreach (View view in allScheduleList) ScrollViewScheduleDetail.Children.Add(view);
                        });
                    }
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ScrollViewScheduleDetail.Children.Clear();
                        ScrollViewScheduleDetail.Children.Add(new ViewNoConnection());
                    });
                }
                Thread.Sleep(2000);
            }

        }


        private void GetSchedules()
        {
            try
            {
                var schedules = _socket.Message(_command.getSchedule());
                Device.BeginInvokeOnMainThread(() =>
                {
                    ScrollViewScheduleDetail.Children.Clear();
                    var scheduleList = GetScheduleObject(schedules);
                    foreach (View view in scheduleList) ScrollViewScheduleDetail.Children.Add(view);
                });
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ScrollViewScheduleDetail.Children.Clear();
                    ScrollViewScheduleDetail.Children.Add(new ViewNoConnection());
                });

            }
        }

        private List<object> GetScheduleObject(string schedules)
        {
            var scheduleListObject = new List<object>();
            try
            {
                if (schedules == "No Data" || schedules == "")
                {
                    scheduleListObject.Add(new ViewEmptySchedule("No Schedules Made"));
                    return scheduleListObject;
                }


                var scheduleList = schedules.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                foreach (var schedule in scheduleList)
                {
                    var viewSchedule = new ViewSchedule(schedule.Split(',').ToList());
                    scheduleListObject.Add(viewSchedule);
                    viewSchedule.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                    viewSchedule.GetTapGestureRecognizer().Tapped += ViewScheduleScreen_Tapped;
                }

                return scheduleListObject;
            }
            catch
            {
                scheduleListObject = new List<object> {new ViewNoConnection()};
                return scheduleListObject;
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
                
                var schedule = _schedulesList.First(x => x.ID == id);
                var pump = _equipmentList.First(x => x.ID == schedule.id_Pump);
                var schedulesSummary = "";
                schedulesSummary += schedule.WEEK + '#' + schedule.TIME + '#' + pump.NAME + '#' + schedule.ID + '#' + pump.ID + '#'+ schedule.NAME;
                foreach (var scheduleDetail in schedule.ScheduleDetails)
                {
                    var equipment = _equipmentList.First(x => x.ID == scheduleDetail.id_Equipment);
                    schedulesSummary += '#' + scheduleDetail.id_Equipment + ',' + equipment.NAME + ',' + scheduleDetail.DURATION ;
                }

                var scheduleList = GetScheduleSummaryObject(schedulesSummary, floatingScreen);

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
            else
            {
                try
                {
                    var schedulesSummary = _socket.Message(_command.getScheduleInfo(id));
                    var scheduleList = GetScheduleSummaryObject(schedulesSummary, floatingScreen);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            floatingScreen.SetFloatingScreen(scheduleList);
                        }
                        catch
                        {
                            var scheduleSummaryListObject = new List<object> {new ViewNoConnection()};
                            floatingScreen.SetFloatingScreen(scheduleSummaryListObject);
                        }
                    });
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        var scheduleSummaryListObject = new List<object> {new ViewNoConnection()};
                        floatingScreen.SetFloatingScreen(scheduleSummaryListObject);
                    });
                }
            }
        }

        private List<object> GetScheduleSummaryObject(string schedulesSummary, FloatingScreen floatingScreen)
        {
            var scheduleSummaryListObject = new List<object>();
            try
            {
                if (schedulesSummary == "No Data" || schedulesSummary == "")
                {
                    scheduleSummaryListObject.Add(new ViewEmptySchedule("No Schedules Details Found"));
                    return scheduleSummaryListObject;
                }


                var scheduleList = schedulesSummary.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                
                ViewScheduleSummary viewSchedule = null;
                viewSchedule = new DatabaseController().IsRealtimeFirebaseSelected() ? new ViewScheduleSummary(scheduleList, floatingScreen, _equipmentList) : 
                    new ViewScheduleSummary(scheduleList, floatingScreen);
                
                scheduleSummaryListObject.Add(viewSchedule);


                return scheduleSummaryListObject;
            }
            catch
            {
                scheduleSummaryListObject = new List<object> {new ViewNoConnection()};
                return scheduleSummaryListObject;
            }
        }

        private void ViewScheduleScreen_Tapped(object sender, EventArgs e)
        {
            var scheduleSwitch = (View) sender;
            ViewScheduleSummary(scheduleSwitch.AutomationId);
        }

        private void ScheduleSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            var scheduleSwitch = (Switch) sender;
            try
            {
                new Thread(() => ChangeScheduleState(scheduleSwitch, scheduleSwitch.AutomationId))
                    .Start();
            }
            catch
            {
                DisplayAlert("Warning!!!", "This switch failed to parse it's ID \n COULD NOT CHANGE SCHEDULE STATE",
                    "Understood");
            }
        }

        private void ChangeScheduleState(Switch scheduleSwitch, string id)
        {
            if (new DatabaseController().IsRealtimeFirebaseSelected())
            {
                var schedule = _schedulesList.First(x => x.ID == id);
                schedule.isActive = Convert.ToInt32(scheduleSwitch.IsToggled).ToString();
                var key = Task.Run(() => new Authentication().SetSchedule(schedule)).Result;
                
            }
            else
            {
                try
                {
                    var result = _socket.Message(_command.ChangeSchedule(id, Convert.ToInt32(scheduleSwitch.IsToggled)));
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (result == "success")
                        {
                        }
                        else
                        {
                            DisplayAlert("Warning!!!", result, "Understood");
                        }
                    });
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        scheduleSwitch.Toggled -= ScheduleSwitch_Toggled;
                        scheduleSwitch.IsToggled = !scheduleSwitch.IsToggled;
                        scheduleSwitch.Toggled += ScheduleSwitch_Toggled;
                        DisplayAlert("Warning!!!", "Failed to reach the controller \n COULD NOT CHANGE SCHEDULE STATE",
                            "Understood");
                    });
                }

            }

        }

        private void ButtonCreateSchedule_OnClicked(object sender, EventArgs e)
        {
            if (new DatabaseController().IsRealtimeFirebaseSelected())
            {
                if (_equipmentList.Count > 0)
                    Navigation.PushModalAsync(new UpdateSchedule(_equipmentList));
                else
                    DisplayAlert("Cannot Create a Schedule", "You are missing the equipment that is needed to create a schedule", "Understood");
                
            }
            else
                Navigation.PushModalAsync(new UpdateSchedule());
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}