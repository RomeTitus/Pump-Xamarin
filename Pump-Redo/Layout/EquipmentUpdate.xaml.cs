using System;
using System.Collections.Generic;
using System.Linq;
using Pump.Class;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EquipmentUpdate
    {
        private readonly Equipment _equipment;
        private readonly List<Equipment> _equipmentList;

        private readonly KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation>
            _observableFilterKeyValuePair;

        private readonly EquipmentScreen _equipmentScreen;
        private readonly SocketPicker _socketPicker;

        public EquipmentUpdate(
            KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation> observableFilterKeyValuePair,
            SocketPicker socketPicker, EquipmentScreen equipmentScreen, Equipment equipment = null)
        {
            InitializeComponent();
            _socketPicker = socketPicker;
            _observableFilterKeyValuePair = observableFilterKeyValuePair;
            _equipmentScreen = equipmentScreen;
            _equipmentList = new List<Equipment>();
            
            observableFilterKeyValuePair.Value.EquipmentList.ForEach(x => _equipmentList.Add(x));
            
            if (equipment == null)
            {
                ButtonUpdateEquipment.Text = "Create";
                _equipment = new Equipment();
                PopulateCreate();
            }

            else
            {
                _equipmentList.Remove(equipment);
                _equipment = equipment;
                PopulateUpdate();
             }
        }

        private void PopulateCreate()
        {
            
            PopulateCommon();
        }

        private void PopulateUpdate()
        {
            FrameSystemPicker.BackgroundColor = Color.Gray;
            IsPumpCheckBox.Color = Color.Gray;
            SystemPicker.IsEnabled = false;
            IsPumpCheckBox.IsEnabled = false;
            PopulateCommon();
        }

        private void PopulateCommon()
        {
            SystemPicker.SelectedIndexChanged += SystemPicker_OnSelectedIndexChanged;
            EquipmentName.Text = _equipment.NAME;
            if(_observableFilterKeyValuePair.Value.SubControllerList.Any() == false)
                SystemPicker.Items.Add("Main");
            
            foreach (var subController in _observableFilterKeyValuePair.Value.SubControllerList)
            {
                SystemPicker.Items.Add(subController.Name);
                if (subController.Id == _equipment.AttachedSubController)
                    SystemPicker.SelectedItem = subController.Name;
            }
            if (SystemPicker.SelectedIndex == -1)
                SystemPicker.SelectedIndex = 0;
            if (_equipment.isPump)
                IsPumpCheckBox.IsChecked = true;
            if (_equipment.DirectOnlineGPIO != null) 
                IsDirectOnlineCheckBox.IsChecked = true;
        }
        
        private void IsPumpCheckBox_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var isPumpCheckBox = (CheckBox)sender;
            StackLayoutPump.IsVisible = isPumpCheckBox.IsChecked;
            if (isPumpCheckBox.IsChecked == false)
                IsDirectOnlineCheckBox.IsChecked = false;
        }

        private void IsDirectOnlineCheckBox_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var isDirectOnlineCheckBox = (CheckBox)sender;
            StackLayoutDirectOnline.IsVisible = isDirectOnlineCheckBox.IsChecked;
        }

        private void SystemPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var systemPicker = (Picker)sender;
            PopulateAvailablePins(systemPicker.SelectedIndex);
        }

        private void PopulateAvailablePins(int selectedIndex)
        {
            var controllerEquipment = selectedIndex == 0
                ? _equipmentList.Where(y => string.IsNullOrEmpty(y.AttachedSubController)).ToList()
                : _equipmentList.Where(y =>
                    y.AttachedSubController == _observableFilterKeyValuePair.Value
                        .SubControllerList[SystemPicker.SelectedIndex - 1].Id).ToList();

            GpioPicker.Items.Clear();
            DirectOnlineGpioPicker.Items.Clear();
            foreach (var pin in GpioPins.GetDigitalGpioList()
                         .Where(x => controllerEquipment.Select(y => y.GPIO).Contains(x) == false && 
                                     controllerEquipment.Select(y => y.DirectOnlineGPIO).Contains(x) == false))
            {
                if(_equipment.DirectOnlineGPIO is null || pin != _equipment.DirectOnlineGPIO)
                    GpioPicker.Items.Add("Pin: " + pin);
                
                if(pin != _equipment.GPIO)
                    DirectOnlineGpioPicker.Items.Add("Pin: " + pin);
            }
            
            GpioPicker.SelectedIndex = GpioPicker.Items.IndexOf("Pin: " + _equipment.GPIO);
            
            if (_equipment.DirectOnlineGPIO is not null)
                DirectOnlineGpioPicker.SelectedIndex = DirectOnlineGpioPicker.Items.IndexOf("Pin: " + _equipment.DirectOnlineGPIO);
        }

        private string EquipmentValidate()
        {
            var notification = "";

            if (string.IsNullOrWhiteSpace(EquipmentName.Text))
            {
                notification += "\n\u2022 Equipment name required";
                EquipmentName.PlaceholderColor = Color.Red;
                EquipmentName.Placeholder = "Equipment name";
            }

            if (SystemPicker.SelectedIndex == -1)
                notification += "\n\u2022 Select a Sub-Controller";


            if (GpioPicker.SelectedIndex == -1)
                notification += "\n\u2022 Select a Pin";


            if (DirectOnlineGpioPicker.SelectedIndex == -1 && IsDirectOnlineCheckBox.IsChecked)
                notification += "\n\u2022 Select a Direct Online Pin";

            return notification;
        }

        private async void ButtonUpdateEquipment_OnClicked(object sender, EventArgs e)
        {
            var notification = EquipmentValidate();

            if (!string.IsNullOrWhiteSpace(notification))
            {
                await DisplayAlert("Incomplete", notification, "Understood");
            }
            else
            {
                _equipment.NAME = EquipmentName.Text;
                _equipment.GPIO = long.Parse(GpioPicker.SelectedItem.ToString().Replace("Pin: ", ""));
                _equipment.isPump = IsPumpCheckBox.IsChecked;
                if (IsDirectOnlineCheckBox.IsChecked && IsPumpCheckBox.IsChecked)
                    _equipment.DirectOnlineGPIO =
                        long.Parse(DirectOnlineGpioPicker.SelectedItem.ToString().Replace("Pin: ", ""));
                _equipment.AttachedSubController = SystemPicker.SelectedItem.ToString() == "Main" ? null : _observableFilterKeyValuePair.Value.SubControllerList[SystemPicker.SelectedIndex].Id;
                
                
                
                var loadingScreen = new PopupLoading ("Uploading");
                await PopupNavigation.Instance.PushAsync(loadingScreen);
                await _socketPicker.SendCommand(_equipment, _observableFilterKeyValuePair.Key);
                await PopupNavigation.Instance.PopAllAsync();
                    
                    
                if (_observableFilterKeyValuePair.Value.EquipmentList.Any(x => x.Id == _equipment.Id))
                {
                    var index = _observableFilterKeyValuePair.Value.EquipmentList.IndexOf(_equipment);
                    _observableFilterKeyValuePair.Value.EquipmentList[index] = _equipment;
                }
                else
                {
                    _observableFilterKeyValuePair.Value.EquipmentList.Add(_equipment);
                }

                _equipmentScreen.AddLoadingEquipmentScreenFromId(_equipment.Id);   
                await Navigation.PopModalAsync();
            }
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}