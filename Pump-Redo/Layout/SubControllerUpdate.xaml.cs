using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pump.Class;
using Pump.CustomRender;
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
        private readonly MainPage _mainPage;
        public SubControllerUpdate(SocketPicker socketPicker, SubController subController,
            KeyValuePair<IrrigationConfiguration, ObservableIrrigation> observableKeyValuePair, ViewSubControllerSummary subControllerSummary, MainPage mainPage)
        {
            InitializeComponent();
            _socketPicker = socketPicker;
            _subController = subController;
            _observableKeyValuePair = observableKeyValuePair;
            _subControllerSummary = subControllerSummary;
            _mainPage = mainPage;
            PopulateSubController();
        }

        private async void PopulateSubController()
        {
            await SetFocus();
            LabelSubController.Text = _subController.Name;
            SubControllerName.Text = _subController.Name;
            SubControllerMac.Text = _subController.DeviceGuid.Split('-').Last();
            SubControllerIp.Text = _subController.AddressPath;
            SubControllerLoRa.IsChecked = _subController.UseLoRa;
            StackLayoutKeys.IsVisible = _subController.UseLoRa;


            for (var i = 0; i < _subController.KeyPath?.Count; i++)
            {
                KeyPathEntery.Text += _subController.KeyPath[i].ToString();
                if (i != _subController.KeyPath.Count - 1)
                    KeyPathEntery.Text += ">";
            }
        }

        private async Task SetFocus()
        {
            await Task.Delay(200);
            SubControllerName.TextBox_Focused(this, new FocusEventArgs(this, true));
            SubControllerMac.TextBox_Focused(this, new FocusEventArgs(this, true));
            KeyPathEntery.TextBox_Focused(this, new FocusEventArgs(this, true));
            if (_subController.AddressPath != null)
                SubControllerIp.TextBox_Focused(this, new FocusEventArgs(this, true));

            await Task.Delay(300);
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void SetSubControllerVariables()
        {
            _subController.Name = SubControllerName.Text;
            _subController.AddressPath = SubControllerIp.Text;
            _subController.UseLoRa = SubControllerLoRa.IsChecked;
            _subController.KeyPath = new List<int>();
            foreach (var key in KeyPathEntery.Text.Split('>'))
            {
                _subController.KeyPath.Add(int.Parse(key));
            }
        }

        private string Validation()
        {
            var notification = ValidateIpTextChange(SubControllerIp);
            notification += KeyPathTextValidator(KeyPathEntery);
            return notification;
        }

        private async void ButtonUpdateSubController_OnClicked(object sender, EventArgs e)
        {
            var notification = Validation();

            if (string.IsNullOrEmpty(notification) == false)
            {
                await DisplayAlert("Sub Controller", notification, "Understood");
                return;
            }

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

        private void IpEntry_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (EntryOutlined)sender;
            ValidateIpTextChange(entry);
        }

        private void KeyPathEntry_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (EntryOutlined)sender;
            KeyPathTextValidator(entry);
        }

        private void SetPlaceholderColor(EntryOutlined entry, Color placeholderColor, Color borderColor)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                entry.PlaceholderColor = placeholderColor;
                entry.BorderColor = borderColor;
            });
        }

        private string ValidateIpTextChange(EntryOutlined entry)
        {
            var allowedCharacters = new List<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', ':' };

            if (string.IsNullOrEmpty(entry.Text))
                return string.Empty;

            if (entry.Text.Any(charValue => !allowedCharacters.Contains(charValue)))
            {
                SetPlaceholderColor(entry, Color.Red, Color.Red);
                return "\n\u2022 incorrect format";
            }

            if (entry.Text.Length > 3 && !entry.Text.Contains("."))
            {
                SetPlaceholderColor(entry, Color.Red, Color.Red);
                return "\n\u2022 incorrect format";
            }

            var IpAndPort = entry.Text.Split(':');
            var ipArray = IpAndPort[0].Split('.');
            if (ipArray.Any(subIp => subIp.Length > 3))
            {
                SetPlaceholderColor(entry, Color.Red, Color.Red);
                return "\n\u2022 incorrect format";
            }

            if (IpAndPort.Length == 1)
            {
                SetPlaceholderColor(entry, Color.Red, Color.Red);
                return "\n\u2022 incorrect format";
            }

            if (IpAndPort.Length > 2)
            {
                SetPlaceholderColor(entry, Color.Red, Color.Red);
                return "\n\u2022 incorrect format";
            }

            if (IpAndPort.Length > 1 && (IpAndPort[1].Length == 0 || IpAndPort[1].Length > 5))
            {
                SetPlaceholderColor(entry, Color.Red, Color.Red);
                return "\n\u2022 incorrect format";
            }

            SetPlaceholderColor(entry, Color.Navy, Color.Black);
            return string.Empty;
        }

        private string KeyPathTextValidator(EntryOutlined entry)
        {
            var allowedCharacters = new List<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '>'};
            
            if (string.IsNullOrEmpty(entry.Text))
                return string.Empty;

            if (entry.Text.Any(charValue => !allowedCharacters.Contains(charValue)))
            {
                SetPlaceholderColor(entry, Color.Red, Color.Red);
                return "\n\u2022 incorrect format";
            }

            var keyArray = entry.Text.Split('>');

            

            foreach ( var key in keyArray )
            {
                if(string.IsNullOrEmpty(key) || key.Length > 3)
                {
                    SetPlaceholderColor(entry, Color.Red, Color.Red);
                    return "\n\u2022 incorrect format";
                }
            }

            SetPlaceholderColor(entry, Color.Navy, Color.Black);
            return string.Empty;
        }

        private async void Button_OnPressed_IrrigationConfig(object sender, EventArgs e)
        {
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var loadingScreen = new PopupLoading("Connecting...");
                    await PopupNavigation.Instance.PushAsync(loadingScreen);

                    var blueToothManager = _socketPicker.BluetoothManager();
                    await blueToothManager.ConnectToKnownDevice(new Guid(_subController.DeviceGuid), new CancellationToken(), 3);
                    await blueToothManager.IsValidController();
                    await PopupNavigation.Instance.PopAllAsync();

                    await Navigation.PushModalAsync(new SetupSystem(blueToothManager, new NotificationEvent(), _mainPage));
                }
                catch (Exception exception)
                {
                    await PopupNavigation.Instance.PopAllAsync();
                    await DisplayAlert("Connect Exception!", exception.Message, "Understood");
                }
            });
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