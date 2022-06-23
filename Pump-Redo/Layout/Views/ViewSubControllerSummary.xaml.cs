using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSubControllerSummary : ContentView
    {
        public readonly SubController SubController;

        public ViewSubControllerSummary(SubController subController)
        {
            InitializeComponent();
            SubController = subController;
            stackLayoutSubControllerSummary.AutomationId = SubController.Id;
            AutomationId = SubController.Id;
            Populate();
        }

        public void Populate()
        {
            LabelSubControllerName.Text = SubController.NAME;
            if (SubController.UseLoRa)
                LabelType.Text = "Long Range";
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return StackLayoutViewSubControllerTapGesture;
        }
    }
}