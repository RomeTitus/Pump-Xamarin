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

            for (int i = 5; i < schedule.Count; i++)
            {
                SetWeek(schedule[i]);
            }
        }

        private void Populate(string id, string name, string time, string isActive, string pump)
        {
            labelScheduleName.Text = name;
            labelScheduleTime.Text = time;
            switchScheduleIsActive.AutomationId = id;
            LabelPumpName.Text = pump;
            StackLayoutViewSchedule.AutomationId = id;
            if (isActive.Contains("0"))
                switchScheduleIsActive.IsToggled = false;
            else
                switchScheduleIsActive.IsToggled = true;
        }

        private void SetWeek(string week)
        {
            if (week.Contains("SUNDAY"))
            {
                LabelSunday.TextColor = Color.Black;
                LabelSunday.Font = Font.SystemFontOfSize(12)
                    .WithAttributes(FontAttributes.Bold);
            }

            if (week.Contains("MONDAY"))
            {
                LabelMonday.TextColor = Color.Black;
                LabelMonday.Font = Font.SystemFontOfSize(12)
                    .WithAttributes(FontAttributes.Bold);
            }

            if (week.Contains("TUESDAY"))
            {
                LabelTuesday.TextColor = Color.Black;
                LabelTuesday.Font = Font.SystemFontOfSize(12)
                    .WithAttributes(FontAttributes.Bold);
            }

            if (week.Contains("WEDNESDAY"))
            {
                LabelWednesday.TextColor = Color.Black;
                LabelWednesday.Font = Font.SystemFontOfSize(12)
                    .WithAttributes(FontAttributes.Bold);
            }
            if (week.Contains("THURSDAY"))
            {
                LabelThursday.TextColor = Color.Black;
                LabelThursday.Font = Font.SystemFontOfSize(12)
                    .WithAttributes(FontAttributes.Bold);
            }

            if (week.Contains("FRIDAY"))
            {
                LabelFriday.TextColor = Color.Black;
                LabelFriday.Font = Font.SystemFontOfSize(12)
                    .WithAttributes(FontAttributes.Bold);
            }

            if (week.Contains("SATURDAY"))
            {
                LabelSaturday.TextColor = Color.Black;
                LabelSaturday.Font = Font.SystemFontOfSize(12)
                    .WithAttributes(FontAttributes.Bold);
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
    }
}