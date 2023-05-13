using System;
using System.Timers;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewActiveScheduleSummary : ContentView
    {
        private Timer _timer;
        public ActiveSchedule ActiveSchedule;

        public ViewActiveScheduleSummary(ActiveSchedule activeSchedule, double? size = null)
        {
            InitializeComponent();
            AutomationId = activeSchedule.Id;
            this.ActiveSchedule = activeSchedule;
            if (size != null)
            {
                HeightRequest = 150 * size.Value * 0.7;
                LabelScheduleName.FontSize *= size.Value;
                LabelPump.FontSize *= size.Value;
                LabelZone.FontSize *= size.Value;
                LabelStartTime.FontSize *= size.Value * 0.7;
                LabelEndTime.FontSize *= size.Value * 0.7;
            }

            PopulateSchedule();
        }

        public void PopulateSchedule()
        {
            LabelScheduleName.Text = ActiveSchedule.Weekday.Substring(0,3) + ": " + ActiveSchedule.Name;
            LabelPump.Text = ActiveSchedule.NamePump;
            LabelZone.Text = ActiveSchedule.NameEquipment;
            
            var startTime = ActiveSchedule.StartTime.TimeOfDay;

            LabelStartTime.Text = "Start Time: \n" + startTime;
            timer_Elapsed(null, null);
            StartEvent();
        }

        private void StartEvent()
        {
            _timer = new Timer(1000); // 1 seconds
            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = true;
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                string duration;
                if (DateTime.Now >= ActiveSchedule.StartTime)
                {
                    var span = ActiveSchedule.EndTime - DateTime.Now;
                    if (ActiveSchedule.TimeAdjustment is not null)
                        ImageTimeWarning.IsVisible = true;

                    duration = $"Time left: \n{span:hh\\:mm\\:ss}";
                    LabelEndTime.Text = duration;
                }
                else
                {
                    var span = ActiveSchedule.EndTime - ActiveSchedule.StartTime;
                    if (ActiveSchedule.TimeAdjustment is not null)
                        ImageTimeWarning.IsVisible = true;

                    duration = $"Duration: \n{span:hh\\:mm\\:ss}";
                    LabelEndTime.Text = duration;
                }
            });
        }
    }
}