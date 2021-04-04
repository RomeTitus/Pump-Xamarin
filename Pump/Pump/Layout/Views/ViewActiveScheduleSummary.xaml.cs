using System;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewActiveScheduleSummary : ContentView
    {
        public ActiveSchedule ActiveSchedule;
        private System.Timers.Timer _timer;
        public ViewActiveScheduleSummary(ActiveSchedule activeSchedule, double? size = null)
        {
            InitializeComponent();
            AutomationId = activeSchedule.Id;
            ActiveSchedule = activeSchedule;
            if (size != null)
            {
                HeightRequest = 150 * size.Value * 0.7;
                LabelScheduleName.FontSize *= size.Value;
                LablePump.FontSize *= size.Value;
                LableZone.FontSize *= size.Value;
                LableStartTime.FontSize *= size.Value*0.7;
                LableEndTime.FontSize *= size.Value * 0.7;

            }
            PopulateSchedule();
        }

        public void PopulateSchedule()
        {
            LabelScheduleName.Text = ActiveSchedule.Name;
            LablePump.Text = ActiveSchedule.NamePump;
            LableZone.Text = ActiveSchedule.NameEquipment;

            var startTime = ActiveSchedule.StartTime.TimeOfDay;

            LableStartTime.Text = "Start Time: \n" + startTime;
            timer_Elapsed(null, null);
            StartEvent();
        }

        private void StartEvent()
        {
            _timer = new System.Timers.Timer(1000); // 1 seconds
            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = true;
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            
                string duration;
                if (DateTime.Now >= ActiveSchedule.StartTime)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        var span = ActiveSchedule.EndTime - DateTime.Now;
                        duration = $"Time left: \n{span:hh\\:mm\\:ss}";
                        LableEndTime.Text = duration;
                    });
                }
                else
                {
                    var span = ActiveSchedule.EndTime - ActiveSchedule.StartTime;
                    duration = $"Duration: \n{span:hh\\:mm\\:ss}";
                    LableEndTime.Text = duration;
                }   
                
        }
        
    }
}