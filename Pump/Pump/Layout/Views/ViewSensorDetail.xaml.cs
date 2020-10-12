using System;
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
        public readonly Sensor Sensor;
        private string _image = "";
        private string _oldImage = "";
        public ViewSensorDetail(Sensor sensor)
        {
            InitializeComponent();
            Sensor = sensor;
            AutomationId = Sensor.ID;
            PopulateSensor();
        }
        public ViewSensorDetail(Sensor sensor, double size)
        {
            InitializeComponent();
            Sensor = sensor;
            AutomationId = Sensor.ID;
            this.HeightRequest = 150 * size;
            LableSensorType.FontSize *= size;
            LabelSensorName.FontSize *= size;
            PopulateSensor();
        }

        public void PopulateSensor()
        {
            LableSensorType.Text = Sensor.TYPE;
            LabelSensorName.Text = Sensor.NAME;
            
            if (Sensor.TYPE == "Pressure Sensor")
            {
                PressureSensor();
            }

            if (_oldImage != _image)
            {
                _oldImage = _image;
                ImageSensor.Source = ImageSource.FromResource(
                    _image,
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly);
            }
            
        }

        private void PressureSensor()
        {
            var reading = Convert.ToDouble(Sensor.LastReading);
            
            var voltage = reading * 5.0 / 1024.0;

            var pressure_pascal = 3.0 * (voltage - 0.47) * 1000000.0;

            var bars = pressure_pascal / 10e5;

            try
            {
                if (bars < 2)
                {
                    LableSensorStatus.Text = bars.ToString("0.##");
                    _image = "Pump.Icons.PressureLow.png";
                }
                else
                {
                    LableSensorStatus.Text = bars.ToString("0.##");
                    _image = "Pump.Icons.PressureHigh.png";
                }
            }
            catch
            {
                LableSensorStatus.Text = "Could Not Read Pressure :/";
                _image = "Pump.Icons.PressureLow.png";
            }
        }
    }
}