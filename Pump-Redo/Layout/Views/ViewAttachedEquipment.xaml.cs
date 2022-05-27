using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using EmbeddedImages;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewAttachedEquipment : ContentView
    {
        private readonly Sensor _sensor;
        public readonly Equipment Equipment;

        public ViewAttachedEquipment(Equipment equipment, Sensor sensor)
        {
            InitializeComponent();
            Equipment = equipment;
            _sensor = sensor;
            Populate();
        }

        private void Populate()
        {
            LabelEquipmentName.Text = Equipment.NAME;
            if (Equipment.isPump)
                EquipmentImage.Source = ImageSource.FromResource(
                    "Pump.Icons.activePump.png",
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly);
            var attachedSensor = _sensor.AttachedEquipment.FirstOrDefault(x => x?.id_Equipment == Equipment.ID);
            if (attachedSensor != null)
            {
                EquipmentCheckBox.IsChecked = true;
                SensorThresholdLow.Text = attachedSensor.ThresholdLow.ToString(CultureInfo.InvariantCulture);
                SensorThresholdHigh.Text = attachedSensor.ThresholdHigh.ToString(CultureInfo.InvariantCulture);
                SensorThresholdTimer.Text = attachedSensor.ThresholdTimer.ToString(CultureInfo.InvariantCulture);
            }
        }

        public AttachedSensor GetAttachedSensorDetail()
        {
            if (!EquipmentCheckBox.IsChecked)
                return null;


            return new AttachedSensor
            {
                id_Equipment = Equipment.ID,
                ThresholdLow = Convert.ToDouble(SensorThresholdLow.Text, CultureInfo.InvariantCulture),
                ThresholdHigh = Convert.ToDouble(SensorThresholdHigh.Text, CultureInfo.InvariantCulture),
                ThresholdTimer = Convert.ToDouble(SensorThresholdTimer.Text, CultureInfo.InvariantCulture)
            };
        }

        private void EquipmentCheckBox_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var equipmentCheckBox = (CheckBox)sender;
            SensorDetail.IsVisible = equipmentCheckBox.IsChecked;
        }

        public bool IsSelected()
        {
            return EquipmentCheckBox.IsChecked;
        }
    }
}