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
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Pump.SocketController.BT;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using JObject = Newtonsoft.Json.Linq.JObject;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SetupSystem
    {
        private readonly BluetoothManager _blueToothManager;
        private readonly List<DHCPConfig> _dhcpConfigList = new List<DHCPConfig>();
        private readonly NotificationEvent _notificationEvent;
        private readonly DatabaseController _database;
        private PopupDHCPConfig _popupDhcpConfig;
        private WiFiContainer _selectedWiFiContainer;
        private List<WiFiContainer> _wiFiContainers;
        private readonly MainPage _mainPage;

        public SetupSystem(BluetoothManager blueToothManager, NotificationEvent notificationEvent, MainPage mainPage)
        {
            InitializeComponent();
            _blueToothManager = blueToothManager;
            _mainPage = mainPage;
            _notificationEvent = notificationEvent;
            _notificationEvent.OnUpdateStatus += NotificationEventOnNewNotification;
            _database = new DatabaseController();
         
            GetControllerInfo();
        }

        private void SetControllerStatus()
        {
            StackLayoutControllerName.IsVisible = false;
            ButtonCreate.IsVisible = false;
            StackLayoutLoRaConfig.IsVisible = true;
        }

        private void PopulateSubController()
        {
            SubControllerStackLayout.IsVisible = true;
            if (!_mainPage.ObservableDict.Any()) return;

            SubControllerCheckbox.IsEnabled = true;
            SubControllerCheckbox.Color = Color.Crimson;

            foreach (var irrigationConfig in _mainPage.ObservableDict.Keys)
            {
                ControllerPicker.Items.Add(irrigationConfig.Path);
            }
        }

        private async void GetControllerInfo(string controllerInfo = null)
        {
            WiFiDhcp.IsVisible = false;
            LanDhcp.IsVisible = false;

            if (string.IsNullOrEmpty(controllerInfo))
            {
                GridNetworkDetail.IsVisible = false;
                SetUpSystemActivityIndicator.IsVisible = true;
                controllerInfo = await _blueToothManager.SendAndReceiveToBleAsync(SocketCommands.ControllerInfo());
                SetUpSystemActivityIndicator.IsVisible = false;
                GridNetworkDetail.IsVisible = true;
            }

            var controllerInfoJObject = JObject.Parse(controllerInfo);
            var connectionInfoJObject = (JObject) controllerInfoJObject["Connection"];

            foreach (var connectionInfo in connectionInfoJObject)
                if (connectionInfo.Key.Contains("eth"))
                {
                    LanDhcp.ClassId = connectionInfo.Key.ToLower() + "/" + connectionInfo.Value;
                    LabelLanIp.Text = "IP: " + connectionInfo.Value;
                    LanDhcp.IsVisible = true;
                }
                else if (connectionInfo.Key.Contains("wlan"))
                {
                    var wifiDetailList = connectionInfo.Value.ToString().Split('\n');
                    if (wifiDetailList.Length == 3)
                    {
                        var signal = int.Parse(wifiDetailList.Last().Replace(" dBm", "").Replace("-", ""));
                        SetSignalStrength(SignalImage, signal);
                    }

                    LabelWiFiName.Text = wifiDetailList.First();
                    LabelWiFiIp.Text = "IP: " + wifiDetailList[1];
                    WiFiDhcp.ClassId = connectionInfo.Key.ToLower() + "/" + wifiDetailList[1];
                    WiFiDhcp.IsVisible = true;
                }
                else if (connectionInfo.Key.Contains("DHCP"))
                {
                    _dhcpConfigList.Clear();
                    foreach (var dhcpConfig in connectionInfo.Value)
                    {
                        var config = JsonConvert.DeserializeObject<DHCPConfig>(dhcpConfig.First.ToString());
                        config.DHCPinterface = dhcpConfig.Path.Replace("DHCP.", "");
                        _dhcpConfigList.Add(config);
                    }
                }

            if (controllerInfoJObject.ContainsKey("Config"))
            {

                SetControllerStatus();
                var controllerConf = (JObject)controllerInfoJObject["Config"];
                if (controllerConf["IsMain"].Value<bool>() == false)
                {
                    foreach (var keyPairValue in _mainPage.ObservableDict)
                    {
                        var subFound = keyPairValue.Value.SubControllerList.FirstOrDefault(x => x.KeyPath.First() == controllerConf["Address"].Value<int>());
                        if(subFound != null)
                        {
                            SubControllerStackLayout.IsVisible = true;
                            SubControllerCheckbox.IsChecked= true;
                            AddSiteLabel.IsVisible = false;

                            ControllerPicker.Items.Add(keyPairValue.Key.Path);
                            ControllerPicker.SelectedIndex = 0;

                            SiteLayout.Children.ForEach(x => x.IsEnabled = false);
                            SiteLayout.Children.First(x => ((Label)((Frame)x).Content).Text == keyPairValue.Key.ControllerPairs.First(x => x.Value.Contains(subFound.Id)).Key).BackgroundColor = Color.LightCyan;

                            CheckBoxLoRa.IsChecked = subFound.UseLoRa;
                            CheckBoxLoRa.IsEnabled = false;
                            CheckBoxLoRa.Color = Color.Gray;
                        }
                    }
                }
            }
            else
            {
                PopulateSubController();
            }
        }

        private static void SetSignalStrength(Image image, int dBm)
        {
            string signalStrength;

            if (dBm < 53)
                signalStrength = "5";

            else if (dBm < 60)
                signalStrength = "4";

            else if (dBm < 68)
                signalStrength = "3";

            else if (dBm < 77)
                signalStrength = "3";

            else if (dBm < 81)
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
                var loadingScreen = new PopupLoading { CloseWhenBackgroundIsClicked = false };
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
            _popupDhcpConfig.GetSaveButtonDhcpSaveButton().Pressed += OnPressed;
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

            var loadingScreen = new PopupLoading { CloseWhenBackgroundIsClicked = false };
            await PopupNavigation.Instance.PushAsync(loadingScreen);
            var result =
                await _blueToothManager.SendAndReceiveToBleAsync(SocketCommands.TempDhcpConfig(dhcpConfigSerialize),
                    8000);
            await PopupNavigation.Instance.PopAllAsync();
            GetControllerInfo(result);
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
                            var password = await DisplayPromptAsync("WiFi",
                                "Connect to " + _selectedWiFiContainer.ssid + "\nEnter WiFi Password");
                            if (password == null)
                                return;
                            passwordValidationFailed = password.Length < 7;
                            if (passwordValidationFailed)
                                await DisplayAlert("Validation", "\u2022 Password too short", "Understood");
                            _selectedWiFiContainer.passkey = password;
                        } while (passwordValidationFailed);
                    }
                    else
                    {
                        await DisplayAlert("WiFi", "Connect to " + _selectedWiFiContainer.ssid, "");
                    }

                    await PopupNavigation.Instance.PopAllAsync();
                    var loadingScreen = new PopupLoading { CloseWhenBackgroundIsClicked = false };
                    await PopupNavigation.Instance.PushAsync(loadingScreen);
                    var result = await ConnectToWifi(_selectedWiFiContainer);
                    await PopupNavigation.Instance.PopAllAsync();
                    GetControllerInfo(result);
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
            return await _blueToothManager.SendAndReceiveToBleAsync(SocketCommands.WiFiScan());
        }

        private async Task<string> ConnectToWifi(WiFiContainer wiFiContainer)
        {
            var wiFiJson = JObject.FromObject(wiFiContainer);
            return await _blueToothManager.SendAndReceiveToBleAsync(SocketCommands.WiFiConnect(wiFiJson), 8000);
        }

        private string Validation()
        {
            var notification = "";
            if (string.IsNullOrEmpty(TxtControllerName.Text))
            {
                notification += "\n\u2022 Controller name required";
                LabelControllerName.TextColor = Color.Red;
            }

            if (SubControllerCheckbox.IsChecked == false)
                return notification;

            if (SiteLayout.Children.FirstOrDefault(x => x.BackgroundColor == Color.LightCyan) is null)
            {
                notification += "\n\u2022 Controller Site is required to add the SubController";
                SiteLayout.Children.ForEach(x => ((Frame)x).BorderColor = Color.Red);
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

        private void SubControllerCheckbox_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            PairSubMainStackLayout.IsVisible = checkBox.IsChecked;

            if (checkBox.IsChecked)
            {
                ControllerPicker.SelectedIndex = _mainPage.ObservableDict.Any() ? 0 : -1;
                ButtonCreate.Text = "Pair";
            }
            else
            {
                ControllerPicker.SelectedIndex = -1;
                ButtonCreate.Text = "Create";
            }
        }

        private void ControllerPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            SiteLayout.Children.Clear();
            var picker = (Picker)sender;
            if (picker.SelectedIndex == -1)
                return;
            foreach (var siteName in _mainPage.ObservableDict.Keys.ToList()[picker.SelectedIndex]
                         .ControllerPairs.Keys)
            {
                var tapGestureRecognizer = new TapGestureRecognizer();
                var frame = new Frame
                {
                    BackgroundColor = Color.White,
                    BorderColor = Color.Black,
                    CornerRadius = 10,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.Fill,
                    GestureRecognizers = { tapGestureRecognizer },
                    Content = new Label { Text = siteName, FontSize = 20 }
                };
                tapGestureRecognizer.Tapped += PairControllerToSiteTapGesture;
                SiteLayout.Children.Add(frame);
            }
            var mainControllerConfig =
                _mainPage.ObservableDict.Keys.First(x => x.Path == ControllerPicker.SelectedItem.ToString());

            CheckBoxLoRa.Color = mainControllerConfig.LoRaSet ? Color.Accent : Color.Gray;
        }

        private void PairControllerToSiteTapGesture(object sender, EventArgs e)
        {
            var view = (Frame)sender;
            view.BackgroundColor = Color.LightCyan;

            foreach (var siteView in SiteLayout.Children.Where(x => x != view))
            {
                siteView.BackgroundColor = Color.White;
            }
        }

        private async void AddNewSite_OnTapped(object sender, EventArgs e)
        {
            var siteName = await DisplayPromptAsync("New Site",
                "Enter the site name");

            var existingTempView = (Frame)SiteLayout.Children.FirstOrDefault(x => x.AutomationId == "Temp");
            if (existingTempView is not null)
            {
                existingTempView.Content = new Label { Text = siteName, FontSize = 20 };
                return;
            }

            var tapGestureRecognizer = new TapGestureRecognizer();
            var frame = new Frame
            {
                BackgroundColor = Color.White,
                BorderColor = Color.Black,
                CornerRadius = 10,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Fill,
                GestureRecognizers = { tapGestureRecognizer },
                Content = new Label { Text = siteName, FontSize = 20 },
                AutomationId = "Temp"
            };
            tapGestureRecognizer.Tapped += PairControllerToSiteTapGesture;
            SiteLayout.Children.Add(frame);
            PairControllerToSiteTapGesture(frame, EventArgs.Empty);
            await Task.Delay(100);
            await SiteScroll.ScrollToAsync(frame, ScrollToPosition.MakeVisible, true);
        }

        private async void ButtonCreate_OnClicked(object sender, EventArgs e)
        {
            var notification = Validation();
            if (!string.IsNullOrEmpty(notification))
            {
                await DisplayAlert("Setup", notification, "Understood");
                return;
            }

            if (SubControllerCheckbox.IsChecked)
            {
                await PairToMainController();
            }
            else
                await CreateMainController();
        }

        private async Task CreateMainController()
        {
            var controllerConfig = new JObject();
            controllerConfig["UID"] = JObject.Parse(_database.GetUserAuthentication().UserInfo)["Uid"].ToString();
            controllerConfig["Path"] = TxtControllerName.Text.Replace(" ", "_");
            controllerConfig["IsMain"] = true;
            var loadingScreen = new PopupLoading { CloseWhenBackgroundIsClicked = false };
            await PopupNavigation.Instance.PushAsync(loadingScreen);
            var result =
                await _blueToothManager.SendAndReceiveToBleAsync(
                    SocketCommands.SetupFirebaseController(controllerConfig), 8000);
            await PopupNavigation.Instance.PopAllAsync();

            if (result == null)
                return;

            var irrigationController = JsonConvert.DeserializeObject<IrrigationConfiguration>(result);
            _database.SaveIrrigationConfiguration(irrigationController);

            if (result == "Already_Exist") return;
            _notificationEvent.UpdateStatus();
            _mainPage?.PopulateSavedIrrigation(_database.GetIrrigationConfigurationList());
            _mainPage?.SubscribeToNewController(irrigationController);
        }

        private async Task PairToMainController()
        {
            //LoRaSet Bool field needs to be true
            var mainControllerConfig =
                _mainPage.ObservableDict.Keys.First(x => x.Path == ControllerPicker.SelectedItem.ToString());
            var mainObservableIrrigation = _mainPage.ObservableDict[mainControllerConfig];
            int subAddress = mainControllerConfig.Address + 1;

            if (mainObservableIrrigation.SubControllerList.Any())
            {
                subAddress = mainObservableIrrigation.SubControllerList.Last().KeyPath.First() + 1;
                if (subAddress > 254)
                    subAddress = 0;
            }

            var loadingScreen = new PopupLoading("Pairing with " + mainControllerConfig.Path + "...");
            await PopupNavigation.Instance.PushAsync(loadingScreen);

            var authConfig = new JObject
            {
                ["UID"] = JObject.Parse(_database.GetUserAuthentication().UserInfo)["Uid"].ToString(),
                ["IsMain"] = false
            };
            
            var result = await _blueToothManager.SendAndReceiveToBleAsync(
                    SocketCommands.PairSubController(mainControllerConfig, authConfig, TxtControllerName.Text,
                        new List<int> { subAddress, mainControllerConfig.Address }, CheckBoxLoRa.IsChecked)
                , 8000);
            
            await PopupNavigation.Instance.PopAllAsync();

            //Prompt to Force Pair
            if (result != "ok")
            {
                var force = await DisplayAlert("Paring Failed",
                    result +
                    "\nWould you like to force pair? \n This will pair regardless if the controller can reach " +
                    mainControllerConfig.Path, "Force Pair", "Cancel");
                if(force == false)
                    return;
                
                await PopupNavigation.Instance.PushAsync(loadingScreen);
                
                    await _blueToothManager.SendAndReceiveToBleAsync(
                        SocketCommands.PairSubController(mainControllerConfig, authConfig, TxtControllerName.Text,
                            new List<int> { subAddress, mainControllerConfig.Address }, CheckBoxLoRa.IsChecked, true)
                        , 8000);
                await PopupNavigation.Instance.PopAllAsync();
            } 
            
            loadingScreen = new PopupLoading("Uploading to " + mainControllerConfig.Path + "..");
            await PopupNavigation.Instance.PushAsync(loadingScreen);


            var addressPath = string.Empty;
            
            var lanIp = LabelLanIp.Text.Split(new string[] { "IP: " }, StringSplitOptions.None); //Split(Convert.ToChar("IP: "));
            if (lanIp.Length == 2)
                addressPath = lanIp[1] + ":20002";
            else
            {
                var wifiIp = LabelWiFiIp.Text.Split(new string[] { "IP: " }, StringSplitOptions.None);
                if (wifiIp.Length == 2)
                    addressPath = wifiIp[1] + ":20002";    
            }
            
            //Create SubController
            var newSubController = new SubController
            {
                Name = TxtControllerName.Text,
                DeviceGuid = _blueToothManager.BleDevice.Id.ToString(),
                KeyPath = new List<int> { subAddress, mainControllerConfig.Address },
                UseLoRa = CheckBoxLoRa.IsChecked,
                AddressPath = addressPath
            };

            var key = await _mainPage.SocketPicker.SendCommand(newSubController, mainControllerConfig); // "-NJ9RU7toGoJhpgO5ZGN";

            await PopupNavigation.Instance.PopAllAsync();

            var selectedFrame = (Label) ((Frame) SiteLayout.Children.First(x => ((Frame)x).BackgroundColor == Color.LightCyan)).Content;

            var existingSite =
                mainControllerConfig.ControllerPairs.Any(x => selectedFrame.Text == x.Key);


            if(mainControllerConfig.ControllerPairs.TryGetValue(selectedFrame.Text, out List<string> subControllerIds))
            {
                subControllerIds.Add(key);
            }
            else
            {
                mainControllerConfig.ControllerPairs.Add(selectedFrame.Text, new List<string> { key });
            }
            //We need to save controller config 
            
            await _mainPage.SocketPicker.UpdateIrrigationConfig(mainControllerConfig);

            _database.SaveIrrigationConfiguration(mainControllerConfig);
            _notificationEvent.UpdateStatus();
            _mainPage?.PopulateSavedIrrigation(_database.GetIrrigationConfigurationList());

            _mainPage?.SubscribeToNewController(mainControllerConfig);
            
        }

        private async void OnTapped_MoreConnections(object sender, EventArgs e)
        {
            if (PopupNavigation.Instance.PopupStack.FirstOrDefault(x => x.GetType() == typeof(PopupMoreConnection)) !=
                null)
                return;

            try
            {
                var loadingScreen = new PopupLoading { CloseWhenBackgroundIsClicked = false };
                await PopupNavigation.Instance.PushAsync(loadingScreen);
                var loRaConfig = await _blueToothManager.SendAndReceiveToBleAsync(SocketCommands.GetLoRaConfig());
                await PopupNavigation.Instance.PopAllAsync();

                if (loRaConfig == "None")
                {
                    await DisplayAlert("LoRa", "Failed to communicate with LoRa module", "Understood");
                    return;
                }

                var popupMoreConnection = new PopupMoreConnection(loRaConfig, _blueToothManager);
                await PopupNavigation.Instance.PushAsync(popupMoreConnection);
            }
            catch (Exception exception)
            {
                await PopupNavigation.Instance.PopAllAsync();
                await DisplayAlert("Exception", exception.ToString(), "Understood");
            }
        }

        private void CheckBoxLoRa_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if(CheckBoxLoRa.Color != Color.Gray)
                return;
            CheckBoxLoRa.IsChecked = false;
            DisplayAlert("LoRa", "The LoRa module needs to first be setup on the main controller", "Understood");
            
        }
    }
}