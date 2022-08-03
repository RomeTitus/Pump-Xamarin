using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSubControllerSummary
    {
        public bool LoadedData;

        public ViewSubControllerSummary(string subControllerId)
        {
            InitializeComponent();
            AutomationId = subControllerId;
            LabelSubControllerName.IsVisible = false;
            LabelType.IsVisible = false;
            ActivityIndicatorMobileLoadingIndicator.IsVisible = true;
        }

        public ViewSubControllerSummary(SubController subController)
        {
            InitializeComponent();
            AutomationId = subController?.Id;
            Populate(subController);
        }

        public void Populate(SubController subController)
        {
            LoadedData = true;
            LabelSubControllerName.IsVisible = true;
            ActivityIndicatorMobileLoadingIndicator.IsVisible = false;

            if (subController == null)
            {
                LabelSubControllerName.Text = "Main Controller";
                return;
            }

            LabelType.IsVisible = true;

            LabelSubControllerName.Text = subController.Name;
            if (subController.UseLoRa)
                LabelType.Text = "Long Range";
            StackLayoutStatus.AddUpdateRemoveStatus(subController.ControllerStatus);
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return GridViewSubControllerTapGesture;
        }
        
        public void AddStatusActivityIndicator()
        {
            StackLayoutStatus.AddStatusActivityIndicator();
        }
    }
}