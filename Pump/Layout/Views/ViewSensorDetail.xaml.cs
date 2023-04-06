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
    public partial class ViewSensorDetail
    {
        public readonly Sensor Sensor;
        private string _image = "";
        private string _oldImage = "";

        public ViewSensorDetail(Sensor sensor)
        {
            InitializeComponent();
            Sensor = sensor;
            AutomationId = Sensor.Id;
            Populate();
        }

        public ViewSensorDetail(Sensor sensor, double size)
        {
            InitializeComponent();
            Sensor = sensor;
            AutomationId = Sensor.Id;
            HeightRequest = 150 * size;
            LabelSensorType.FontSize *= size;
            LabelSensorName.FontSize *= size;
            Populate();
        }

        public void Populate()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                LabelSensorType.Text = Sensor.TYPE;
                LabelSensorName.Text = Sensor.NAME;
                if (Sensor.LastUpdated != null)
                LabelSensorLastUpdated.Text = ScheduleTime.FromUnixTimeStampUtc(Sensor.LastUpdated.Value).ToLocalTime()
                    .ToString("dd/MM/yyyy HH:mm")
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
                        typeof(ImageResourceExtension).GetTypeInfo().Assembly);
                }
            });
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

        private void TemperatureSensor()
        {
            try
            {
                var reading = Convert.ToDouble(Sensor.LastReading, CultureInfo.InvariantCulture);

                _image = reading > 8 ? "Pump.Icons.Temp_High.png" : "Pump.Icons.Temp_Low.png";
                LableSensorStatus.Text = Sensor.LastReading + "°C";
            }
            catch
            {
                LableSensorStatus.Text = "Could Not Read Temperature :/";
                _image = "Pump.Icons.Temp_Unknown.png";
            }
        }
    }
}