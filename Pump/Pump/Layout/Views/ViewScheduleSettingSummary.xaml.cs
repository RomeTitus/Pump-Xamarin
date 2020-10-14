using System;
using System.Collections.Generic;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewScheduleSettingSummary : ContentView
    {
        public readonly Schedule _schedule;
        public readonly Equipment _equipment;
        public ViewScheduleSettingSummary(Schedule schedule, Equipment equipment)
        {
            InitializeComponent();
            _schedule = schedule;
            _equipment = equipment;
            switchScheduleIsActive.AutomationId = _schedule.ID;
            StackLayoutViewSchedule.AutomationId = _schedule.ID;
            Populate();
        }

        public void Populate()
        {
            labelScheduleName.Text = _schedule.NAME;
            labelScheduleTime.Text = _schedule.TIME;
            
            LabelPumpName.Text = _equipment.NAME;
            
            switchScheduleIsActive.IsToggled = !_schedule.isActive.Contains("0");
            SetWeek();
        }

        private void SetWeek()
        {
            
            if (_schedule.WEEK.Contains("SUNDAY"))
            {
                LabelSunday.TextColor = Color.Black;
                LabelSunday.FontAttributes = FontAttributes.Bold;
                LabelSunday.FontSize = 12;
            }

            if (_schedule.WEEK.Contains("MONDAY"))
            {
                LabelMonday.TextColor = Color.Black;
                LabelMonday.FontAttributes = FontAttributes.Bold;
                LabelMonday.FontSize = 12;
            }

            if (_schedule.WEEK.Contains("TUESDAY"))
            {
                LabelTuesday.TextColor = Color.Black;
                LabelTuesday.FontAttributes = FontAttributes.Bold;
                LabelTuesday.FontSize = 12;
            }

            if (_schedule.WEEK.Contains("WEDNESDAY"))
            {
                LabelWednesday.TextColor = Color.Black;
                LabelWednesday.FontAttributes = FontAttributes.Bold;
                LabelWednesday.FontSize = 12;
            }

            if (_schedule.WEEK.Contains("THURSDAY"))
            {
                LabelThursday.TextColor = Color.Black;
                LabelThursday.FontAttributes = FontAttributes.Bold;
                LabelThursday.FontSize = 12;
            }

            if (_schedule.WEEK.Contains("FRIDAY"))
            {
                LabelFriday.TextColor = Color.Black;
                LabelFriday.FontAttributes = FontAttributes.Bold;
                LabelFriday.FontSize = 12;
            }

            if (_schedule.WEEK.Contains("SATURDAY"))
            {
                LabelSaturday.TextColor = Color.Black;
                LabelSaturday.FontAttributes = FontAttributes.Bold;
                LabelSaturday.FontSize = 12;
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

        private void SwitchTapGestureRecognizer(object sender, EventArgs e)
        {
            switchScheduleIsActive.IsToggled = !switchScheduleIsActive.IsToggled;
        }
    }
}