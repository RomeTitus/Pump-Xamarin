using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewEmptySchedule : ContentView
    {
        public ViewEmptySchedule(string text)
        {
            InitializeComponent();
            EmptyScheduleLabel.Text = text;
        }
    }
}