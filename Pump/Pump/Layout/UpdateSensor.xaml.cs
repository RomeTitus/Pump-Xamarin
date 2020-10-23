using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pump.Class;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdateSensor : ContentPage
    {
        private readonly List<long> _avalibleGpio;
        private List<long> _usableGpio;
        private readonly List<SubController> _subControllerList;
        private readonly List<Equipment> _equipmentList;
        private readonly List<string> _sensorTypesList = new List<string>{"Pressure Sensor"};
        private readonly Sensor _sensor;

        public UpdateSensor(List<long> avalibleGpio, List<SubController> subControllerList, List<Equipment> equipmentList, Sensor sensor = null)
        {
            InitializeComponent();
            _avalibleGpio = avalibleGpio;
            _subControllerList = subControllerList;
            _equipmentList = equipmentList;
            if (sensor == null)
            {
                sensor = new Sensor();
                ButtonUpdateSensor.Text = "Create";
            }

            _sensor = sensor;
            Populate();
        }

        private void Populate()
        {
            SensorName.Text = _sensor.NAME;
            SystemPicker.Items.Add("Main");
            SystemPicker.SelectedIndex = 0;
            var index = 1;
            foreach (var subController in _subControllerList)
            {
                SystemPicker.Items.Add(subController.NAME);
                if (_sensor.AttachedSubController != null && _sensor.AttachedSubController == subController.ID)
                    SystemPicker.SelectedIndex = index;
                index++;
            }

            index = 0;
            foreach (var sensorType in _sensorTypesList)
            {
                SensorTypePicker.Items.Add(sensorType);
                if (_sensor.TYPE != null && _sensor.TYPE == sensorType)
                    SensorTypePicker.SelectedIndex = index;
                index++;
            }

            foreach (var equipment in _equipmentList)
            {
                ScrollViewAttachedEquipment.Children.Add(new ViewAttachedEquipment(equipment, _sensor));
            }
        }

        private void UpdateGpioPicker()
        {
            var index = 0;
            foreach (var gpio in _usableGpio)
            {
                GpioPicker.Items.Add("Pin: " + gpio);
                if (_sensor.GPIO != null && _sensor.GPIO == gpio)
                    GpioPicker.SelectedIndex = index;
                index++;
            }
        }

        private string SensorValidate()
        {
            var notification = "";

            if (string.IsNullOrWhiteSpace(SensorName.Text))
            {
                if (notification.Length < 1)
                    notification = "\u2022 Equipment name required";
                else
                    notification += "\n\u2022 Equipment name required";
                SensorName.PlaceholderColor = Color.Red;
                SensorName.Placeholder = "Equipment name";
            }

            if (SystemPicker.SelectedIndex == -1)
            {
                if (notification.Length < 1)
                    notification = "\u2022 Select a Sub-Controller";
                else
                    notification += "\n\u2022 Select a Sub-Controller";
            }

            if (SensorTypePicker.SelectedIndex == -1)
            {
                if (notification.Length < 1)
                    notification = "\u2022 Select a Sensor Type";
                else
                    notification += "\n\u2022 Select a Sensor Type";
            }

            if (GpioPicker.SelectedIndex == -1)
            {
                if (notification.Length < 1)
                    notification = "\u2022 Select a Pin";
                else
                    notification += "\n\u2022 Select a Pin";
            }

            

            /*
            if (DirectOnlineGpioPicker.SelectedIndex == -1 && IsDirectOnlineCheckBox.IsChecked)
            {
                if (notification.Length < 1)
                    notification = "\u2022 Select a Direct Online Pin";
                else
                    notification += "\n\u2022 Select a Direct Online Pin";
            }
            */
            return notification;
        }
        private void ButtonUpdateSensor_OnClicked(object sender, EventArgs e)
        {
            
            var notification = SensorValidate();

            if (!string.IsNullOrWhiteSpace(notification))
            {
                DisplayAlert("Incomplete", notification, "Understood");
            }
            else
            {
                _sensor.NAME = SensorName.Text;
                _sensor.GPIO = _usableGpio[GpioPicker.SelectedIndex];

                _sensor.TYPE = _sensorTypesList[SensorTypePicker.SelectedIndex];
                
                if (SystemPicker.SelectedIndex == 0)
                    _sensor.AttachedSubController = null;
                else
                {
                    _sensor.AttachedSubController = _subControllerList[SystemPicker.SelectedIndex - 1].ID;
                }
                var sensorKey = Task.Run(() => new Authentication().SetSensor(_sensor)).Result;
                Navigation.PopModalAsync();

                foreach (var scrollViewEquipment in ScrollViewAttachedEquipment.Children)
                {
                    var viewEquipment = (ViewAttachedEquipment) scrollViewEquipment;
                    var attachedSensor = _equipmentList.First(x => x.ID == viewEquipment._equipment.ID).AttachedSensor.FirstOrDefault(x => x.ID == _sensor.ID);
                    var newAttachedSensor = viewEquipment.GetAttachedSensorDetail();

                    if (newAttachedSensor != null)
                    {
                        var test = "";
                    }
                    if (JsonConvert.SerializeObject(attachedSensor) == JsonConvert.SerializeObject(newAttachedSensor)) continue;

                    if(newAttachedSensor == null)
                        _equipmentList.First(x => x.ID == viewEquipment._equipment.ID).AttachedSensor.Remove(attachedSensor);
                    else
                    {
                        int index = _equipmentList.First(x => x.ID == viewEquipment._equipment.ID).AttachedSensor.IndexOf(attachedSensor);
                        if (index != -1)
                            _equipmentList.First(x => x.ID == viewEquipment._equipment.ID).AttachedSensor[index] = newAttachedSensor;
                        else
                        {
                            _equipmentList.First(x => x.ID == viewEquipment._equipment.ID).AttachedSensor.Add(newAttachedSensor);
                        }
                    }
                    var equipmentKey = Task.Run(() => new Authentication().SetEquipment(_equipmentList.First(x => x.ID == viewEquipment._equipment.ID))).Result;
                    
                }
            }
            
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void SensorTypePicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var sensorType = (Picker) sender;
            if (sensorType.Items[sensorType.SelectedIndex] == "Pressure Sensor")
            {
                _usableGpio = new GpioPins().GetAnalogGpioList().Where(x => _avalibleGpio.Contains(x)).Select(x => x)
                    .ToList();
            }
            else
            {
                _usableGpio = new GpioPins().GetDigitalGpioList().Where(x => _avalibleGpio.Contains(x)).Select(x => x)
                    .ToList();
            }

            UpdateGpioPicker();
        }
    }
}