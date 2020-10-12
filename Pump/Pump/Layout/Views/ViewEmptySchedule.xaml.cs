using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewEmptySchedule : ContentView
    {
        public ViewEmptySchedule(string text)
        {
            ID = "-849";
            AutomationId = ID;
            InitializeComponent();
            EmptyScheduleLabel.Text = text;
        }

        public string ID { get; private set; }
    }
}