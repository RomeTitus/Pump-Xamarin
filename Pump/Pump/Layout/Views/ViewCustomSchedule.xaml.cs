using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewCustomSchedule : ContentView
    {
        private readonly CustomSchedule schedule;
        private List<Equipment> equipmentList;
        public ViewCustomSchedule(CustomSchedule schedule, List<Equipment> equipmentList)
        {
            InitializeComponent();
            this.schedule = schedule;
            this.equipmentList = equipmentList;
            Populate();
        }


        private void Populate()
        {
            var scheduleTime = new ScheduleTime();
            if (schedule.StartTime != 0)
            {
                var timeSpent = DateTime.Now - scheduleTime.FromUnixTimeStamp(schedule.StartTime); 
                labelScheduleTime.Text = "Time Spent: " + scheduleTime.convertDateTimeToString(timeSpent);
            }
            labelScheduleName.Text = schedule.NAME;
            switchScheduleIsActive.AutomationId = schedule.ID;
            foreach (var equipment in equipmentList.Where(equipment => equipment.ID == schedule.id_Pump))
            {
                LabelPumpName.Text = equipment.NAME;
            }
            StackLayoutViewSchedule.AutomationId = schedule.ID;
            switchScheduleIsActive.IsToggled = !schedule.isActive.Contains("0");
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