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
        private DatabaseController databaseController = new DatabaseController();
        private List<Equipment> _equipmentList;
        private List<Equipment> _oldEquipmentList;
        private List<Schedule> _scheduleList;
        private List<Schedule> _oldScheduleList;

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
                            _equipmentList = new List<Equipment>();
                        var equipment = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            _equipmentList.RemoveAll(y => y.ID == x.Key);
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
                            _scheduleList = new List<Schedule>();
                        var schedule = x.Object;

                        if (x.EventType == FirebaseEventType.Delete)
                        {
                            _scheduleList.RemoveAll(y => y.ID == x.Key);
                        }
                        else
                        {
                            var existingSchedule = _scheduleList.FirstOrDefault(y => y.ID == x.Key);
                            if (existingSchedule != null)
                            {
                                FirebaseMerger.CopyValues(existingSchedule, schedule);
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


            PopulateScheduleSummary();
        }

        private void PopulateScheduleSummary()
        {
            while (databaseController.IsRealtimeFirebaseSelected())
            {
                try
                {

                    if (_equipmentList != null && _scheduleList != null && (_oldScheduleList == null || (!_scheduleList.All(_oldScheduleList.Contains) || _scheduleList.Count < _oldScheduleList.Count)))
                    {
                        if (_oldScheduleList == null)
                            _oldScheduleList = new List<Schedule>();
                        _oldScheduleList.Clear();
                        foreach (var schedule in _scheduleList)
                        {
                            _oldScheduleList.Add(schedule);
                        }

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            ScrollViewScheduleDetail.Children.Clear();
                            var allScheduleList = GetScheduleObject();
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


        private List<object> GetScheduleObject()
        {

            var scheduleListObject = new List<object>();
            try
            {
                if (_scheduleList.Count == 0)
                {
                    scheduleListObject.Add(new ViewEmptySchedule("No Schedules Made"));
                    return scheduleListObject;
                }

                foreach (var viewSchedule in _scheduleList.Select(schedule => new ViewScheduleSettingSummary(schedule, _equipmentList.First(x => x.ID == schedule.id_Pump))))
                {
                    scheduleListObject.Add(viewSchedule);
                    //viewSchedule.GetSwitch().Toggled += ScheduleSwitch_Toggled;
                    //viewSchedule.GetTapGestureRecognizer().Tapped += ViewScheduleScreen_Tapped;
                }
                return scheduleListObject;
            }
            catch
            {
                scheduleListObject = new List<object> { new ViewNoConnection() };
                return scheduleListObject;
            }
        }



        //Old Stuff



        /*
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
    
        */
    }
}