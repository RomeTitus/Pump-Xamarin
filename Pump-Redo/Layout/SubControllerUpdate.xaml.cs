using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SubControllerUpdate : ContentPage
    {
        private readonly KeyValuePair<IrrigationConfiguration, ObservableIrrigation> _observableKeyValuePair;
        private readonly SocketPicker _socketPicker;
        private readonly SubController _subController;
        private readonly ViewSubControllerSummary _subControllerSummary;

        public SubControllerUpdate(SocketPicker socketPicker, SubController subController,
            KeyValuePair<IrrigationConfiguration, ObservableIrrigation> observableKeyValuePair, ViewSubControllerSummary subControllerSummary)
        {
            InitializeComponent();
            _socketPicker = socketPicker;
            _subController = subController;
            _observableKeyValuePair = observableKeyValuePair;
            _subControllerSummary = subControllerSummary;
            PopulateSubController();
        }

        private async void PopulateSubController()
        {
            await SetFocus();
            LabelSubController.Text = _subController.Name;
            SubControllerName.Text = _subController.Name;
            SubControllerMac.Text = _subController.Mac;
            SubControllerIp.Text = _subController.IpAddress;
            SubControllerPort.Text = _subController.Port.ToString();
            SubControllerLoRa.IsChecked = _subController.UseLoRa;
            StackLayoutKeys.IsVisible = _subController.UseLoRa;
            IncomingKey.Text = _subController.IncomingKey.ToString();
            for (var i = 0; i < _subController.OutgoingKey?.Count; i++)
            {
                OutgoingKey.Text += _subController.OutgoingKey[i].ToString();
                if (i != _subController.OutgoingKey.Count - 1)
                    OutgoingKey.Text += ",";
            }
        }

        private async Task SetFocus()
        {
            await Task.Delay(200);
            SubControllerName.TextBox_Focused(this, new FocusEventArgs(this, true));
            SubControllerMac.TextBox_Focused(this, new FocusEventArgs(this, true));
            IncomingKey.TextBox_Focused(this, new FocusEventArgs(this, true));
            OutgoingKey.TextBox_Focused(this, new FocusEventArgs(this, true));
            if (_subController.IpAddress != null)
                SubControllerIp.TextBox_Focused(this, new FocusEventArgs(this, true));
            if (string.IsNullOrEmpty(_subController.Port.ToString()) == false)
                SubControllerPort.TextBox_Focused(this, new FocusEventArgs(this, true));

            await Task.Delay(300);
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void SetSubControllerVariables()
        {
            _subController.Name = SubControllerName.Text;
            _subController.Mac = SubControllerMac.Text;
            _subController.IpAddress = SubControllerIp.Text;
            _subController.Port = int.Parse(SubControllerPort.Text);
            _subController.UseLoRa = SubControllerLoRa.IsChecked;
            _subController.IncomingKey = int.Parse(IncomingKey.Text);
            _subController.OutgoingKey = OutgoingKey.Text.Split(',').Select(int.Parse).ToList();
        }

        private async void ButtonUpdateSubController_OnClicked(object sender, EventArgs e)
        {
            SetSubControllerVariables();
            
            
            var loadingScreen = new PopupLoading ("Uploading");
            await PopupNavigation.Instance.PushAsync(loadingScreen);
            _subControllerSummary.LoadedData = false; //Force reload 
            await _socketPicker.SendCommand(_subController, _observableKeyValuePair.Key);
            await PopupNavigation.Instance.PopAllAsync();
            _subControllerSummary.AddStatusActivityIndicator();
            var index = _observableKeyValuePair.Value.SubControllerList.IndexOf(_subController);
            _observableKeyValuePair.Value.SubControllerList[index] = _subController;

            

            await Navigation.PopModalAsync();
        }

        private void TapGestureLoRa_OnTapped(object sender, EventArgs e)
        {
            SubControllerLoRa.IsChecked = !SubControllerLoRa.IsChecked;
        }

        private void SubControllerLoRa_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            StackLayoutKeys.IsVisible = e.Value;
        }
    }
}