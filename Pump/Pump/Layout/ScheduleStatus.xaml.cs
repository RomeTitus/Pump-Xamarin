using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pump.Database;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ScheduleStatus : ContentPage
    {
        string oldActiveSchedule = null;
        string oldQueueActiveSchedule = null;
        string oldActiveSensorStatus = null;
        SocketCommands command = new SocketCommands();
        SocketMessage socket = new SocketMessage();
        public ScheduleStatus()
        {
            InitializeComponent();
            new Thread(() => ThreadController()).Start();


        }


        private void ThreadController()
        {
            var started = false;
            var databaseController = new DatabaseController();

            Thread scheduleDetail = null;
            Thread sensorStatus = null;
            Thread queueScheduleDetail = null;

            while (true)
            {
                
                if (started == false && databaseController.GetActivityStatus() != null && databaseController.GetActivityStatus().status)
                {
                    //Start the threads
                    scheduleDetail = new Thread(() => getScheduleDetail());
                    queueScheduleDetail = new Thread(() => getQueueScheduleDetail());
                    sensorStatus = new Thread(() => getSensorStatus());

                    scheduleDetail.Start();
                    queueScheduleDetail.Start();
                    sensorStatus.Start();
                    started = true;
                }

                if (scheduleDetail != null && sensorStatus != null && queueScheduleDetail != null)
                {
                    if (started == true && databaseController.GetActivityStatus() != null && databaseController.GetActivityStatus().status == false)
                    {
                        scheduleDetail.Abort();
                        queueScheduleDetail.Abort();
                        sensorStatus.Abort();
                        started = false;
                        //Stop the threads
                    }

                }
                Thread.Sleep(2000);
            }
        }

        private void getScheduleDetail()
        {
            bool running = true;
            while (running)
            {
                try
                {
                    string schedules = socket.Message(command.getActiveSchedule());
                    Device.BeginInvokeOnMainThread(() =>
                    {
                       
                        if (oldActiveSchedule == schedules)
                            return;

                        ScrollViewScheduleStatus.Children.Clear();
                        oldActiveSchedule = schedules;

                        var scheduleList = getScheduleDetailObject(schedules);
                        foreach (View view in scheduleList)
                        {
                            ScrollViewScheduleStatus.Children.Add(view);
                        }

                    });
                }catch (ThreadAbortException)
                {
                    running = false;
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ScrollViewScheduleStatus.Children.Clear();
                        ScrollViewScheduleStatus.Children.Add(new ViewNoConnection());
                    });

                }
                Thread.Sleep(15000);
            }
            
        }

        private List<object> getScheduleDetailObject(string schedules)
        {
            List<object> scheduleListObject = new List<object>();
                try
                {
                        if (schedules == "No Data" || schedules == "")
                        {
                            scheduleListObject.Add(new ViewEmptySchedule());
                            return scheduleListObject;
                        }


                        List<string> scheduleList = new List<string>();
                        if (schedules.Contains("$"))
                        {
                            List<string> schedulewithManual = schedules.Split('$').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                            if (schedulewithManual.Count > 1)
                            {
                                scheduleListObject.Add(new ViewManualSchedule(schedulewithManual[0].Split(',').ToList(), true));
                                scheduleList = schedulewithManual[(schedulewithManual.Count - 1)].Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                            }
                            else
                                scheduleListObject.Add(new ViewManualSchedule(schedulewithManual[0].Split(',').ToList(), false));
                        }
                        else
                            scheduleList = schedules.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                        foreach (var schedule in scheduleList)
                        {
                            scheduleListObject.Add(new ViewScheduleDetail(schedule.Split(',').ToList()));
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

        private void getQueueScheduleDetail()
        {
            bool running = true;
            while (running)
            {
                try
                {
                    string Queueschedules = socket.Message(command.getQueueSchedule());
                    
                    Device.BeginInvokeOnMainThread(() =>
                    {

                        if (oldQueueActiveSchedule == Queueschedules)
                            return;
                        ScrollViewQueueStatus.Children.Clear();
                        oldQueueActiveSchedule = Queueschedules;

                        var QueuescheduleList = getQueueScheduleDetailObject(Queueschedules);
                        foreach (View view in QueuescheduleList)
                        {
                            ScrollViewQueueStatus.Children.Add(view);
                        }

                    });
                }
                catch (ThreadAbortException)
                {
                    running = false;
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ScrollViewQueueStatus.Children.Clear();
                        ScrollViewQueueStatus.Children.Add(new ViewNoConnection());
                    });

                }
                Thread.Sleep(15000);
            }
        }

        private List<object> getQueueScheduleDetailObject(string Queueschedules)
        {
            List<object> QueuescheduleListObject = new List<object>();
            try
            {
                if (Queueschedules == "No Data" || Queueschedules == "")
                {
                    QueuescheduleListObject.Add(new ViewEmptySchedule());
                    return QueuescheduleListObject;
                }


                List<string> QueuescheduleList = new List<string>();
                QueuescheduleList = Queueschedules.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                foreach (var schedule in QueuescheduleList)
                {
                    QueuescheduleListObject.Add(new ViewScheduleDetail(schedule.Split(',').ToList()));
                }

                return QueuescheduleListObject;  
            }
            catch
            {
                QueuescheduleListObject.Add(new ViewNoConnection());
                return QueuescheduleListObject;
            }
               
        }

        private void getSensorStatus()
        {
            bool running = true;
            while (running)
            {
                try
                {
                    string ActiveSensorStatus = socket.Message(command.getActiveSensorStatus());

                    Device.BeginInvokeOnMainThread(() =>
                    {

                        if (oldActiveSensorStatus == ActiveSensorStatus)
                            return;
                        ScrollViewSensorStatus.Children.Clear();
                        oldActiveSensorStatus = ActiveSensorStatus;

                        var SensorListObject = getSensorStatusObject(ActiveSensorStatus);
                        foreach (View view in SensorListObject)
                        {
                            ScrollViewSensorStatus.Children.Add(view);
                        }

                    });
                }
                catch (ThreadAbortException)
                {
                    running = false;
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ScrollViewSensorStatus.Children.Clear();
                        ScrollViewSensorStatus.Children.Add(new ViewNoConnection());
                    });

                }
                Thread.Sleep(2000);
            }
        }

        private List<object> getSensorStatusObject(string ActiveSensorStatus)
        {
            List<object> SensorListObject = new List<object>();
            try
            {
                if (ActiveSensorStatus == "No Data" || ActiveSensorStatus == "")
                {
                    SensorListObject.Add(new ViewEmptySchedule());
                    return SensorListObject;
                }

                var SensorList = ActiveSensorStatus.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                foreach (var Sensor in SensorList)
                {
                    SensorListObject.Add(new ViewSensorDetail(Sensor.Split(',').ToList()));
                }

                return SensorListObject;
            }
            catch
            {
                SensorListObject.Add(new ViewNoConnection());
                return SensorListObject;
            }

        }

        private void ScrollViewScheduleStatusTap_Tapped(object sender, EventArgs e)
        {
            if (oldActiveSchedule == null)
                return;
            var FloatingScreen = new FloatingScreen();
            FloatingScreen.setFloatingScreen(getScheduleDetailObject(oldActiveSchedule));
            PopupNavigation.Instance.PushAsync(FloatingScreen);

        }

        private void ScrollViewQueueStatusTap_Tapped(object sender, EventArgs e)
        {
            if (oldQueueActiveSchedule == null)
                return;
            var FloatingScreen = new FloatingScreen();
            FloatingScreen.setFloatingScreen(getQueueScheduleDetailObject(oldQueueActiveSchedule));
            PopupNavigation.Instance.PushAsync(FloatingScreen);
        }

        private void ScrollViewSensorStatusTap_Tapped(object sender, EventArgs e)
        {
            if (oldActiveSensorStatus == null)
                return;
            var FloatingScreen = new FloatingScreen();
            FloatingScreen.setFloatingScreen(getSensorStatusObject(oldActiveSensorStatus));
            PopupNavigation.Instance.PushAsync(FloatingScreen);
        }
    }
}