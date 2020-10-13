using System;
using Pump.Class;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewManualSchedule : ContentView
    {
        private readonly ManualSchedule _manualSchedule;
        private static System.Timers.Timer _timer;
        public ViewManualSchedule(ManualSchedule manual)
        {
            InitializeComponent();
            _manualSchedule = manual;
            AutomationId = _manualSchedule.ID;
            SetManualText();
            SetUpTimerEvent();
        }

        private void SetUpTimerEvent()
        {
            _timer = new System.Timers.Timer {AutoReset = false};
            _timer.Elapsed += (timer_Elapsed);
            _timer.Interval = GetInterval();
            _timer.Start();
        }

        private void SetManualText()
        {
            LableManual.Text = _manualSchedule.RunWithSchedule ? "Manual Running with Schedule" : "Manual Running without Schedule";

            LableManualTime.Text = "Duration: " + ScheduleTime.ConvertTimeSpanToString(ScheduleTime.FromUnixTimeStampUtc(_manualSchedule.EndTime) - DateTime.UtcNow);
        }

        private static double GetInterval()
        {
            var now = DateTime.Now;
            return ((60 - now.Second) * 1000 - now.Millisecond);
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            LableManualTime.Text = "Duration: " + ScheduleTime.ConvertTimeSpanToString(ScheduleTime.FromUnixTimeStampUtc(_manualSchedule.EndTime) - DateTime.UtcNow);
            _timer.Interval = GetInterval();
            _timer.Start();
        }
    }
}