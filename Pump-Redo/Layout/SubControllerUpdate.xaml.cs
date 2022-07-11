using System;
using System.Linq;
using System.Threading.Tasks;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SubControllerUpdate : ContentPage
    {
        private readonly SocketPicker _socketPicker;
        private readonly SubController _subController;
        private readonly IrrigationConfiguration _configuration;

        public SubControllerUpdate(SocketPicker socketPicker, SubController subController, IrrigationConfiguration configuration)
        {
            InitializeComponent();
            _socketPicker = socketPicker;
            _subController = subController;
            _configuration = configuration;
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
            if(_subController.IpAddress != null)
                SubControllerIp.TextBox_Focused(this, new FocusEventArgs(this, true));
            if(string.IsNullOrEmpty(_subController.Port.ToString()) == false)
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
            await _socketPicker.SendCommand(_subController, _configuration);
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