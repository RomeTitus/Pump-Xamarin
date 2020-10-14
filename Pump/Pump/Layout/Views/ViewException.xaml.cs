using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewException : ContentView
    {
        public ViewException()
        {
            ID = "-849";
            AutomationId = ID;
            InitializeComponent();
        }
        public string ID { get; private set; }
    }
}