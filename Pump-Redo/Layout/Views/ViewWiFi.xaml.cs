using System.Reflection;
using EmbeddedImages;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewWiFi : ContentView
    {
        private readonly WiFiContainer _wiFiContainer;

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
            if (_wiFiContainer.encryption_type.Contains("Encryption: on"))
                LabelEncryption.Text = "Encrypted";
            var signal = int.Parse(_wiFiContainer.signal.Replace(" dBm", "").Replace("-", ""));
            SetSignalStrength(SignalImage, signal);
        }

        public TapGestureRecognizer GetGestureRecognizer()
        {
            return StackLayoutViewWiFiTapGesture;
        }

        private static void SetSignalStrength(Image image, int dBm)
        {
            string signalStrength;

            if (dBm < 50)
                signalStrength = "5";

            else if (dBm < 57)
                signalStrength = "4";

            else if (dBm < 64)
                signalStrength = "3";

            else if (dBm < 70)
                signalStrength = "3";

            else if (dBm < 80)
                signalStrength = "1";

            else
                signalStrength = "NoSignal";

            image.Source = ImageSource.FromResource(
                "Pump.Icons.Signal_" + signalStrength + ".png",
                typeof(ImageResourceExtension).GetTypeInfo().Assembly);
        }
    }
}