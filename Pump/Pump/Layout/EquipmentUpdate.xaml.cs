using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pump.Class;
using Pump.Database;
using Pump.Droid.Database.Table;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EquipmentUpdate : ContentPage
    {
        private readonly List<SubController> _subControllerList;
        private readonly List<Equipment> _equipmentList;
        private List<long> _avalibleGpio;
        private readonly Site _site;
        private readonly Equipment _equipment;
        private readonly SocketPicker _socketPicker;
        public EquipmentUpdate(List<Equipment> equipmentList, List<SubController> subControllerList, Site site, SocketPicker socketPicker, Equipment equipment = null)
        {
            InitializeComponent();
            _socketPicker = socketPicker;
            _site = site;
            _equipmentList = equipmentList;
            _subControllerList = subControllerList;
            if (equipment == null)
            {
                equipment = new Equipment();
                ButtonUpdateEquipment.Text = "Create";
            }
            else
                _equipmentList.Remove(equipment);
            

            _equipment = equipment;
            Populate();
        }

        private void Populate()
        {
            SystemPicker.SelectedIndexChanged += SystemPicker_OnSelectedIndexChanged;
            EquipmentName.Text = _equipment.NAME;
            SystemPicker.Items.Add("Main");
            SystemPicker.SelectedIndex = 0;
            var index = 1;
            foreach (var subController in _subControllerList)
            {
                SystemPicker.Items.Add(subController.NAME);
                if (_equipment.AttachedSubController != null && _equipment.AttachedSubController == subController.ID)
                    SystemPicker.SelectedIndex = index;
                index++;
            }

            if (_equipment.isPump)
            {
                IsPumpCheckBox.IsChecked = true;
                if (_equipment.DirectOnlineGPIO != null)
                {
                    IsDirectOnlineCheckBox.IsChecked = true;
                }
            }
            

            
            

            
        }
        private string EquipmentValidate()
        {
            var notification = "";

            if (string.IsNullOrWhiteSpace(EquipmentName.Text))
            {
                if (notification.Length < 1)
                    notification = "\u2022 Equipment name required";
                else
                    notification += "\n\u2022 Equipment name required";
                EquipmentName.PlaceholderColor = Color.Red;
                EquipmentName.Placeholder = "Equipment name";
            }

            if (SystemPicker.SelectedIndex == -1)
            {
                if (notification.Length < 1)
                    notification = "\u2022 Select a Sub-Controller";
                else
                    notification += "\n\u2022 Select a Sub-Controller";
            }

            if (GpioPicker.SelectedIndex == -1)
            {
                if (notification.Length < 1)
                    notification = "\u2022 Select a Pin";
                else
                    notification += "\n\u2022 Select a Pin";
            }

            if (DirectOnlineGpioPicker.SelectedIndex == -1 && IsDirectOnlineCheckBox.IsChecked)
            {
                if (notification.Length < 1)
                    notification = "\u2022 Select a Direct Online Pin";
                else
                    notification += "\n\u2022 Select a Direct Online Pin";
            }

            return notification;
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
            var isDirectOnlineCheckBox = (CheckBox) sender;
            StackLayoutDirectOnline.IsVisible = isDirectOnlineCheckBox.IsChecked;
        }

        private void SystemPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var systemPicker = (Picker) sender;
            var selectedIndex = systemPicker.SelectedIndex;
           _avalibleGpio = new GpioPins().GetDigitalGpioList();
            var usedEquipment = selectedIndex == 0 ? _equipmentList.Where(y => string.IsNullOrEmpty(y.AttachedSubController)).ToList() : _equipmentList.Where(y => !string.IsNullOrEmpty(y.AttachedSubController) && y.AttachedSubController == _subControllerList[SystemPicker.SelectedIndex - 1].ID).ToList();
            var usedPins = usedEquipment.Select(x => x.GPIO).ToList();
            usedPins.AddRange(usedEquipment.Where(x => x.DirectOnlineGPIO != null).Select(y => y.DirectOnlineGPIO.Value));
            
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
                if (_equipment.GPIO == gpio && ((usedEquipment.FirstOrDefault(x => x.AttachedSubController == _equipment.AttachedSubController) != null) || usedEquipment.Count == 0))
                    GpioPicker.SelectedIndex = index;
                index++;
            }

            index = 0;
            foreach (var gpio in _avalibleGpio)
            {
                DirectOnlineGpioPicker.Items.Add("Pin: " + gpio);
                if (_equipment.DirectOnlineGPIO != null && (_equipment.DirectOnlineGPIO == gpio && (usedEquipment.FirstOrDefault(x => x.AttachedSubController == _equipment.AttachedSubController) != null) || usedEquipment.Count == 0))
                    DirectOnlineGpioPicker.SelectedIndex = index;
                index++;
            }
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
                _equipment.GPIO = _avalibleGpio[GpioPicker.SelectedIndex];
                _equipment.isPump = IsPumpCheckBox.IsChecked;
                if (IsDirectOnlineCheckBox.IsChecked && IsPumpCheckBox.IsChecked)
                    _equipment.DirectOnlineGPIO = _avalibleGpio[DirectOnlineGpioPicker.SelectedIndex];
                _equipment.AttachedSubController = SystemPicker.SelectedIndex == 0 ? null : _subControllerList[SystemPicker.SelectedIndex - 1].ID;
                var key = await _socketPicker.SendCommand(_equipment);
                await UpdateEquipmentToSite(key);
                //

                await Navigation.PopModalAsync();
            }
        }

        private async Task UpdateEquipmentToSite(string key)
        {
            if(_site.Attachments.Contains(key))
                return;
            _site.Attachments.Add(key);
            await _socketPicker.SendCommand(_site);
        }
        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

    }
}