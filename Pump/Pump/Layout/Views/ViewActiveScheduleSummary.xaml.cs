using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewActiveScheduleSummary : ContentView
    {
        public ActiveSchedule ActiveSchedule;
        public ViewActiveScheduleSummary(ActiveSchedule activeSchedule)
        {
            InitializeComponent();
            AutomationId = activeSchedule.ID;
            ActiveSchedule = activeSchedule;
            PopulateSchedule();
        }

        public void PopulateSchedule()
        {
            LabelScheduleName.Text = ActiveSchedule.NAME;
            LablePump.Text = ActiveSchedule.name_Pump;
            LableZone.Text = ActiveSchedule.name_Equipment;
            LableStartTime.Text = ActiveSchedule.StartTime.ToString(CultureInfo.InvariantCulture);
            LableEndTime.Text = ActiveSchedule.StartTime.ToString(CultureInfo.InvariantCulture);
        }
    }
}