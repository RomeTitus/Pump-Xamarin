using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EmbeddedImages;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewAttachedEquipment : ContentView
    {
        public Equipment _equipment;
        private Sensor _sensor;
        private AttachedSensor _attachedSensor;
        public ViewAttachedEquipment(Equipment equipment, Sensor sensor)
        {
            InitializeComponent();
            _equipment = equipment;
            _sensor = sensor;
            Populate();
        }

        private void Populate()
        {
            LabelEquipmentName.Text = _equipment.NAME;
            if (_equipment.isPump)
                EquipmentImage.Source = ImageSource.FromResource(
                    "Pump.Icons.activePump.png",
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly);
            var attachedSensor = _equipment.AttachedSensor.FirstOrDefault(x => x.ID == _sensor.ID);
            if (attachedSensor != null)
            {
                _attachedSensor = attachedSensor;
                EquipmentCheckBox.IsChecked = true;
                SensorThresholdLow.Text = attachedSensor.ThresholdLow.ToString().Replace(",", ".");
                SensorThresholdHigh.Text = attachedSensor.ThresholdHigh.ToString().Replace(",", ".");
                SensorThresholdTimer.Text = attachedSensor.ThresholdTimer.ToString().Replace(",", ".");
            }
                
        }

        public AttachedSensor GetAttachedSensorDetail()
        {
            if (!EquipmentCheckBox.IsChecked)
                return null;


            return new AttachedSensor
            {
                ID = _sensor.ID,
                ThresholdLow = Convert.ToDouble(SensorThresholdLow.Text.Replace(".", ",")),
                ThresholdHigh = Convert.ToDouble(SensorThresholdHigh.Text.Replace(".", ",")),
                ThresholdTimer = Convert.ToDouble(SensorThresholdTimer.Text.Replace(".", ","))
            };

            
        }

        private void EquipmentCheckBox_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var equipmentCheckBox = (CheckBox) sender;
            SensorDetail.IsVisible = equipmentCheckBox.IsChecked;
        }
    }
}