using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pump.Class;
using Pump.Database;
using Pump.Droid.Database.Table;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Pump.SocketController.BT;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    //DisplayPromptAsync
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetupSystem : ContentPage
    {
        private List<PumpConnection> _controllerList = new List<PumpConnection>();
        private readonly BluetoothManager _blueToothManage;
        private List<WiFiContainer> _wiFiContainers;
        private WiFiContainer _selectedWiFiContainer;
        private ViewBasicAlert _viewBasicAlert;
        private readonly ControllerEvent _controllerEvent;
        public SetupSystem(BluetoothManager blueToothManager, ControllerEvent controllerEvent)
        {
            InitializeComponent();
            _blueToothManage = blueToothManager;
            _controllerEvent = controllerEvent;
            _controllerEvent.OnUpdateStatus += _controllerEvent_OnNewController;
            IsMain.CheckedChanged += IsMain_CheckedChanged;
            PopulateControllers();
        }


        private void IsMain_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var isMain = (CheckBox) sender;
            PairGrid.IsVisible = !isMain.IsChecked;
            ButtonCreate.Text = isMain.IsChecked ? "Create" : "Pair";
            LabelControllerName.Text = isMain.IsChecked ? "Controller Name" : "SubController Name";
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
                var wiFiView = new ViewAvailableWiFi(_wiFiContainers);
                await GeneratePopupScreen(new List<object> {wiFiView});
                
                foreach (var views in wiFiView.GetChildren())
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
                _selectedWiFiContainer = _wiFiContainers.FirstOrDefault(x => x.ssid == tapGestureRecognizerWiFi.AutomationId);

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
                    await GeneratePopupScreen(new List<object> {_viewBasicAlert}, 400);
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
            if(_viewBasicAlert.Editable && !string.IsNullOrEmpty(_viewBasicAlert.GetEditableText()))
                _selectedWiFiContainer.passkey = _viewBasicAlert.GetEditableText();

            LabelWiFi.Text = _selectedWiFiContainer.ssid;

            await PopupNavigation.Instance.PopAllAsync();

            var loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
            await PopupNavigation.Instance.PushAsync(loadingScreen);
            var result = await ConnectToWifi(_selectedWiFiContainer);
            await PopupNavigation.Instance.PopAllAsync();
            LabelIP.IsVisible = true;
            LabelIP.Text = result;
        }

        private List<WiFiContainer> DictToWiFiContainer(string Dict)
        {
            var wiFiList = new List<WiFiContainer>();
            if (Dict == "None") return wiFiList;
            var wiFiObject = JObject.Parse(Dict);

            wiFiList.AddRange(wiFiObject["Networks"].Select(wiFiInfo => 
                new WiFiContainer {encryption_type = wiFiInfo["encryption_type"].ToString(), signal = wiFiInfo["signal"].ToString(), ssid = wiFiInfo["ssid"].ToString()}));
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
                ControllerPicker.Items.Add(string.IsNullOrEmpty(_controllerList[i].Name) ? "Name is missing" : _controllerList[i].Name);
                if (_controllerList[i].ID == selectedController.ID)
                    ControllerPicker.SelectedIndex = i;
            }

            if (!_controllerList.Any())
            {
                IsMain.IsEnabled = false;
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
                await GeneratePopupScreen(new List<object>{_viewBasicAlert}, 400);
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
                var pumpController = new PumpConnection {Name = TxtControllerName.Text, Mac = _blueToothManage.BleDevice.NativeDevice.ToString()};
                if (!string.IsNullOrEmpty(LabelIP.Text))
                {
                    pumpController.InternalPath = LabelIP.Text;
                    pumpController.InternalPort = 8080;
                }
                new DatabaseController().UpdateControllerConnection(pumpController);
                await DisplayAlert("Setup", result, "Understood");
                _controllerEvent.NewControllerConnection();
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
                Key = randomKey,
                IpAdress = LabelIP.Text,
                Port = 8080,
                UseLoRa = !IsMain.IsChecked
            };
            
            var subControllerId = await new Authentication(pumpConnection.Mac).SetSubController(mainController);

            var subController = new SubController {
                BTmac = irrigationSelf.BTmac, 
                Port = irrigationSelf.Port, 
                Key = randomKey, 
                NAME = "Main Controller", 
                IpAdress = irrigationSelf.IpAdress,
                UseLoRa = !IsMain.IsChecked,
                ID = subControllerId
            };
            
            var result = await _blueToothManage.SendAndReceiveToBle(SocketCommands.SetupSubController(subController, subControllerId));

            await DisplayAlert("Setup", result, "Understood");

            
            _controllerEvent.NewControllerConnection();
        
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

        private async void _controllerEvent_OnNewController(object sender, ControllerEventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}