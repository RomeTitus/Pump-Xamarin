using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewManualSchedule : ContentView
    {
        public ViewManualSchedule(IReadOnlyList<string> manual, bool isManualWithSchedule)
        {
            InitializeComponent();
            setManualText(manual, isManualWithSchedule);
        }

        private void setManualText(IReadOnlyList<string> manual, bool isManualWithSchedule)
        {
            var scheduleTime = new ScheduleTime();
            LableManual.Text = isManualWithSchedule ? "Manual Running with Schedule" : "Manual Running without Schedule";

            LableManualTime.Text = "Duration: " + manual[0];
        }
    }
}