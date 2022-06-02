using System.Reflection;
using EmbeddedImages;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewBluetoothSummary : ContentView
    {
        public IDevice BluetoothDevice;

        public ViewBluetoothSummary(IDevice bluetoothDevice)
        {
            InitializeComponent();
            BluetoothDevice = bluetoothDevice;
            StackLayoutBluetoothDeviceSummary.AutomationId = bluetoothDevice.Id.ToString();
            AutomationId = bluetoothDevice.Id.ToString();
            Populate();
            SetImage();
        }

        public void Populate()
        {
            if (!string.IsNullOrEmpty(BluetoothDevice.Name))
                LabelBluetoothDeviceName.Text = BluetoothDevice.Name;
            //LabelRSSI.Text = "RSSI: " + BluetoothDevice.Rssi;
        }

        private void SetImage()
        {
            var signal = int.Parse(BluetoothDevice.Rssi.ToString().Replace("-", ""));
            SetSignalStrength(SignalImage, signal);
            
            if (!string.IsNullOrEmpty(BluetoothDevice.Name)) return;
            LabelBluetoothDeviceName.Text = BluetoothDevice.NativeDevice.ToString();

            BlueToothDeviceImage.Source = ImageSource.FromResource(
                "Pump.Icons.NoConnection.png",
                typeof(ImageResourceExtension).GetTypeInfo().Assembly);
        }
        
        private static void SetSignalStrength(Image image, int dBm)
        {
            string signalStrength;
                    
            if (dBm < 50)
                signalStrength = "5";

            else if (dBm < 57 )
                signalStrength = "4";
                    
            else if (dBm < 62 )
                signalStrength = "3";
                    
            else if (dBm < 67 )
                signalStrength = "3";
                    
            else if (dBm < 70 )
                signalStrength = "1";

            else
                signalStrength = "NoSignal";

            image.Source = ImageSource.FromResource(
                "Pump.Icons.Signal_" + signalStrength + ".png",
                typeof(ImageResourceExtension).GetTypeInfo().Assembly);
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return StackLayoutViewSubControllerTapGesture;
        }
    }
}