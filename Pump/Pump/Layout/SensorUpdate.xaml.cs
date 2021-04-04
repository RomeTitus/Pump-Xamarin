using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pump.Class;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
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
        private readonly List<Sensor> _sensorList;
        private readonly List<string> _sensorTypesList = new List<string>{"Pressure Sensor", "Temperature Sensor" };
        private readonly Sensor _sensor;
        private readonly Site _site;
        private readonly SocketPicker _socketPicker;

        public SensorUpdate(List<Sensor> sensorList, List<SubController> subControllerList, List<Equipment> equipmentList, Site site, SocketPicker socketPicker, Sensor sensor = null)
        {
            InitializeComponent();
            _socketPicker = socketPicker;
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

            foreach (var equipment in _equipmentList.OrderBy(x => !x.isPump))
            {
                ScrollViewAttachedEquipment.Children.Add(new ViewAttachedEquipment(equipment, _sensor));
            }

            index = 0;
            foreach (var sensor in _sensorTypesList)
            {
                SensorTypePicker.Items.Add(sensor);
                if (_sensor.TYPE == sensor)
                    SensorTypePicker.SelectedIndex = index;
                index++;
            }

            if (SensorTypePicker.SelectedIndex == -1)
                SensorTypePicker.SelectedIndex = 0;
            
        }

        private void UpdateGpioPicker()
        {
            GpioPicker.Items.Clear();
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
                _sensor.AttachedEquipment.Clear();
                foreach (var viewAttachedEquipment in ScrollViewAttachedEquipment.Children)
                {
                    var attachedEquipment = (ViewAttachedEquipment) viewAttachedEquipment;
                    if(!attachedEquipment.IsSelected()) continue;
                    _sensor.AttachedEquipment.Add(attachedEquipment.GetAttachedSensorDetail());
                }
                
                await _socketPicker.SendCommand(_sensor);
                if(!_site.Attachments.Contains(_sensor.ID))
                    await UpdateSensorToSite(_sensor.ID);
                
                await Navigation.PopModalAsync();
            }
            
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private async void SensorTypePicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            
            var sensorType = (Picker) sender;
            if (sensorType.SelectedIndex == -1)
                return;
            if (sensorType.Items[sensorType.SelectedIndex] == "Pressure Sensor" || sensorType.Items[sensorType.SelectedIndex] == "Temperature Sensor")
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

            if (!_usableGpio.Any())
            {
                ButtonUpdateSensor.IsEnabled = false;
                await DisplayAlert("Notification", "No Available pins for " + sensorType.Items[sensorType.SelectedIndex], "Understood");
            }
        }

        private void SystemPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var systemPicker = (Picker)sender;
            var selectedIndex = systemPicker.SelectedIndex;
            _avalibleGpio = new GpioPins().GetAnalogGpioList();
            var usedSensors = selectedIndex == 0 ? _sensorList.Where(y => string.IsNullOrEmpty(y.AttachedSubController)).ToList() : _sensorList.Where(y => !string.IsNullOrEmpty(y.AttachedSubController) && y.AttachedSubController == _subControllerList[SystemPicker.SelectedIndex - 1].ID).ToList();
            var usedPins = usedSensors.Where(x => x.ID != _sensor.ID).Select(x => x.GPIO).ToList();
            
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

            SensorTypePicker_OnSelectedIndexChanged(SensorTypePicker, null);
        }
        private async Task UpdateSensorToSite(string key)
        {
            if (_site.Attachments.Contains(key))
                return;
            _site.Attachments.Add(key);
            await _socketPicker.SendCommand(_site);
        }
    }
}