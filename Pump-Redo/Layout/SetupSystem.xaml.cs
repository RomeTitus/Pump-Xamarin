using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EmbeddedImages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pump.Class;
using Pump.Database;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Pump.SocketController.BT;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetupSystem : ContentPage
    {
        private readonly BluetoothManager _blueToothManage;
        private WiFiContainer _selectedWiFiContainer;
        private List<WiFiContainer> _wiFiContainers;
        private readonly List<DHCPConfig> _dhcpConfigList = new List<DHCPConfig>();
        private readonly NotificationEvent _notificationEvent;
        private PopupDHCPConfig _popupDhcpConfig;
        
        public SetupSystem(BluetoothManager blueToothManager, NotificationEvent notificationEvent)
        {
            InitializeComponent();
            _blueToothManage = blueToothManager;
            _notificationEvent = notificationEvent;
            _notificationEvent.OnUpdateStatus += NotificationEventOnNewNotification;
            GetConnectionInfo();
        }

        private async void GetConnectionInfo(string connection = null)
        {
            WiFiDhcp.IsVisible = false;
            LanDhcp.IsVisible = false;

            if (string.IsNullOrEmpty(connection))
            {
                GridNetworkDetail.IsVisible = false;
                SetUpSystemActivityIndicator.IsVisible = true;
                connection = await _blueToothManage.SendAndReceiveToBleAsync(SocketCommands.ConnectionInfo());
                SetUpSystemActivityIndicator.IsVisible = false;
                GridNetworkDetail.IsVisible = true;

            }

            var connectionInfoJObject = JObject.Parse(connection);

            foreach (var connectionInfo in connectionInfoJObject)
            {
                if (connectionInfo.Key.Contains("eth"))
                {
                    LanDhcp.ClassId = connectionInfo.Key.ToLower() + "/" + connectionInfo.Value;
                    LabelLanIp.Text = "IP: " + connectionInfo.Value;
                    LanDhcp.IsVisible = true;
                }
                else if(connectionInfo.Key.Contains("wlan"))
                {
                    var wifiDetailList = connectionInfo.Value.ToString().Split('\n');
                    if (wifiDetailList.Length == 3)
                    {
                        var signal = int.Parse(wifiDetailList.Last().Replace(" dBm", "").Replace("-", ""));
                        SetSignalStrength(SignalImage, signal);
                    }

                    LabelWiFiName.Text = wifiDetailList.First();
                    LabelWiFiIp.Text = "IP: " + wifiDetailList[1];
                    WiFiDhcp.ClassId = connectionInfo.Key.ToLower()  + "/" + wifiDetailList[1];
                    WiFiDhcp.IsVisible = true;
                }else if (connectionInfo.Key.Contains("DHCP"))
                {
                    _dhcpConfigList.Clear();
                    foreach (var dhcpConfig in connectionInfo.Value)
                    {
                        var config = JsonConvert.DeserializeObject<DHCPConfig>(dhcpConfig.First.ToString());
                        config.DHCPinterface = dhcpConfig.Path.Replace("DHCP.", "");
                        _dhcpConfigList.Add(config);
                    }
                }
            }
        }

        private static void SetSignalStrength(Image image, int dBm)
        {
            string signalStrength;
                    
            if (dBm < 53)
                signalStrength = "5";

            else if (dBm < 60 )
                signalStrength = "4";
                    
            else if (dBm < 68 )
                signalStrength = "3";
                    
            else if (dBm < 77 )
                signalStrength = "3";
                    
            else if (dBm < 81 )
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
                var result = await ScanWiFi();
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
        
        private async void SetDHCPInterface_OnTapped(object sender, EventArgs e)
        {
            var networkLabel = (Label)sender;
            _popupDhcpConfig = new PopupDHCPConfig(
                networkLabel.ClassId.Split('/').ToList(),
                _dhcpConfigList.FirstOrDefault(x => x.DHCPinterface.Contains(networkLabel.ClassId.Split('/').First()))); 
            await PopupNavigation.Instance.PushAsync(_popupDhcpConfig);
            _popupDhcpConfig.GetSaveButtonDhcpSaveButton().Pressed +=OnPressed;
            
        }

        private async void OnPressed(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAllAsync();
            var dhcpConfig = _popupDhcpConfig.GetDhcpConfig();
            var dhcpConfigSerialize = JObject.Parse(JsonConvert.SerializeObject(dhcpConfig));
            
            if (string.IsNullOrEmpty(dhcpConfigSerialize["ip_address"].ToString()) &&
                string.IsNullOrEmpty(dhcpConfigSerialize["routers"].ToString()) &&
                string.IsNullOrEmpty(dhcpConfigSerialize["domain_name_servers"].ToString()))
            {
                dhcpConfigSerialize.Remove("ip_address");
                dhcpConfigSerialize.Remove("routers");
                dhcpConfigSerialize.Remove("domain_name_servers");
            }
            var loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
                await PopupNavigation.Instance.PushAsync(loadingScreen);
                var result = await _blueToothManage.SendAndReceiveToBleAsync(SocketCommands.TempDhcpConfig(dhcpConfigSerialize), 8000);
                await PopupNavigation.Instance.PopAllAsync();
                GetConnectionInfo(result);
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
                    if (_selectedWiFiContainer.encryption_type.Contains("Encryption: on"))
                    {
                        bool passwordValidationFailed;
                        do
                        {
                            var password = await DisplayPromptAsync("WiFi", "Connect to " + _selectedWiFiContainer.ssid + "\nEnter WiFi Password");
                            if(password == null)
                                return;
                            passwordValidationFailed = password.Length < 7;
                            if(passwordValidationFailed)
                                await DisplayAlert("Validation", "\u2022 Password too short", "Understood");
                            _selectedWiFiContainer.passkey = password;
                        } while (passwordValidationFailed);
                    }
                    else
                        await DisplayAlert("WiFi", "Connect to " + _selectedWiFiContainer.ssid, "");
                    
                    await PopupNavigation.Instance.PopAllAsync();
                    var loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
                    await PopupNavigation.Instance.PushAsync(loadingScreen);
                    var result = await ConnectToWifi(_selectedWiFiContainer);
                    await PopupNavigation.Instance.PopAllAsync();
                    GetConnectionInfo(result);

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
            return await _blueToothManage.SendAndReceiveToBleAsync(SocketCommands.WiFiConnect(wiFiJson), 8000);
        }

        private async void ButtonCreate_OnClicked(object sender, EventArgs e)
        {
            var notification = Validation();
            if (!string.IsNullOrEmpty(notification))
            {
                await DisplayAlert("Setup", notification, "Understood");
                return;
            }

            var controllerConfig = new JObject();
            controllerConfig["UID"] = JObject.Parse(new DatabaseController().GetUserAuthentication().UserInfo)["Uid"].ToString();
            controllerConfig["Path"] = TxtControllerName.Text.Replace(" ", "_");
            controllerConfig["IsMain"] = true;
            var loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
            await PopupNavigation.Instance.PushAsync(loadingScreen);
            var result = await _blueToothManage.SendAndReceiveToBleAsync(SocketCommands.SetupFirebaseController(controllerConfig), 8000);
            await PopupNavigation.Instance.PopAllAsync();

            if(result == null)
                return;
            await DisplayAlert("Info", result, "Understood");

            if (result != "Already_Exist")
            {
                _notificationEvent.UpdateStatus();
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