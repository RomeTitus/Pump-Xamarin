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
    public partial class IrrigationControllerSettings
    {
        private readonly List<string> _connectionLabel = new List<string> { "Cloud", "Network", "Bluetooth" };
        private readonly KeyValuePair<IrrigationConfiguration, ObservableIrrigation> _keyValueIrrigation;
        private readonly SocketPicker _socketPicker;
        private readonly MainPage _mainPage;

        public IrrigationControllerSettings(MainPage mainPage,
            KeyValuePair<IrrigationConfiguration, ObservableIrrigation> keyValueIrrigation, SocketPicker socketPicker)
        {
            InitializeComponent();
            _mainPage = mainPage;
            _keyValueIrrigation = keyValueIrrigation;
            _socketPicker = socketPicker;
            Populate();
        }

        private async void Populate()
        {
            LabelName.Text = _keyValueIrrigation.Key.Path;
            _connectionLabel.ForEach(x => ConnectionTypePicker.Items.Add(x));
            ConnectionTypePicker.SelectedIndex = _keyValueIrrigation.Key.ConnectionType;
            await SetFocus();
            InternalIpEntry.Text = _keyValueIrrigation.Key.InternalPath;
            InternalPortEntry.Text = _keyValueIrrigation.Key.InternalPort.ToString();
            ExternalIpEntry.Text = _keyValueIrrigation.Key.ExternalPath;
            ExternalPortEntry.Text = _keyValueIrrigation.Key.ExternalPort.ToString();
            PopulateSites();
        }

        private void PopulateSites()
        {
            foreach (var keyControllerPair in _keyValueIrrigation.Key.ControllerPairs)
                SiteLayout.Children.Add(new ViewSiteSummary(keyControllerPair, _keyValueIrrigation, _socketPicker));
        }

        private async Task SetFocus()
        {
            await Task.Delay(200);

            if (_keyValueIrrigation.Key.InternalPath != null)
                InternalIpEntry.TextBox_Focused(this, new FocusEventArgs(this, true));
            else
                InternalIpEntry.TextBox_Unfocused(this, new FocusEventArgs(this, true));

            if (_keyValueIrrigation.Key.InternalPort != null)
                InternalPortEntry.TextBox_Focused(this, new FocusEventArgs(this, true));
            else
                InternalPortEntry.TextBox_Unfocused(this, new FocusEventArgs(this, true));

            if (_keyValueIrrigation.Key.ExternalPath != null)
                ExternalIpEntry.TextBox_Focused(this, new FocusEventArgs(this, true));
            else
                ExternalIpEntry.TextBox_Unfocused(this, new FocusEventArgs(this, true));

            if (_keyValueIrrigation.Key.ExternalPort != null)
                ExternalPortEntry.TextBox_Focused(this, new FocusEventArgs(this, true));
            else
                ExternalPortEntry.TextBox_Unfocused(this, new FocusEventArgs(this, true));

            await Task.Delay(300);
        }

        private string Validation()
        {
            var notification = "";

            foreach (var view in SiteLayout.Children)
            {
                var viewSite = (ViewSiteSummary)view;
                var entry = viewSite.GetSiteNameEntry();
                if (string.IsNullOrEmpty(entry.Text))
                {
                    notification += "\n\u2022 Site name cannot be empty";
                    SetPlaceholderColor(entry, Color.Red, Color.Red);
                }
            }

            notification += ValidateIpTextChange(InternalIpEntry, "Internal IP");
            notification += ValidateIpTextChange(ExternalIpEntry, "External IP");

            notification += ValidatePortTextChange(InternalPortEntry, "Internal Port");
            notification += ValidatePortTextChange(ExternalPortEntry, "External Port");

            if (string.IsNullOrEmpty(InternalIpEntry.Text) == false && InternalIpEntry.Text.Any() && string.IsNullOrEmpty(InternalPortEntry.Text))
            {
                notification += "\n\u2022 Internal port cannot be empty when Internal IP is filled in";
                SetPlaceholderColor(InternalPortEntry, Color.Red, Color.Red);
            }

            if (string.IsNullOrEmpty(ExternalIpEntry.Text) == false && ExternalIpEntry.Text.Any() && string.IsNullOrEmpty(ExternalPortEntry.Text))
            {
                notification += "\n\u2022 External port cannot be empty when External IP is filled in";
                SetPlaceholderColor(ExternalPortEntry, Color.Red, Color.Red);
            }

            return notification;
        }


        private string ValidateIpTextChange(EntryOutlined entry, string interfaceName = "")
        {
            var allowedCharacters = new List<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.' };

            if (string.IsNullOrEmpty(entry.Text))
                return string.Empty;
            
            if (entry.Text.Any(charValue => !allowedCharacters.Contains(charValue)))
            {
                SetPlaceholderColor(entry, Color.Red, Color.Red);
                return "\n\u2022" + interfaceName + " incorrect format";
            }

            if (entry.Text.Length > 3 && !entry.Text.Contains("."))
            {
                SetPlaceholderColor(entry, Color.Red, Color.Red);
                return "\n\u2022" + interfaceName + " incorrect format";
            }

            var ipArray = entry.Text.Split('.');
            if (ipArray.Any(subIp => subIp.Length > 3))
            {
                SetPlaceholderColor(entry, Color.Red, Color.Red);
                return "\n\u2022" + interfaceName + " incorrect format";
            }

            SetPlaceholderColor(entry, Color.Navy, Color.Black);
            return string.Empty;
        }

        private string ValidatePortTextChange(EntryOutlined entry, string interfaceName = "")
        {
            if (!string.IsNullOrEmpty(entry.Text) && Convert.ToInt32(entry.Text) > 65535)
            {
                SetPlaceholderColor(entry, Color.Red, Color.Red);
                return "\n\u2022" + interfaceName + " incorrect format";
            }

            SetPlaceholderColor(entry, Color.Navy, Color.Black);
            return string.Empty;
        }

        private void SetPlaceholderColor(EntryOutlined entry, Color placeholderColor, Color borderColor)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                entry.PlaceholderColor = placeholderColor;
                entry.BorderColor = borderColor;
            });
        }

        private void IpEntry_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (EntryOutlined)sender;
            ValidateIpTextChange(entry, e.NewTextValue);
        }

        private void PortEntry_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var allowedCharacters = new List<char> { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            var entryOutline = (EntryOutlined)sender;
            var invalidChar = e.NewTextValue?.Where(charValue => !allowedCharacters.Contains(charValue));
            if (invalidChar != null && invalidChar.Any())
                entryOutline.Text = e.OldTextValue;
            ValidatePortTextChange(entryOutline);
        }

        private async void Button_OnPressed_Update(object sender, EventArgs e)
        {
            var notification = Validation();
            if (string.IsNullOrEmpty(notification) == false)
            {
                await DisplayAlert("Setup", notification, "Understood");
                return;
            }

            var irrigationConfiguration = _keyValueIrrigation.Key;
            irrigationConfiguration.ConnectionType = ConnectionTypePicker.SelectedIndex;

            if (InternalIpEntry.Text != irrigationConfiguration.InternalPath)
                irrigationConfiguration.InternalPath = InternalIpEntry.Text;

            if (InternalPortEntry.Text != irrigationConfiguration.InternalPort.ToString())
                irrigationConfiguration.InternalPort = StringToInt(InternalPortEntry.Text);

            if (ExternalIpEntry.Text != irrigationConfiguration.ExternalPath)
                irrigationConfiguration.ExternalPath = ExternalIpEntry.Text;

            if (ExternalPortEntry.Text != irrigationConfiguration.ExternalPort.ToString())
                irrigationConfiguration.ExternalPort = StringToInt(ExternalPortEntry.Text);

            foreach (var view in SiteLayout.Children)
            {
                var viewSite = (ViewSiteSummary)view;
                var keyPair = viewSite.GetKeyValuePair();
                var newSiteName = viewSite.GetSiteNameEntry().Text;
                if (newSiteName != keyPair.Key)
                {
                    irrigationConfiguration.ControllerPairs.Add(newSiteName, keyPair.Value);
                    irrigationConfiguration.ControllerPairs.Remove(keyPair.Key);
                }
            }

            var loadingScreen = new PopupLoading { CloseWhenBackgroundIsClicked = false };
            await PopupNavigation.Instance.PushAsync(loadingScreen);

            //Force Firebase
            var result = await _socketPicker.UpdateIrrigationConfig(irrigationConfiguration);
            if (result)
            {
                _mainPage.UpdateSiteNames(irrigationConfiguration);
            }
            //_mainPage.
            await PopupNavigation.Instance.PopAllAsync();
            await Navigation.PopModalAsync();
        }

        private async void Button_OnPressed_Back(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private static int? StringToInt(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            return Convert.ToInt32(value);
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
                    await blueToothManager.ConnectToKnownDevice(new Guid(_keyValueIrrigation.Key.DeviceGuid), new CancellationToken(), 3);
                    await blueToothManager.IsValidController();
                    await PopupNavigation.Instance.PopAllAsync();
                    
                    await Navigation.PushModalAsync(new SetupSystem(blueToothManager, new NotificationEvent(), null, true));
                }
                catch(Exception exception)
                {
                    await PopupNavigation.Instance.PopAllAsync();
                    await DisplayAlert("Connect Exception!", exception.Message, "Understood");
                }
            });
        }
    }
}