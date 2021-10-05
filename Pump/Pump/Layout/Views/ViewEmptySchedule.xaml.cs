using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewEmptySchedule : ContentView
    {
        public ViewEmptySchedule(string text = "", double? size = null)
        {
            InitializeComponent();
            if (size != null)
            {
                EmptyScheduleLabel.FontSize *= size.Value;
                HeightRequest = 150 * size.Value;
            }

            EmptyScheduleLabel.Text = text;
            Populate();
        }

        private void Populate()
        {
            AutomationId = "-849";
        }
    }
}