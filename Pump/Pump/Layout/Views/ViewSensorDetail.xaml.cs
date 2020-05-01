using EmbeddedImages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            string image = "";
            if (sensorList[1] == "Pressure Sensor")
            {
                
                if (sensorList[3] == ("0") || sensorList[3] == ("False"))
                {
                    LableSensorStatus.Text = "Low Pressure";
                    image = "Pump.Icons.PressureLow.png";
                    
                }
                else
                {
                    LableSensorStatus.Text = "High Pressure";
                    image = "Pump.Icons.PressureHigh.png";
                }
                
            }
            ImageSensor.Source = ImageSource.FromResource(
                    image,
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly);
        }
    }
}