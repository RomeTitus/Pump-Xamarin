using System;
using System.Collections.Generic;
using System.Linq;
using Pump.Class;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SensorUpdate
    {
        private readonly KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation>
            _observableFilterKeyValuePair;

        private readonly Sensor _sensor;
        private readonly List<string> _sensorTypesList = new List<string> { "Pressure Sensor", "Temperature Sensor" };
        private readonly SocketPicker _socketPicker;
        private readonly List<Sensor> _sensorList;

        public SensorUpdate(
            KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation> observableFilterKeyValuePair,
            SocketPicker socketPicker, Sensor sensor = null)
        {
            InitializeComponent();
            _socketPicker = socketPicker;
            _observableFilterKeyValuePair = observableFilterKeyValuePair;
            _sensorList = new List<Sensor>();
            
            observableFilterKeyValuePair.Value.SensorList.ForEach(x => _sensorList.Add(x));
            
            if (sensor == null)
            {
                ButtonUpdateSensor.Text = "Create";
                _sensor = new Sensor();
                PopulateCreate();
            }
            else
            {
                _sensorList.Remove(sensor);
                _sensor = sensor;
                PopulateUpdate();
            }

           
        }

        private void PopulateCreate()
        {
            PopulateCommon();
            SystemPicker.SelectedIndexChanged += SystemPicker_OnSelectedIndexChanged;
        }
        
        private void PopulateUpdate()
        {
            FrameSystemPicker.BackgroundColor = Color.Gray;
            SystemPicker.IsEnabled = false;
            FrameSensorTypePicker.BackgroundColor = Color.Gray;
            SensorTypePicker.IsEnabled = false;
            PopulateCommon();
        }

        
        private void PopulateCommon()
        {
            foreach (var sensorType in _sensorTypesList)
            {
                SensorTypePicker.Items.Add(sensorType);
                if (_sensor.TYPE == sensorType)
                    SystemPicker.SelectedItem = _sensor.TYPE;
            }

            if (SensorTypePicker.SelectedIndex == -1)
                SensorTypePicker.SelectedIndex = 0;
            
            
            SensorName.Text = _sensor.NAME;
            if (_observableFilterKeyValuePair.Value.SubControllerList.Any() == false || _observableFilterKeyValuePair.Key.ControllerPairs.FirstOrDefault(x => x.Value.Contains(_observableFilterKeyValuePair.Value.SubControllerList.First().Id)).Value.Contains("MainController"))
                SystemPicker.Items.Add("Main");

            foreach (var subController in _observableFilterKeyValuePair.Value.SubControllerList)
            {
                SystemPicker.Items.Add(subController.Name);
                if (subController.Id == _sensor.AttachedSubController)
                    SystemPicker.SelectedItem = subController.Name;
            }
            if (SystemPicker.SelectedIndex == -1)
                SystemPicker.SelectedIndex = 0;
            foreach (var equipment in _observableFilterKeyValuePair.Value.EquipmentList.OrderByDescending(x => x.isPump))
                ScrollViewAttachedEquipment.Children.Add(new ViewAttachedEquipment(equipment, _sensor));
            
            PopulateAvailablePins(SystemPicker.SelectedIndex);
        }

        private string SensorValidate()
        {
            var notification = "";

            if (string.IsNullOrWhiteSpace(SensorName.Text))
            {
                if (notification.Length < 1)
                    notification += "\n\u2022 Equipment name required";
                SensorName.PlaceholderColor = Color.Red;
                SensorName.Placeholder = "Equipment name";
            }

            if (SystemPicker.SelectedIndex == -1)
                notification += "\n\u2022 Select a Sub-Controller";


            if (SensorTypePicker.SelectedIndex == -1)
                notification += "\n\u2022 Select a Sensor Type";


            if (GpioPicker.SelectedIndex == -1)
                notification += "\n\u2022 Select a Pin";

            return notification;
        }
        
        private void SensorTypePicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if(SystemPicker.SelectedIndex != -1)
                PopulateAvailablePins(SystemPicker.SelectedIndex);
        }

        private void SystemPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var systemPicker = (Picker)sender;
            if(SensorTypePicker.SelectedIndex != -1)
                PopulateAvailablePins(systemPicker.SelectedIndex);
        }
        
        private void PopulateAvailablePins(int selectedIndex)
        {
            var controllerSensor = selectedIndex == 0
                ? _sensorList.Where(y => string.IsNullOrEmpty(y.AttachedSubController)).ToList()
                : _sensorList.Where(y =>
                    y.AttachedSubController == _observableFilterKeyValuePair.Value
                        .SubControllerList[SystemPicker.SelectedIndex - 1].Id).ToList();

            GpioPicker.Items.Clear();
            var gpioPins =
                SensorTypePicker.SelectedItem.ToString() == "Pressure Sensor" ||
                SensorTypePicker.SelectedItem.ToString() == "Temperature Sensor"
                    ? GpioPins.GetAnalogGpioList()
                    : GpioPins.GetDigitalGpioList();
            
            foreach (var pin in gpioPins.Where(x => controllerSensor.Select(y => y.GPIO).Contains(x) == false))
            {
                GpioPicker.Items.Add("Pin: " + pin);
            }
            if(SensorTypePicker.SelectedItem.ToString() == "Pressure Sensor")
                GpioPicker.SelectedIndex = GpioPicker.Items.IndexOf("Pin: " + (_sensor.GPIO -37));
            else
                GpioPicker.SelectedIndex = GpioPicker.Items.IndexOf("Pin: " + _sensor.GPIO);
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


                _sensor.GPIO =
                    SensorTypePicker.SelectedItem.ToString() == "Pressure Sensor" ||
                    SensorTypePicker.SelectedItem.ToString() == "Temperature Sensor"
                        ? long.Parse(GpioPicker.SelectedItem.ToString().Replace("Pin: ", "")) + 37
                        : long.Parse(GpioPicker.SelectedItem.ToString().Replace("Pin: ", ""));


                _sensor.TYPE = _sensorTypesList[SensorTypePicker.SelectedIndex];

                if (SystemPicker.SelectedIndex == 0)
                    _sensor.AttachedSubController = null;
                else
                    _sensor.AttachedSubController = _observableFilterKeyValuePair.Value
                        .SubControllerList[SystemPicker.SelectedIndex - 1].Id;

                _sensor.AttachedEquipment = GetAttachedEquipment();
                
                var loadingScreen = new PopupLoading ("Uploading");
                await PopupNavigation.Instance.PushAsync(loadingScreen);
                await _socketPicker.SendCommand(_sensor, _observableFilterKeyValuePair.Key);
                await PopupNavigation.Instance.PopAllAsync();
                
                if (_observableFilterKeyValuePair.Value.SensorList.Any(x => x.Id == _sensor.Id))
                {
                    var index = _observableFilterKeyValuePair.Value.SensorList.IndexOf(_sensor);
                    _observableFilterKeyValuePair.Value.SensorList[index] = _sensor;
                }
                else
                {
                    _observableFilterKeyValuePair.Value.SensorList.Add(_sensor);
                }

                await Navigation.PopModalAsync();
            }
        }

        private List<AttachedSensor> GetAttachedEquipment()
        {
            var attachedSensors = new List<AttachedSensor>();
            foreach (var viewAttachedEquipment in ScrollViewAttachedEquipment.Children)
            {
                var attachedEquipment = (ViewAttachedEquipment)viewAttachedEquipment;
                if (!attachedEquipment.IsSelected()) continue;
                attachedSensors.Add(attachedEquipment.GetAttachedSensorDetail());
            }

            return attachedSensors;
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}