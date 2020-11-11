using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EquipmentUpdate : ContentPage
    {
        List<long> _avalibleGpio;
        private List<SubController> _subControllerList;
        private Equipment _equipment;

        public EquipmentUpdate(List<long> avalibleGpio, List<SubController> subControllerList, Equipment equipment = null)
        {
            InitializeComponent();
            _avalibleGpio = avalibleGpio;
            _subControllerList = subControllerList;
            if (equipment == null)
            {
                equipment = new Equipment();
                ButtonUpdateEquipment.Text = "Create";
            }

            _equipment = equipment;
            populate();
        }

        private void populate()
        {
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
                

            
                

            index = 0;
            foreach (var gpio in _avalibleGpio)
            {
                GpioPicker.Items.Add("Pin: " + gpio);
                if (_equipment.GPIO == gpio)
                    GpioPicker.SelectedIndex = index;
                index++;
            }
            
            //if (GpioPicker.SelectedIndex == -1 && GpioPicker.Items.Count > 0)
            //    GpioPicker.SelectedItem = 0;

            index = 0;
            foreach (var gpio in _avalibleGpio)
            {
                DirectOnlineGpioPicker.Items.Add("Pin: " + gpio);
                if (_equipment.DirectOnlineGPIO != null && _equipment.DirectOnlineGPIO == gpio)
                    DirectOnlineGpioPicker.SelectedIndex = index;
                index++;
            }
            //if (DirectOnlineGpioPicker.SelectedIndex == -1 && DirectOnlineGpioPicker.Items.Count > 0)
            //    DirectOnlineGpioPicker.SelectedItem = 0;
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
        private void ButtonUpdateEquipment_OnClicked(object sender, EventArgs e)
        {
            var notification = EquipmentValidate();
            
            if (!string.IsNullOrWhiteSpace(notification))
            {
                DisplayAlert("Incomplete", notification, "Understood");
            }
            else
            {
                _equipment.NAME = EquipmentName.Text;
                _equipment.GPIO = _avalibleGpio[GpioPicker.SelectedIndex];
                _equipment.isPump = IsPumpCheckBox.IsChecked;
                if (IsDirectOnlineCheckBox.IsChecked && IsPumpCheckBox.IsChecked)
                    _equipment.DirectOnlineGPIO = _avalibleGpio[DirectOnlineGpioPicker.SelectedIndex];
                if (SystemPicker.SelectedIndex == 0)
                    _equipment.AttachedSubController = null;
                else
                {
                    _equipment.AttachedSubController = _subControllerList[SystemPicker.SelectedIndex -1].ID;
                }
                var key = Task.Run(() => new Authentication().SetEquipment(_equipment)).Result;
                Navigation.PopModalAsync();
            }
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
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

    }
}