using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewWiFi : ContentView
    {
        private WiFiContainer _wiFiContainer;

        public ViewWiFi(WiFiContainer wiFiContainer)
        {
            InitializeComponent();
            _wiFiContainer = wiFiContainer;
            Populate();
            StackLayoutViewWiFi.AutomationId = _wiFiContainer.ssid;
        }

        private void Populate()
        {
            LabelSsid.Text = _wiFiContainer.ssid;
            LabelEncryption.Text = "Encryption: " + _wiFiContainer.encryption_type;
            LabelSignal.Text = "Signal: " + _wiFiContainer.signal;
        }

        public TapGestureRecognizer GetGestureRecognizer()
        {
            return StackLayoutViewWiFiTapGesture;
        }
    }
}