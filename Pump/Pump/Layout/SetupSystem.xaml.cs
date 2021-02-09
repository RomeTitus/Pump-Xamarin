using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pump.Database;
using Pump.Droid.Database.Table;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetupSystem : ContentPage
    {
        private List<PumpConnection> _controllerList = new List<PumpConnection>();
        private BluetoothManager _bluetoothManage;
        private List<WiFiContainer> _wiFiContainers;
        public SetupSystem(BluetoothManager bluetoothManager)
        {
            InitializeComponent();
            _bluetoothManage = bluetoothManager;
            IsMain.CheckedChanged += IsMain_CheckedChanged;
            PopulateControllers();
        }

        private void IsMain_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var isMain = (CheckBox) sender;
            PairGrid.IsVisible = !isMain.IsChecked;
            ButtonCreate.Text = isMain.IsChecked ? "Create" : "Pair";
        }

        private async void WiFiLabel_OnTapped(object sender, EventArgs e)
        {
            var result = "";
            try
            {
                var loadingScreen = new VerifyConnections {CloseWhenBackgroundIsClicked = false};
                await PopupNavigation.Instance.PushAsync(loadingScreen);
                result = await ScanWiFi();
                await PopupNavigation.Instance.PopAllAsync();

                if (SocketExceptions.CheckException(result))
                {
                    await DisplayAlert("WIFI", "We ran into a problem \n" + result, "Understood");
                    return;
                }

                _wiFiContainers = DictToWiFiContainer(result);
                var WiFiView = new ViewAvailableWiFi(_wiFiContainers);
                var floatingScreen = new FloatingScreenScroll();
                floatingScreen.SetFloatingScreen(new List<object> {WiFiView});
                await PopupNavigation.Instance.PushAsync(floatingScreen);


                foreach (var views in WiFiView.GetChildren())
                {
                    var wifiView = (ViewWiFi) views;
                    wifiView.GetGestureRecognizer().Tapped += WiFiView_Tapped;
                }
            }
            catch
            {
                await DisplayAlert("Bluetooth Exception", result, "Connect", "Cancel");
            }
        }

        private async void WiFiView_Tapped(object sender, EventArgs e)
        {
            try
            {
                var tapGestureRecognizerWiFi = (StackLayout)sender;
                var wifiContainer = _wiFiContainers.FirstOrDefault(x => x.ssid == tapGestureRecognizerWiFi.AutomationId);

                if (wifiContainer != null)
                {
                    if (string.IsNullOrEmpty(wifiContainer.encryption_type))
                        await DisplayAlert("WIFI", "Are you use you want to connect to " + wifiContainer.ssid, "Connect", "Cancel");
                    else
                    {
                        var passkey = await DisplayPromptAsync("WIFI", "Are you use you want to connect to " + wifiContainer.ssid + "\nEnter Password", "Connect", "Cancel");
                        Forms9Patch.KeyboardService.Hide();
                        if (string.IsNullOrEmpty(passkey)) return;
                        wifiContainer.passkey = passkey;
                    }
                         
                    LabelWiFi.Text = wifiContainer.ssid;
                    
                    await PopupNavigation.Instance.PopAllAsync();

                    var loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
                    await PopupNavigation.Instance.PushAsync(loadingScreen);
                    var result = await ConnectToWifi(wifiContainer);
                    await PopupNavigation.Instance.PopAllAsync();
                    //await DisplayAlert("WIFI", result, "Understood");
                    LabelIP.IsVisible = true;
                    LabelIP.Text = result;
                }
                else
                    await DisplayAlert("WIFI", "We could not find that WiFi Details", "Understood");
            }
            catch (Exception exception)
            {
                await DisplayAlert("WIFI", "We ran into a problem \n" + exception.Message, "Understood");
            }
        }

        private List<WiFiContainer> DictToWiFiContainer(string Dict)
        {
            var WiFiList = new List<WiFiContainer>();
            if (Dict == "None") return WiFiList;
            //var test1 = JsonConvert.DeserializeObject<Dictionary<string, string>>(Dict);
            var WiFiObject = JObject.Parse(Dict);

            WiFiList.AddRange(WiFiObject["Networks"].Select(WiFiInfo => 
                new WiFiContainer {encryption_type = WiFiInfo["encryption_type"].ToString(), signal = WiFiInfo["signal"].ToString(), ssid = WiFiInfo["ssid"].ToString()}));
            return WiFiList;
        }

        private async Task<string> ScanWiFi()
        {
            return await _bluetoothManage.WriteToBle(SocketCommands.WiFiScan);
        }

        private async Task<string> ConnectToWifi(WiFiContainer wiFiContainer)
        {
            var wiFiJosn = JObject.FromObject(wiFiContainer);
            return await _bluetoothManage.WriteToBle(SocketCommands.WiFiConnect(wiFiJosn), 8000);
        }

        private void PopulateControllers()
        {
            _controllerList = new DatabaseController().GetControllerConnectionList();
            ControllerPicker.Items.Clear();
            var selectedController = new DatabaseController().GetControllerConnectionSelection();
            for (var i = 0; i < _controllerList.Count; i++)
            {
                ControllerPicker.Items.Add(string.IsNullOrEmpty(_controllerList[i].Name) ? "Name is missing" : _controllerList[i].Name);
                if (_controllerList[i].ID == selectedController.ID)
                    ControllerPicker.SelectedIndex = i;
            }
        }

        private async Task<string> SendUid(string uid)
        {
            return await _bluetoothManage.WriteToBle(SocketCommands.FirebaseUID(uid));
        }

        private async void WiFiIpLabel_OnTapped(object sender, EventArgs e)
        {
            await DisplayAlert("Network", "Change IP not supported yet!", "Understood");
        }

        private async void ButtonCreate_OnClicked(object sender, EventArgs e)
        {
            if (IsMain.IsChecked)
            {
                var passkey = await DisplayPromptAsync("Setup", "You are creating a new Controller \nTo prevent someone from Pickpocketing  \nEnter a good Password", "Connect", "Cancel");
                Forms9Patch.KeyboardService.Hide();
                if (string.IsNullOrEmpty(passkey)) return;
                var passkeyConfirmation = await DisplayPromptAsync("Setup", "Please re-enter your password  \nEnter Password", "Connect", "Cancel");
                Forms9Patch.KeyboardService.Hide();
                if (passkey != passkeyConfirmation)
                    await DisplayAlert("Setup", "Passwords does not match", "Understood");
                else
                {
                    var result = await SendUid(passkey);
                    await DisplayAlert("Setup", result, "Understood");
                }
                    

            }
        }
    }
}