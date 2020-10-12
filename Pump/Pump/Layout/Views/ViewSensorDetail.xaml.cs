using System.Collections.Generic;
using System.Reflection;
using EmbeddedImages;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSensorDetail : ContentView
    {
        public Sensor Sensor;
        public ViewSensorDetail(Sensor sensor)
        {
            InitializeComponent();
            Sensor = sensor;
            AutomationId = Sensor.ID;
            PopulateSensor();
        }

        public void PopulateSensor()
        {
            LableSensorType.Text = Sensor.TYPE;
            LabelSensorName.Text = Sensor.NAME;
            var image = "";
            if (Sensor.TYPE == "Pressure Sensor")
            {
                Sensor.LastReading = Sensor.LastReading.Replace('.', ',');
                try
                {
                    if (double.Parse(Sensor.LastReading) < 2)
                    {
                        LableSensorStatus.Text = Sensor.LastReading;
                        image = "Pump.Icons.PressureLow.png";
                    }
                    else
                    {
                        LableSensorStatus.Text = Sensor.LastReading;
                        image = "Pump.Icons.PressureHigh.png";
                    }
                }
                catch
                {
                    LableSensorStatus.Text = "Could Not Read Pressure :/";
                    image = "Pump.Icons.PressureLow.png";
                }
            }

            ImageSensor.Source = ImageSource.FromResource(
                image,
                typeof(ImageResourceExtention).GetTypeInfo().Assembly);
        }
    }
}