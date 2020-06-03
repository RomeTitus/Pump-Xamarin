using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pump.Layout.Views;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewScheduleScreen : ContentPage
    {
        SocketCommands command = new SocketCommands();
        SocketMessage socket = new SocketMessage();
        public ViewScheduleScreen()
        {
            InitializeComponent();

            
            new Thread(GetSchedules).Start();
        }

        public void GetSchedules()
        {
                try
                {
                    string schedules = socket.Message(command.getSchedule());
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ScrollViewScheduleDetail.Children.Clear();
                        var scheduleList = getScheduleObject(schedules);
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

        private List<object> getScheduleObject(string schedules)
        {
            List<object> scheduleListObject = new List<object>();
            try
            {
                if (schedules == "No Data" || schedules == "")
                {
                    scheduleListObject.Add(new ViewEmptySchedule("No Schedules Made"));
                    return scheduleListObject;
                }


                List<string> scheduleList = new List<string>();
                
                scheduleList = schedules.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                foreach (var schedule in scheduleList)
                {
                    scheduleListObject.Add(new ViewSchedule(schedule.Split(',').ToList()));
                }
                return scheduleListObject;
            }
            catch
            {
                scheduleListObject = new List<object>();
                scheduleListObject.Add(new ViewNoConnection());
                return scheduleListObject;
            }
        }

        private void ButtonCreateSchedule_OnClicked(object sender, EventArgs e)
        {
            
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}