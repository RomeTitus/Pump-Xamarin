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
            stackLayoutBluetoothDeviceSummary.AutomationId = bluetoothDevice.Id.ToString();
            AutomationId = bluetoothDevice.Id.ToString();
            Populate();
            setImage();
        }

        public void Populate()
        {
            if (!string.IsNullOrEmpty(BluetoothDevice.Name))
                LabelBluetoothDeviceName.Text = BluetoothDevice.Name;
            LabelRSSI.Text = "RSSI: " + BluetoothDevice.Rssi;
        }

        private void setImage()
        {
            if (!string.IsNullOrEmpty(BluetoothDevice.Name)) return;
//            LabelBluetoothDeviceName.Text = "Unknown";
            LabelBluetoothDeviceName.Text = BluetoothDevice.NativeDevice.ToString();

            BlueToothDeviceImage.Source = ImageSource.FromResource(
                "Pump.Icons.NoConnection.png",
                typeof(ImageResourceExtention).GetTypeInfo().Assembly);
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return StackLayoutViewSubControllerTapGesture;
        }
    }
}