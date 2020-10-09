using System;
using System.Collections.Generic;
using System.Linq;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewCustomSchedule : ContentView
    {
        private readonly CustomSchedule _schedule;
        private readonly List<Equipment> _equipmentList;
        public ViewCustomSchedule(CustomSchedule schedule, List<Equipment> equipmentList)
        {
            InitializeComponent();
            _schedule = schedule;
            _equipmentList = equipmentList;
            Populate();
        }


        private void Populate()
        {
            var scheduleTime = new ScheduleTime();
            var runningCustomSchedule = new RunningCustomSchedule();
            var endTime = new RunningCustomSchedule().getCustomScheduleEndTime(_schedule);
            switchScheduleIsActive.AutomationId = _schedule.ID;
            StackLayoutViewSchedule.AutomationId = _schedule.ID;
            if (endTime != null)
            {
                if (runningCustomSchedule.getCustomScheduleDetailRunning(_schedule) != null)
                {
                    var timeLeft = (TimeSpan) (endTime - DateTime.Now);
                    LabelScheduleTime.Text = "Time left: " + scheduleTime.convertDateTimeToString(timeLeft);
                    switchScheduleIsActive.IsToggled = true;
                }
                else
                {
                    var timeLeft = (TimeSpan) (endTime - scheduleTime.FromUnixTimeStamp(_schedule.StartTime));
                    LabelScheduleTime.Text = "Duration: " + scheduleTime.convertDateTimeToString(timeLeft);
                    switchScheduleIsActive.IsToggled = false;
                }
            }

            labelScheduleName.Text = _schedule.NAME;
            
            foreach (var equipment in _equipmentList.Where(equipment => equipment.ID == _schedule.id_Pump))
            {
                LabelPumpName.Text = equipment.NAME;
            }

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