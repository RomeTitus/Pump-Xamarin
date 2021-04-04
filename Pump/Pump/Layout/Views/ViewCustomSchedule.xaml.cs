using System;
using Pump.Class;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewCustomSchedule : ContentView
    {
        public CustomSchedule Schedule { get; set; }
        public Equipment Equipment { get;}
        public ViewCustomSchedule(CustomSchedule schedule, Equipment equipment)
        {
            InitializeComponent();
            AutomationId = schedule.ID;
            Schedule = schedule;
            Equipment = equipment;
            Populate();
        }


        public void Populate()
        {
            var endTime = new RunningCustomSchedule().getCustomScheduleEndTime(Schedule);
            if(switchScheduleIsActive.AutomationId == null)
                switchScheduleIsActive.AutomationId = Schedule.ID;
            if (StackLayoutViewSchedule.AutomationId == null)
                StackLayoutViewSchedule.AutomationId = Schedule.ID;
            LabelScheduleRepeat.Text = "Repeat: " + Schedule.Repeat;
            if (endTime != null)
            {
                if (RunningCustomSchedule.GetCustomScheduleDetailRunning(Schedule) != null)
                {
                    var timeLeft = (TimeSpan) (endTime - DateTime.UtcNow);
                    LabelScheduleTime.Text = "Time left: " + ScheduleTime.ConvertTimeSpanToString(timeLeft);
                    switchScheduleIsActive.IsToggled = true;
                }
                else
                {
                    var timeLeft = (TimeSpan) (endTime - ScheduleTime.FromUnixTimeStampLocal(Schedule.StartTime));
                    LabelScheduleTime.Text = "Duration: " + ScheduleTime.ConvertTimeSpanToString(timeLeft);
                    switchScheduleIsActive.IsToggled = false;
                }
            }

            labelScheduleName.Text = Schedule.NAME;
            
            if(Equipment != null)
                LabelPumpName.Text = Equipment.NAME;
            

        }

        public Switch GetSwitch()
        {
            return switchScheduleIsActive;
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return StackLayoutViewScheduleTapGesture;
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            switchScheduleIsActive.IsToggled = !switchScheduleIsActive.IsToggled;
        }

       
    }
}