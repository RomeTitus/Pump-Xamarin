using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewScheduleSummary : ContentView
    {
        private readonly IReadOnlyList<string> _scheduleDetail;

        private readonly FloatingScreen _floatingScreen;
        public ViewScheduleSummary(IReadOnlyList<string> schedule, FloatingScreen floatingScreen)
        {
            InitializeComponent();
            this._floatingScreen = floatingScreen;
            this._scheduleDetail = schedule;
            foreach (var week in schedule[0].Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList())
            {
                SetWeek(week);
            }

            labelScheduleTime.Text = schedule[1];
            LabelPumpName.Text = schedule[2];
            labelScheduleName.Text = schedule[5];

            for (int i = 6; i < schedule.Count; i++)
            {
                ScrollViewZoneDetail.Children.Add(new ViewZoneAndTimeGrid(schedule[i].Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList(), true));
            }

            
        }

        private void SetWeek(string week)
        {
            if (week.Contains("SUNDAY"))
            {
                LabelSunday.TextColor = Color.Black;
                LabelSunday.Font = Font.SystemFontOfSize(24)
                    .WithAttributes(FontAttributes.Bold);
            }

            if (week.Contains("MONDAY"))
            {
                LabelMonday.TextColor = Color.Black;
                LabelMonday.Font = Font.SystemFontOfSize(24)
                    .WithAttributes(FontAttributes.Bold);
            }
                
            if (week.Contains("TUESDAY"))
            {
                LabelTuesday.TextColor = Color.Black;
                LabelTuesday.Font = Font.SystemFontOfSize(24)
                    .WithAttributes(FontAttributes.Bold);
            }
            
            if (week.Contains("WEDNESDAY"))
            {
                LabelWednesday.TextColor = Color.Black;
                LabelWednesday.Font = Font.SystemFontOfSize(24)
                    .WithAttributes(FontAttributes.Bold);
            }
            if (week.Contains("THURSDAY"))
            {
                LabelThursday.TextColor = Color.Black;
                LabelThursday.Font = Font.SystemFontOfSize(24)
                    .WithAttributes(FontAttributes.Bold);
            }
            
            if (week.Contains("FRIDAY"))
            {
                LabelFriday.TextColor = Color.Black;
                LabelFriday.Font = Font.SystemFontOfSize(24)
                    .WithAttributes(FontAttributes.Bold);
            }
            
            if (week.Contains("SATURDAY"))
            {
                LabelSaturday.TextColor = Color.Black;
                LabelSaturday.Font = Font.SystemFontOfSize(24)
                    .WithAttributes(FontAttributes.Bold);
            }
                
        }


        private void ButtonEdit_OnClicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new UpdateSchedule(_scheduleDetail));
            PopupNavigation.Instance.PopAsync();
        }

        private void ButtonDelete_OnClicked(object sender, EventArgs e)
        {
            _floatingScreen.SetFloatingScreen(new List<object> { new ViewDeleteConfirmation(_scheduleDetail) });
        }
    }
}