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
        public ViewCustomSchedule(CustomSchedule schedule, Equipment equipment)
        {
            InitializeComponent();
            AutomationId = schedule.Id;
            Schedule = schedule;
            Equipment = equipment;
            Populate(schedule);
        }

        public CustomSchedule Schedule { get; set; }
        private Equipment Equipment { get; }


        public void Populate(CustomSchedule schedule)
        {
            var endTime = new RunningCustomSchedule().getCustomScheduleEndTime(Schedule);
            
            SwitchScheduleIsActive.AutomationId ??= Schedule.Id;
            StackLayoutViewSchedule.AutomationId ??= Schedule.Id;
            
            LabelScheduleRepeat.Text = "Repeat: " + Schedule.Repeat;
            if (endTime != null)
            {
                if (RunningCustomSchedule.GetCustomScheduleDetailRunning(Schedule) != null)
                {
                    var timeLeft = (TimeSpan)(endTime - DateTime.UtcNow);
                    LabelScheduleTime.Text = "Time left: " + ScheduleTime.ConvertTimeSpanToString(timeLeft);
                    SwitchScheduleIsActive.IsToggled = true;
                }
                else
                {
                    var timeLeft = (TimeSpan)(endTime - ScheduleTime.FromUnixTimeStampLocal(Schedule.StartTime));
                    LabelScheduleTime.Text = "Duration: " + ScheduleTime.ConvertTimeSpanToString(timeLeft);
                    SwitchScheduleIsActive.IsToggled = false;
                }
            }

            LabelScheduleName.Text = Schedule.NAME;

            if (Equipment != null)
                LabelPumpName.Text = Equipment.NAME;
            StackLayoutStatus.AddUpdateRemoveStatus(schedule.ControllerStatus);

        }

        public void AddStatusActivityIndicator()
        {
            StackLayoutStatus.AddStatusActivityIndicator();
        }

        public Switch GetSwitch()
        {
            return SwitchScheduleIsActive;
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return StackLayoutViewScheduleTapGesture;
        }

        private void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            SwitchScheduleIsActive.IsToggled = !SwitchScheduleIsActive.IsToggled;
        }
    }
}