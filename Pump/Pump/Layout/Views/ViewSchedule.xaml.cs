using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSchedule : ContentView
    {
        public ViewSchedule(IReadOnlyList<string> schedule)
        {
            InitializeComponent();
            Populate(schedule[0], schedule[1], schedule[2], schedule[3], schedule[4]);

            for (var i = 5; i < schedule.Count; i++) SetWeek(schedule[i]);
        }

        private void Populate(string id, string name, string time, string isActive, string pump)
        {
            labelScheduleName.Text = name;
            labelScheduleTime.Text = time;
            switchScheduleIsActive.AutomationId = id;
            LabelPumpName.Text = pump;
            StackLayoutViewSchedule.AutomationId = id;
            switchScheduleIsActive.IsToggled = !isActive.Contains("0");
        }

        private void SetWeek(string week)
        {
            if (week.Contains("SUNDAY"))
            {
                LabelSunday.TextColor = Color.Black;
                LabelSunday.FontAttributes = FontAttributes.Bold;
                LabelSunday.FontSize = 12;
            }

            if (week.Contains("MONDAY"))
            {
                LabelMonday.TextColor = Color.Black;
                LabelMonday.FontAttributes = FontAttributes.Bold;
                LabelMonday.FontSize = 12;
            }

            if (week.Contains("TUESDAY"))
            {
                LabelTuesday.TextColor = Color.Black;
                LabelTuesday.FontAttributes = FontAttributes.Bold;
                LabelTuesday.FontSize = 12;
            }

            if (week.Contains("WEDNESDAY"))
            {
                LabelWednesday.TextColor = Color.Black;
                LabelWednesday.FontAttributes = FontAttributes.Bold;
                LabelWednesday.FontSize = 12;
            }

            if (week.Contains("THURSDAY"))
            {
                LabelThursday.TextColor = Color.Black;
                LabelThursday.FontAttributes = FontAttributes.Bold;
                LabelThursday.FontSize = 12;
            }

            if (week.Contains("FRIDAY"))
            {
                LabelFriday.TextColor = Color.Black;
                LabelFriday.FontAttributes = FontAttributes.Bold;
                LabelFriday.FontSize = 12;
            }

            if (week.Contains("SATURDAY"))
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