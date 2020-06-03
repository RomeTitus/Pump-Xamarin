using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSchedule : ContentView
    {
        public ViewSchedule(List<string> schedule)
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
                LabelSunday.IsVisible = true;
            if (week.Contains("MONDAY"))
                LabelMonday.IsVisible = true;
            if (week.Contains("TUESDAY"))
                LabelTuesday.IsVisible = true;
            if (week.Contains("WEDNESDAY"))
                LabelWednesday.IsVisible = true;
            if (week.Contains("THURSDAY"))
                LabelThursday.IsVisible = true;
            if (week.Contains("FRIDAY"))
                LabelFriday.IsVisible = true;
            if (week.Contains("SATURDAY"))
                LabelSaturday.IsVisible = true;
        }
    }
}