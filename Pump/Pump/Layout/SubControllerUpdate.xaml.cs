using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SubControllerUpdate : ContentPage
    {
        private readonly SubController _subController;
        private readonly SocketPicker _socketPicker;
        public SubControllerUpdate(SocketPicker socketPicker, SubController subController = null)
        {
            InitializeComponent();
            _socketPicker = socketPicker;
            if (subController == null)
            {
                _subController = new SubController();
            }
            else
            {
                _subController = subController;
                PopulateSubController();
            }
        }

        private void PopulateSubController()
        {
            LabelHeading.Text = "Update Sub Controller";
            ButtonUpdateSubController.Text = "Update";
            ComKeyStackLayout.IsVisible = true;
            SubControllerMac.IsEnabled = false;
            SubControllerName.Text = _subController.NAME;
            SubControllerMac.Text = _subController.BTmac;
            SubControllerIp.Text = _subController.IpAdress;
            SubControllerPort.Text = _subController.Port.ToString();
            SubControllerLoRa.IsChecked = _subController.UseLoRa;
            IncomingKey.Text = _subController.IncomingKey.ToString();
            for (int i = 0; i < _subController.OutgoingKey?.Count; i++)
            {
                OutgoingKey.Text += _subController.OutgoingKey[i].ToString();
                if (i != _subController.OutgoingKey.Count - 1)
                    OutgoingKey.Text += ",";
            }
            
        }
        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void SetSubControllerVariables()
        {
            _subController.NAME = SubControllerName.Text;
            _subController.BTmac = SubControllerMac.Text;
            _subController.IpAdress = SubControllerIp.Text;
            _subController.Port = int.Parse(SubControllerPort.Text);
            _subController.UseLoRa = SubControllerLoRa.IsChecked;
            _subController.IncomingKey = int.Parse(IncomingKey.Text);
            _subController.OutgoingKey = OutgoingKey.Text.Split(',').Select(int.Parse).ToList();
        }

        private async void ButtonUpdateSubController_OnClicked(object sender, EventArgs e)
        {
            SetSubControllerVariables();
            await _socketPicker.SendCommand(_subController, false);
            await Navigation.PopModalAsync();
        }
    }
}