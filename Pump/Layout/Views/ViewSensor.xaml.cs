using System.Reflection;
using EmbeddedImages;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSensor
    {
        public ViewSensor(Sensor sensor)
        {
            InitializeComponent();
            AutomationId = sensor.Id;
            Populate(sensor);
        }

        public void Populate(Sensor sensor)
        {
            LabelSensorName.Text = sensor.NAME;
            
            LabelPin.Text = "Pin: " + sensor.GPIO;

            if (sensor.TYPE == "Pressure Sensor")
            {
                LabelPin.Text = "Pin: " + (sensor.GPIO - 37) + "A";
                SensorImage.Source = ImageSource.FromResource(
                        "Pump.Icons.PressureHigh.png",
                        typeof(ImageResourceExtension).GetTypeInfo().Assembly);

            }
            StackLayoutStatus.AddUpdateRemoveStatus(sensor.ControllerStatus);
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return GridViewSensorTapGesture;
        }
        
        public void AddStatusActivityIndicator()
        {
            StackLayoutStatus.AddStatusActivityIndicator();
        }
    }
}