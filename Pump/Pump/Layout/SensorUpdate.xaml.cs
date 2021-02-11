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
    public partial class SensorUpdate : ContentPage
    {
        private List<long> _avalibleGpio;
        private List<long> _usableGpio;
        private readonly List<SubController> _subControllerList;
        private readonly List<Equipment> _equipmentList;
        private List<Sensor> _sensorList;
        private readonly List<string> _sensorTypesList = new List<string>{"Pressure Sensor"};
        private readonly Sensor _sensor;
        private Site _site;

        public SensorUpdate(List<Sensor> sensorList, List<SubController> subControllerList, List<Equipment> equipmentList, Site site, Sensor sensor = null)
        {
            InitializeComponent();
            _sensorList = sensorList;
            _subControllerList = subControllerList;
            _equipmentList = equipmentList;
            _site = site;
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
            SystemPicker.SelectedIndexChanged += SystemPicker_OnSelectedIndexChanged;
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
                if (_sensor.GPIO == gpio)
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
            return notification;
        }
        private async void ButtonUpdateSensor_OnClicked(object sender, EventArgs e)
        {
            
            var notification = SensorValidate();

            if (!string.IsNullOrWhiteSpace(notification))
            {
                await DisplayAlert("Incomplete", notification, "Understood");
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
                var sensorKey = await new Authentication().SetSensor(_sensor);
                await UpdateSensorToSite(sensorKey);

                foreach (var scrollViewEquipment in ScrollViewAttachedEquipment.Children)
                {
                    var viewEquipment = (ViewAttachedEquipment) scrollViewEquipment;
                    var attachedSensor = _equipmentList.First(x => x.ID == viewEquipment._equipment.ID).AttachedSensor.FirstOrDefault(x => x.ID == _sensor.ID);
                    var newAttachedSensor = viewEquipment.GetAttachedSensorDetail();

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
                    Navigation.PopModalAsync();
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

        private void SystemPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var systemPicker = (Picker)sender;
            var selectedIndex = systemPicker.SelectedIndex;
            _avalibleGpio = new GpioPins().GetAnalogGpioList();
            var usedSensors = selectedIndex == 0 ? _sensorList.Where(y => string.IsNullOrEmpty(y.AttachedSubController)).ToList() : _sensorList.Where(y => !string.IsNullOrEmpty(y.AttachedSubController) && y.AttachedSubController == _subControllerList[SystemPicker.SelectedIndex - 1].ID).ToList();
            var usedPins = usedSensors.Select(x => x.GPIO).ToList();
            
            for (var i = 0; i < _avalibleGpio.Count; i++)
            {
                if (!usedPins.Contains(_avalibleGpio[i])) continue;
                _avalibleGpio.RemoveAt(i);
                i--;
            }

            GpioPicker.Items.Clear();
            var index = 0;
            foreach (var gpio in _avalibleGpio)
            {
                GpioPicker.Items.Add("Pin: " + gpio);
                if (_sensor.GPIO == gpio && ((usedSensors.FirstOrDefault(x => x.AttachedSubController == _sensor.AttachedSubController) != null) || usedSensors.Count == 0))
                    GpioPicker.SelectedIndex = index;
                index++;
            }
        }
        private async Task UpdateSensorToSite(string key)
        {
            if (_site.Attachments.Contains(key))
                return;
            _site.Attachments.Add(key);
            var siteKey = await new Authentication().SetSite(_site);
        }
    }
}