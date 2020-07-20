using System.Collections.Generic;
using System.Reflection;
using EmbeddedImages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSensorDetail : ContentView
    {
        public ViewSensorDetail(List<string> sensorList)
        {
            InitializeComponent();
            setSensorType(sensorList);
        }

        private void setSensorType(List<string> sensorList)
        {
            LableSensorType.Text = sensorList[1];
            LabelSensorName.Text = sensorList[2];
            var image = "";
            if (sensorList[1] == "Pressure Sensor")
            {
                sensorList[3] = sensorList[3].Replace('.', ',');
                try
                {
                    if (sensorList[3] == "False" || double.Parse(sensorList[3]) < 2)
                    {
                        LableSensorStatus.Text = sensorList[3];
                        image = "Pump.Icons.PressureLow.png";
                    }
                    else
                    {
                        LableSensorStatus.Text = sensorList[3];
                        image = "Pump.Icons.PressureHigh.png";
                    }
                }
                catch
                {
                    LableSensorStatus.Text = "Low Pressure";
                    image = "Pump.Icons.PressureLow.png";
                }
            }

            ImageSensor.Source = ImageSource.FromResource(
                image,
                typeof(ImageResourceExtention).GetTypeInfo().Assembly);
        }
    }
}