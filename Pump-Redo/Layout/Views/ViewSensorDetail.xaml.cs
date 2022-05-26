using System;
using System.Globalization;
using System.Reflection;
using EmbeddedImages;
using Pump.Class;
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
            HeightRequest = 150 * size;
            LabelSensorType.FontSize *= size;
            LabelSensorName.FontSize *= size;
            PopulateSensor();
        }

        public void PopulateSensor()
        {
            LabelSensorType.Text = Sensor.TYPE;
            LabelSensorName.Text = Sensor.NAME;
            if (Sensor.LastUpdated != null)
                LabelSensorLastUpdated.Text = ScheduleTime.FromUnixTimeStampUtc(Sensor.LastUpdated.Value).ToLocalTime().ToString("dd/MM/yyyy HH:mm")
                    .ToString(CultureInfo.InvariantCulture);
            switch (Sensor.TYPE)
            {
                case "Pressure Sensor":
                    PressureSensor();
                    break;
                case "Temperature Sensor":
                    TemperatureSensor();
                    break;
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
            var reading = Convert.ToDouble(Sensor.LastReading, CultureInfo.InvariantCulture);

            var voltage = reading * 5.0 / 1024.0;

            var pressurePascal = 3.0 * (voltage - 0.47) * 1000000.0;

            var bars = pressurePascal / 10e5;

            try
            {
                if (bars < 2)
                {
                    LableSensorStatus.Text = bars.ToString("0.##");
                    _image = "Pump-Redo.Icons.PressureLow.png";
                }
                else
                {
                    LableSensorStatus.Text = bars.ToString("0.##");
                    _image = "Pump-Redo.Icons.PressureHigh.png";
                }
            }
            catch
            {
                LableSensorStatus.Text = "Could Not Read Pressure :/";
                _image = "Pump-Redo.Icons.PressureLow.png";
            }
        }

        private void TemperatureSensor()
        {
            try
            {
                var reading = Convert.ToDouble(Sensor.LastReading, CultureInfo.InvariantCulture);

                _image = reading > 8 ? "Pump-Redo.Icons.Temp_High.png" : "Pump-Redo.Icons.Temp_Low.png";
                LableSensorStatus.Text = Sensor.LastReading + "°C";
            }
            catch
            {
                LableSensorStatus.Text = "Could Not Read Temperature :/";
                _image = "Pump-Redo.Icons.Temp_Unknown.png";
            }
        }
    }
}