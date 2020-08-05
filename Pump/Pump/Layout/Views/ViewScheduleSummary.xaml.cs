using System;
using System.Collections.Generic;
using System.Linq;
using Pump.IrrigationController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewScheduleSummary : ContentView
    {
        private readonly List<Equipment> _equipmentList = null;
        private readonly FloatingScreen _floatingScreen;
        private readonly IReadOnlyList<string> _scheduleDetail;

        public ViewScheduleSummary(IReadOnlyList<string> schedule, FloatingScreen floatingScreen)
        {
            InitializeComponent();
            _floatingScreen = floatingScreen;
            _scheduleDetail = schedule;
            SetScheduleSummary();
        }
        public ViewScheduleSummary(IReadOnlyList<string> schedule, FloatingScreen floatingScreen, List<Equipment> equipmentList)
        {
            InitializeComponent();
            _floatingScreen = floatingScreen;
            _scheduleDetail = schedule;
            _equipmentList = equipmentList;
            SetScheduleSummary();
        }

        private void SetScheduleSummary()
        {
            foreach (var week in _scheduleDetail[0].Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList())
                SetWeek(week);

            labelScheduleTime.Text = _scheduleDetail[1];
            LabelPumpName.Text = _scheduleDetail[2];
            labelScheduleName.Text = _scheduleDetail[5];

            for (var i = 6; i < _scheduleDetail.Count; i++)
                ScrollViewZoneDetail.Children.Add(
                    new ViewZoneAndTimeGrid(_scheduleDetail[i].Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList(),
                        true));
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
            Navigation.PushModalAsync(_equipmentList != null
                ? new UpdateSchedule(_scheduleDetail, _equipmentList)
                : new UpdateSchedule(_scheduleDetail));
            PopupNavigation.Instance.PopAsync();
        }

        private void ButtonDelete_OnClicked(object sender, EventArgs e)
        {
            _floatingScreen.SetFloatingScreen(new List<object> {new ViewDeleteConfirmation(_scheduleDetail)});
        }
    }
}