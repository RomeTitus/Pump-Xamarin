using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EmbeddedImages;
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
        private PopupBasicAlert _popupBasicAlert;
        private List<WiFiContainer> _wiFiContainers;
        
        public SetupSystem(BluetoothManager blueToothManager, NotificationEvent notificationEvent)
        {
            InitializeComponent();
            _blueToothManage = blueToothManager;
            _notificationEvent = notificationEvent;
            _notificationEvent.OnUpdateStatus += NotificationEventOnNewNotification;
            PopulateControllers();
            Task.FromResult(GetConnectionInfo());
        }

        private async Task GetConnectionInfo()
        {
            GridNetworkDetail.IsVisible = false;
            SetUpSystemActivityIndicator.IsVisible = true;
            var result = await _blueToothManage.SendAndReceiveToBleAsync(SocketCommands.ConnectionInfo());
            SetUpSystemActivityIndicator.IsVisible = false;
            GridNetworkDetail.IsVisible = true;
            var connectionInfoJObject = JObject.Parse(result);

            foreach (var connectionInfo in connectionInfoJObject)
            {
                if (connectionInfo.Key.Contains("eth"))
                {
                    LabelLanIp.Text = "IP: " + connectionInfo.Value; 
                }
                else
                {
                    var wifiDetailList = connectionInfo.Value.ToString().Split('\n');
                    if (wifiDetailList.Length == 3)
                    {
                        var signal = int.Parse(wifiDetailList.Last().Replace(" dBm", "").Replace("-", ""));
                        SetSignalStrength(SignalImage, signal);
                    }

                    LabelWiFiName.Text = wifiDetailList.First();
                    LabelWiFiIp.Text = "IP: " + wifiDetailList[1];    
                }
                
            }
        }

        private static void SetSignalStrength(Image image, int dBm)
        {
            string signalStrength;
                    
            if (dBm < 50)
                signalStrength = "5";

            else if (dBm < 57 )
                signalStrength = "4";
                    
            else if (dBm < 62 )
                signalStrength = "3";
                    
            else if (dBm < 67 )
                signalStrength = "3";
                    
            else if (dBm < 70 )
                signalStrength = "1";

            else
                signalStrength = "NoSignal";

            image.Source = ImageSource.FromResource(
                "Pump.Icons.Signal_" + signalStrength + ".png",
                typeof(ImageResourceExtension).GetTypeInfo().Assembly);
        }
        
        private async void LabelWiFiScan_OnTapped(object sender, EventArgs e)
        {
            try
            {
                var loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
                await PopupNavigation.Instance.PushAsync(loadingScreen);
                var result = await Device.InvokeOnMainThreadAsync(ScanWiFi);
                await PopupNavigation.Instance.PopAllAsync();

                if (SocketExceptions.CheckException(result))
                {
                    await DisplayAlert("WIFI", "We ran into a problem \n" + result, "Understood");
                    return;
                }

                _wiFiContainers = DictToWiFiContainer(result);
                var wiFiView = new PopupAvailableWiFi(_wiFiContainers);
                await PopupNavigation.Instance.PushAsync(wiFiView);

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
                    if (!_selectedWiFiContainer.encryption_type.Contains("Encryption: on"))
                        _popupBasicAlert = new PopupBasicAlert("WIFI",
                            "Are you use you want to connect to " + _selectedWiFiContainer.ssid, "Connect",
                            "Cancel");
                    else
                        _popupBasicAlert = new PopupBasicAlert("WIFI",
                            "Are you use you want to connect to " + _selectedWiFiContainer.ssid + "\nEnter Password",
                            "Connect",
                            "Cancel", true);

                    _popupBasicAlert.GetAcceptButton().Clicked += WiFiPassword_Clicked;
                    await PopupNavigation.Instance.PushAsync(_popupBasicAlert);
                }
                else
                {
                    await DisplayAlert("WIFI", "We could not find that WiFi Details", "Understood");
                }
            }
            catch (Exception exception)
            {
                await DisplayAlert("WIFI", "We ran into a problem \n" + exception.Message, "Understood");
            }
        }

        private async void WiFiPassword_Clicked(object sender, EventArgs e)
        {
            if (_popupBasicAlert.Editable && !string.IsNullOrEmpty(_popupBasicAlert.GetEditableText()))
                _selectedWiFiContainer.passkey = _popupBasicAlert.GetEditableText();
            
            await PopupNavigation.Instance.PopAllAsync();

            var loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
            await PopupNavigation.Instance.PushAsync(loadingScreen);
            var result = await ConnectToWifi(_selectedWiFiContainer);
            await PopupNavigation.Instance.PopAllAsync();
            //await GetConnectionInfo(result);
        }

        private static List<WiFiContainer> DictToWiFiContainer(string dict)
        {
            var wiFiList = new List<WiFiContainer>();
            if (dict == "None") return wiFiList;
            var wiFiObject = JObject.Parse(dict);

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
            return await _blueToothManage.SendAndReceiveToBleAsync(SocketCommands.WiFiScan());
        }

        private async Task<string> ConnectToWifi(WiFiContainer wiFiContainer)
        {
            var wiFiJson = JObject.FromObject(wiFiContainer);
            string result = null;
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                result = await _blueToothManage.SendAndReceiveToBleAsync(SocketCommands.WiFiConnect(wiFiJson), 8000);
            });
            return result;
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

            if (!_controllerList.Any()) IsMain.IsEnabled = false;
        }

        private async Task<string> SendUid(string uid)
        {
            string result = null;
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                result = await  _blueToothManage.SendAndReceiveToBleAsync(SocketCommands.FirebaseUid(uid));
            });
            return result;
        }

        private async void ButtonCreate_OnClicked(object sender, EventArgs e)
        {
            var notification = Validation();
            if (!string.IsNullOrEmpty(notification))
            {
                await DisplayAlert("Setup", notification, "Understood");
            }
        }

        private string Validation()
        {
            var notification = "";
            if (string.IsNullOrEmpty(TxtControllerName.Text))
            {
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