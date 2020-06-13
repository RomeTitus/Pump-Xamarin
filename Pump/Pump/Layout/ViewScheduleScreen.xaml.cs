using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewScheduleScreen : ContentPage
    {
        readonly SocketCommands _command = new SocketCommands();
        readonly SocketMessage _socket = new SocketMessage();
        public ViewScheduleScreen()
        {
            InitializeComponent();

            
            new Thread(GetSchedules).Start();
        }

        public void GetSchedules()
        {
                try
                {
                    string schedules = _socket.Message(_command.getSchedule());
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ScrollViewScheduleDetail.Children.Clear();
                        var scheduleList = GetScheduleObject(schedules);
                        foreach (View view in scheduleList)
                        {
                            ScrollViewScheduleDetail.Children.Add(view);
                        }
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
            List<object> scheduleListObject = new List<object>();
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




        private void ViewScheduleSummary(int id)
        {
            
            var floatingScreen = new FloatingScreen();
            PopupNavigation.Instance.PushAsync(floatingScreen);
            new Thread(() => GetScheduleSummary(id, floatingScreen)).Start();
            
        }

        public void GetScheduleSummary(int id, FloatingScreen floatingScreen)
        {
            try
            {
                string schedulesSummary = _socket.Message(_command.getScheduleInfo(id));
                var scheduleList = GetScheduleSummaryObject(schedulesSummary);
                Device.BeginInvokeOnMainThread(() =>
                {
                    try
                    { 
                        floatingScreen.setFloatingScreen(scheduleList);
                    }
                    catch
                    {
                        var scheduleSummaryListObject = new List<object> {new ViewNoConnection()};
                        floatingScreen.setFloatingScreen(scheduleSummaryListObject);
                    }

                });
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    var scheduleSummaryListObject = new List<object> {new ViewNoConnection()};
                    floatingScreen.setFloatingScreen(scheduleSummaryListObject);
                });
            }
        }

        private List<object> GetScheduleSummaryObject(string schedulesSummary)
        {

            List<object> scheduleSummaryListObject = new List<object>();
            try
            {
                if (schedulesSummary == "No Data" || schedulesSummary == "")
                {
                    scheduleSummaryListObject.Add(new ViewEmptySchedule("No Schedules Details Found"));
                    return scheduleSummaryListObject;
                }


                var scheduleList = schedulesSummary.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();


                var viewSchedule = new ViewScheduleSummary(scheduleList);
                scheduleSummaryListObject.Add(viewSchedule);


                return scheduleSummaryListObject;
            }
            catch
            {
                scheduleSummaryListObject = new List<object> { new ViewNoConnection() };
                return scheduleSummaryListObject;
            }
        }

        private void ViewScheduleScreen_Tapped(object sender, EventArgs e)
        {
            var scheduleSwitch = (View)sender;
            ViewScheduleSummary(Convert.ToInt32(scheduleSwitch.AutomationId));
        }

        private void ScheduleSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            var scheduleSwitch = (Switch)sender;
            try
            {
                new Thread(() => ChangeScheduleState(scheduleSwitch, Convert.ToInt32(scheduleSwitch.AutomationId))).Start();
                
            }
            catch
            {
                DisplayAlert("Warning!!!", "This switch failed to parse it's ID \n COULD NOT CHANGE SCHEDULE STATE", "Understood");
            }
            
        }

        private void ChangeScheduleState(Switch scheduleSwitch, int id)
        {
            try
            {
                var result = _socket.Message(_command.ChangeSchedule(id));
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
                    DisplayAlert("Warning!!!", "Failed to reach the controller \n COULD NOT CHANGE SCHEDULE STATE", "Understood");

                });
            }
            

        }

        private void ButtonCreateSchedule_OnClicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new UpdateSchedule());
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}