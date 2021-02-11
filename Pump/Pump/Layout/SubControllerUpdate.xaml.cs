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
    public partial class SubControllerUpdate : ContentPage
    {
        private SubController _subController;
        public SubControllerUpdate(SubController subController = null)
        {
            InitializeComponent();
            if (subController == null)
            {
                _subController = new SubController();
            }
            else
            {
                _subController = subController;
                Populate();
            }
        }

        private void Populate()
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
            SubControllerKey.Text = _subController.Key.ToString();
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
            _subController.Key = int.Parse(SubControllerKey.Text);
        }

        private async void ButtonUpdateSubController_OnClicked(object sender, EventArgs e)
        {
            SetSubControllerVariables();
            var key = await new Authentication().SetSubController(_subController);
            await Navigation.PopModalAsync();
        }
    }
}