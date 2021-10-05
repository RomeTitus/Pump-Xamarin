using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pump.Class;
using Pump.Database;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Pump.SocketController.BT;
using Pump.SocketController.Firebase;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    //DisplayPromptAsync
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetupSystem : ContentPage
    {
        private readonly BluetoothManager _blueToothManage;
        private readonly NotificationEvent _notificationEvent;
        private List<PumpConnection> _controllerList = new List<PumpConnection>();
        private WiFiContainer _selectedWiFiContainer;
        private ViewBasicAlert _viewBasicAlert;
        private List<WiFiContainer> _wiFiContainers;

        public SetupSystem(BluetoothManager blueToothManager, NotificationEvent notificationEvent)
        {
            InitializeComponent();
            _blueToothManage = blueToothManager;
            _notificationEvent = notificationEvent;
            _notificationEvent.OnUpdateStatus += NotificationEventOnNewNotification;
            IsMain.CheckedChanged += IsMain_CheckedChanged;
            PopulateControllers();
            Task.FromResult(GetConnectionInfo());
        }


        private void IsMain_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var isMain = (CheckBox)sender;
            PairGrid.IsVisible = !isMain.IsChecked;
            ButtonCreate.Text = isMain.IsChecked ? "Create" : "Pair";
            LabelControllerName.Text = isMain.IsChecked ? "Controller Name" : "SubController Name";
        }

        private async void WiFiLabel_OnTapped(object sender, EventArgs e)
        {
            try
            {
                var loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
                await PopupNavigation.Instance.PushAsync(loadingScreen);
                var result = await ScanWiFi();
                await PopupNavigation.Instance.PopAllAsync();

                if (SocketExceptions.CheckException(result))
                {
                    await DisplayAlert("WIFI", "We ran into a problem \n" + result, "Understood");
                    return;
                }

                _wiFiContainers = DictToWiFiContainer(result);
                var wiFiView = new ViewAvailableWiFi(_wiFiContainers);
                await GeneratePopupScreen(new List<object> { wiFiView });

                foreach (var views in wiFiView.GetChildren())
                {
                    var wifiView = (ViewWiFi)views;
                    wifiView.GetGestureRecognizer().Tapped += WiFiView_Tapped;
                }
            }
            catch (Exception exception)
            {
                await PopupNavigation.Instance.PopAllAsync();
                await DisplayAlert("Bluetooth Exception", exception.ToString(), "Understood");
            }
        }

        private async void WiFiView_Tapped(object sender, EventArgs e)
        {
            try
            {
                var tapGestureRecognizerWiFi = (StackLayout)sender;
                _selectedWiFiContainer =
                    _wiFiContainers.FirstOrDefault(x => x?.ssid == tapGestureRecognizerWiFi.AutomationId);

                if (_selectedWiFiContainer != null)
                {
                    if (string.IsNullOrEmpty(_selectedWiFiContainer.encryption_type))
                        _viewBasicAlert = new ViewBasicAlert("WIFI",
                            "Are you use you want to connect to " + _selectedWiFiContainer.ssid, "Connect",
                            "Cancel");
                    else
                    {
                        _viewBasicAlert = new ViewBasicAlert("WIFI",
                            "Are you use you want to connect to " + _selectedWiFiContainer.ssid + "\nEnter Password",
                            "Connect",
                            "Cancel", true);
                    }

                    _viewBasicAlert.GetAcceptButton().Clicked += WiFiPassword_Clicked;
                    await GeneratePopupScreen(new List<object> { _viewBasicAlert }, 400);
                }
                else
                    await DisplayAlert("WIFI", "We could not find that WiFi Details", "Understood");
            }
            catch (Exception exception)
            {
                await DisplayAlert("WIFI", "We ran into a problem \n" + exception.Message, "Understood");
            }
        }

        private async void WiFiPassword_Clicked(object sender, EventArgs e)
        {
            if (_viewBasicAlert.Editable && !string.IsNullOrEmpty(_viewBasicAlert.GetEditableText()))
                _selectedWiFiContainer.passkey = _viewBasicAlert.GetEditableText();

            //LabelWiFi.Text = _selectedWiFiContainer.ssid;

            await PopupNavigation.Instance.PopAllAsync();

            var loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
            await PopupNavigation.Instance.PushAsync(loadingScreen);
            var result = await ConnectToWifi(_selectedWiFiContainer);
            await PopupNavigation.Instance.PopAllAsync();
            await GetConnectionInfo(result);
        }

        private List<WiFiContainer> DictToWiFiContainer(string Dict)
        {
            var wiFiList = new List<WiFiContainer>();
            if (Dict == "None") return wiFiList;
            var wiFiObject = JObject.Parse(Dict);

            wiFiList.AddRange(wiFiObject["Networks"].Select(wiFiInfo =>
                new WiFiContainer
                {
                    encryption_type = wiFiInfo["encryption_type"].ToString(), signal = wiFiInfo["signal"].ToString(),
                    ssid = wiFiInfo["ssid"].ToString()
                }));
            return wiFiList;
        }

        private async Task<string> ScanWiFi()
        {
            return await _blueToothManage.SendAndReceiveToBle(SocketCommands.WiFiScan());
        }

        private async Task<string> ConnectToWifi(WiFiContainer wiFiContainer)
        {
            var wiFiJson = JObject.FromObject(wiFiContainer);
            return await _blueToothManage.SendAndReceiveToBle(SocketCommands.WiFiConnect(wiFiJson), 8000);
        }

        private void PopulateControllers()
        {
            _controllerList = new DatabaseController().GetControllerConnectionList();
            ControllerPicker.Items.Clear();
            var selectedController = new DatabaseController().GetControllerConnectionSelection();
            for (var i = 0; i < _controllerList.Count; i++)
            {
                ControllerPicker.Items.Add(string.IsNullOrEmpty(_controllerList[i].Name)
                    ? "Name is missing"
                    : _controllerList[i].Name);
                if (_controllerList[i]?.ID == selectedController?.ID)
                    ControllerPicker.SelectedIndex = i;
            }

            if (!_controllerList.Any())
            {
                IsMain.IsEnabled = false;
            }
        }

        private async Task GetConnectionInfo(string result = null)
        {
            try
            {
                if (result == null)
                    result = await _blueToothManage.SendAndReceiveToBle(SocketCommands.ConnectionInfo());
                var networkDetailObject = JObject.Parse(result);
                LabelIP.IsVisible = true;
                foreach (var networkDetail in networkDetailObject)
                {
                    var networkType = networkDetail.Key;
                    var networkIpAddress = networkDetail.Value.Value<string>();
                    string networkInfo;
                    if (networkIpAddress.Contains("\n"))
                    {
                        LabelWiFi.Text = networkIpAddress.Split('\n').First();

                        networkInfo = networkType + " - " + networkIpAddress.Split('\n').Last();
                    }
                    else
                    {
                        networkInfo = networkType + " - " + networkIpAddress;
                    }

                    if (LabelIP.Text != null && LabelIP.Text.Any())
                        LabelIP.Text = LabelIP.Text + "\n";
                    LabelIP.Text = LabelIP.Text + networkInfo;
                }
            }
            finally
            {
                SetUpSystemActivityIndicator.IsVisible = false;
            }
        }

        private async Task<string> SendUid(string uid)
        {
            return await _blueToothManage.SendAndReceiveToBle(SocketCommands.FirebaseUid(uid));
        }

        private async void WiFiIpLabel_OnTapped(object sender, EventArgs e)
        {
            await DisplayAlert("Network", "Change IP not supported yet!", "Understood");
        }

        private async void ButtonCreate_OnClicked(object sender, EventArgs e)
        {
            var notifiction = Validation();
            if (!string.IsNullOrEmpty(notifiction))
            {
                await DisplayAlert("Setup", notifiction, "Understood");
                return;
            }

            if (IsMain.IsChecked)
            {
                _viewBasicAlert = new ViewBasicAlert("Setup",
                    "You are creating a new Controller \nTo prevent someone from Pickpocketing  \nEnter a good Password",
                    "Please re-enter your password  \nEnter Password",
                    "Connect",
                    "Cancel", true);
                _viewBasicAlert.GetAcceptButton().Clicked += SetUpMainController_Clicked;
                await GeneratePopupScreen(new List<object> { _viewBasicAlert }, 400);
            }
            else
            {
                SetUpSubController();
            }
        }

        private async void SetUpMainController_Clicked(object sender, EventArgs e)
        {
            if (_viewBasicAlert.GetEditableText() != _viewBasicAlert.GetSubEditableText())
                await DisplayAlert("Setup", "Passwords does not match", "Understood");
            else
            {
                var result = await SendUid(_viewBasicAlert.GetEditableText());
                await PopupNavigation.Instance.PopAllAsync();
                var pumpController = new PumpConnection
                    { Name = TxtControllerName.Text, Mac = _blueToothManage.BleDevice.NativeDevice.ToString() };
                if (!string.IsNullOrEmpty(LabelIP.Text))
                {
                    pumpController.InternalPath = LabelIP.Text;
                    pumpController.InternalPort = 8080;
                }

                new DatabaseController().UpdateControllerConnection(pumpController);
                await DisplayAlert("Setup", result, "Understood");
                _notificationEvent.UpdateStatus();
            }
        }

        private async void SetUpSubController()
        {
            var pumpConnection = _controllerList[ControllerPicker.SelectedIndex];
            var randomKey = new Random().Next(10, 100);

            var irrigationSelf = await new Authentication(pumpConnection.Mac).GetConnectedControllerInfo();

            var mainController = new SubController
            {
                BTmac = _blueToothManage.BleDevice.NativeDevice.ToString(),
                NAME = TxtControllerName.Text,
                IncomingKey = randomKey,
                OutgoingKey = new List<int> { randomKey },
                IpAdress = LabelIP.Text,
                Port = 8080,
                UseLoRa = !IsMain.IsChecked
            };

            var subControllerId =
                await new Authentication(pumpConnection.Mac).SetSubController(mainController, new NotificationEvent());

            var subController = new SubController
            {
                BTmac = irrigationSelf.BTmac,
                Port = irrigationSelf.Port,
                IncomingKey = randomKey,
                OutgoingKey = new List<int> { randomKey },
                NAME = "Main Controller",
                IpAdress = irrigationSelf.IpAdress,
                UseLoRa = !IsMain.IsChecked,
                ID = subControllerId
            };

            var result =
                await _blueToothManage.SendAndReceiveToBle(
                    SocketCommands.SetupSubController(subController, subControllerId));

            await DisplayAlert("Setup", result, "Understood");


            _notificationEvent.UpdateStatus();
        }

        private static async Task GeneratePopupScreen(IEnumerable<object> screenViews, double hightRequest = 600)
        {
            var floatingScreen = new FloatingScreenScroll(hightRequest);
            floatingScreen.SetFloatingScreen(screenViews);
            await PopupNavigation.Instance.PushAsync(floatingScreen);
        }

        private string Validation()
        {
            var notification = "";

            if (string.IsNullOrEmpty(TxtControllerName.Text))
            {
                if (notification.Length < 1)
                    notification = "\u2022 Controller name required";
                else
                    notification += "\n\u2022 Controller name required";
                LabelControllerName.TextColor = Color.Red;
            }

            return notification;
        }

        private async void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private async void NotificationEventOnNewNotification(object sender, ControllerEventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}