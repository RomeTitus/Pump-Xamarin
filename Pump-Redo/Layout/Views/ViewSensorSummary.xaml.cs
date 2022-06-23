using System.Reflection;
using EmbeddedImages;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSensorSummary : ContentView
    {
        public readonly Sensor _sensor;

        public ViewSensorSummary(Sensor sensor)
        {
            InitializeComponent();
            _sensor = sensor;
            stackLayoutSensorSummary.AutomationId = _sensor.Id;
            Populate();
        }

        public void Populate()
        {
            LabelSensorName.Text = _sensor.NAME;
            LabelPin.Text = "Pin: " + _sensor.GPIO;
            if (_sensor.TYPE == "Pressure Sensor")
                SensorImage.Source = ImageSource.FromResource(
                    "Pump.Icons.PressureHigh.png",
                    typeof(ImageResourceExtension).GetTypeInfo().Assembly);
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return StackLayoutViewSensorTapGesture;
        }
    }
}