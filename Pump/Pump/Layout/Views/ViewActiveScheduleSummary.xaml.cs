using System.Globalization;
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
            AutomationId = activeSchedule.Id;
            ActiveSchedule = activeSchedule;
            PopulateSchedule();
        }
        public ViewActiveScheduleSummary(ActiveSchedule activeSchedule, double size)
        {
            InitializeComponent();
            AutomationId = activeSchedule.Id;
            ActiveSchedule = activeSchedule;
            HeightRequest = 150 * size * 0.7;
            LabelScheduleName.FontSize *= size;
            LablePump.FontSize *= size;
            LableZone.FontSize *= size;
            LableStartTime.FontSize *= size*0.7;
            LableEndTime.FontSize *= size * 0.7;
            PopulateSchedule();
        }

        public void PopulateSchedule()
        {
            LabelScheduleName.Text = ActiveSchedule.Name;
            LablePump.Text = ActiveSchedule.NamePump;
            LableZone.Text = ActiveSchedule.NameEquipment;
            LableStartTime.Text = ActiveSchedule.StartTime.ToString(CultureInfo.InvariantCulture);
            LableEndTime.Text = ActiveSchedule.EndTime.ToString(CultureInfo.InvariantCulture);
        }
    }
}