﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Plugin.BLE.Abstractions.Contracts;
using Pump.Class;
using Pump.Database;
using Pump.Database.Table;
using Pump.Layout.Views;
using Pump.SocketController;
using Pump.SocketController.BT;
using Pump.SocketController.Firebase;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using System.Timers;

namespace Pump.Layout
{
    public partial class ExistingController : ContentPage
    {
        private readonly BluetoothManager _bluetoothManager;
        private readonly Timer _timer;
        private int _scanCounter;

        private readonly VerifyConnections _loadingScreen = new VerifyConnections
            { CloseWhenBackgroundIsClicked = false };

        private readonly NotificationEvent _notificationEvent;
        private readonly PumpConnection _pumpConnection = new PumpConnection();
        private bool? _externalConnection;
        private bool? _firebaseConnection;
        private double _height;
        private bool? _internalConnection;
        private string _mac;
        private double _width;

        public ExistingController(bool firstConnection, NotificationEvent notificationEvent,
            BluetoothManager bluetoothManager, PumpConnection pumpConnection = null)
        {
            InitializeComponent();
            _bluetoothManager = bluetoothManager;
            _timer = new Timer(300); // 0.3 seconds
            _timer.Elapsed += ScanTimerEvent;
            _bluetoothManager.AdapterBle.ScanTimeoutElapsed += AdapterBleOnScanTimeoutElapsed;

            BtScan();
            FrameAddSystemTap.Tapped += FrameAddSystemTap_Tapped;
            _notificationEvent = notificationEvent;
            _notificationEvent.OnUpdateStatus += NotificationEventOnNewNotification;

            if (pumpConnection != null)
            {
                _pumpConnection = pumpConnection;
                NewControllerStackLayout.IsVisible = false;
                ConnectionTypePickerStackLayout.IsVisible = true;
                PopulateElements();
                ConnectionPicker.SelectedIndexChanged += ConnectionPickerOnSelectedIndexChanged;
            }
            else
            {
                ConnectionTypePickerStackLayout.IsVisible = false;
                NewControllerStackLayout.IsVisible = true;
            }

            if (firstConnection) return;
            BtnBackAddConnectionScreen.IsVisible = true;
        }

        private void ScanTimerEvent(object sender, ElapsedEventArgs e)
        {
            
            Device.BeginInvokeOnMainThread(() =>
            {
                    switch (_scanCounter % 6)
                {
                    case 0:
                        LabelBtScan.Text = "Scan.";
                        break;
                    case 1:
                        LabelBtScan.Text = "Scan..";
                        break;
                    case 2:
                        LabelBtScan.Text = "Scan...";
                        break;
                    case 3:
                        LabelBtScan.Text = "Scan....";
                        break;
                    case 4:
                        LabelBtScan.Text = "Scan.....";
                        break;
                    case 5:
                        LabelBtScan.Text = "Scan......";
                        break;
                }
                _scanCounter++;
            });
            
        }

        private async void BtScan()
        {
            _bluetoothManager.IrrigationDeviceBt.Clear();
            ScrollViewSetupSystem.Children.Clear();
            _bluetoothManager.IrrigationDeviceBt.CollectionChanged += (_, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                    foreach (IDevice bluetoothDevice in args.NewItems)
                    {
                        var template = ScrollViewSetupSystem.Children.FirstOrDefault(x =>
                            x.AutomationId == bluetoothDevice.Id.ToString());
                        if(template != null)
                            continue;
                        var blueToothView = new ViewBluetoothSummary(bluetoothDevice);
                        blueToothView.GetTapGestureRecognizer().Tapped += BlueToothDeviceTapped;
                        ScrollViewSetupSystem.Children.Add(blueToothView);
                    }
            };
            _scanCounter = 1;
            _timer.Enabled = true;
            await _bluetoothManager.StartScanning(Guid.Empty);
            
        }

        private void AdapterBleOnScanTimeoutElapsed(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                _timer.Enabled = false;
                LabelBtScan.Text = "Rescan    ";
            });
        }

        private async void BlueToothDeviceTapped(object sender, EventArgs e)
        {
            var viewBlueTooth = (StackLayout)sender;
            var blueToothDevice =
                _bluetoothManager.IrrigationDeviceBt.First(x => x?.Id.ToString() == viewBlueTooth.AutomationId);
            if (await DisplayAlert("Connect?", "You have selected to connect to " + blueToothDevice.Name,
                    "Accept", "Cancel"))
                try
                {
                    var loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
                    await PopupNavigation.Instance.PushAsync(loadingScreen);
                    await _bluetoothManager.ConnectToDevice(blueToothDevice, 3);

                    await PopupNavigation.Instance.PopAllAsync();

                    if (!await _bluetoothManager.IsValidController())
                        if (!await DisplayAlert("Irrigation", "Not verified controller", "Continue", "Cancel"))
                            return;

                    await Navigation.PushModalAsync(new SetupSystem(_bluetoothManager, _notificationEvent));
                }

                catch (Exception exception)
                {
                    await PopupNavigation.Instance.PopAllAsync();
                    await DisplayAlert("Connect Exception!", exception.Message, "Understood");
                }
        }

        private void FrameAddSystemTap_Tapped(object sender, EventArgs e)
        {
            AddIrrigationController();
        }

        private void ConnectionPickerOnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (ConnectionPicker.SelectedIndex == -1)
                return;
            _pumpConnection.ConnectionType = ConnectionPicker.SelectedIndex;
            new DatabaseController().UpdateControllerConnection(_pumpConnection);
            //TODO Throw event to change connection Type otherwise user needs to reload application
        }


        private void PopulateElements()
        {
            foreach (var connectionType in _pumpConnection.ConnectionTypeList)
                ConnectionPicker.Items.Add(connectionType);

            ConnectionPicker.SelectedIndex = _pumpConnection.ConnectionType;

            TxtControllerName.Text = _pumpConnection.Name;
            TxtInternalConnection.Text = _pumpConnection.InternalPath;
            TxtInternalPort.Text = _pumpConnection.InternalPort.ToString();
            TxtExternalConnection.Text = _pumpConnection.ExternalPath;
            TxtExternalPort.Text = _pumpConnection.ExternalPort.ToString();
            TxtControllerCode.Text = _pumpConnection.Mac;
            TxtControllerCode.IsEnabled = false;
            BtnAddController.Text = "Update";
        }

        private void AddIrrigationController()
        {
            var notification = AddControllerValidator();

            if (!string.IsNullOrWhiteSpace(notification))
            {
                DisplayAlert("Incomplete", notification, "Understood");
            }
            else
            {
                PopupNavigation.Instance.PushAsync(_loadingScreen);
                _pumpConnection.Name = TxtControllerName.Text;
                if (!string.IsNullOrEmpty(TxtInternalConnection.Text) || !string.IsNullOrEmpty(TxtInternalPort.Text))
                {
                    _internalConnection =
                        CheckSocket(TxtInternalConnection.Text, Convert.ToInt32(TxtInternalPort.Text));
                    if (_internalConnection == true)
                    {
                        _loadingScreen.InternalSuccess();
                        _pumpConnection.InternalPath = TxtInternalConnection.Text;
                        _pumpConnection.InternalPort = Convert.ToInt32(TxtInternalPort.Text);
                        _pumpConnection.Mac = _mac;
                    }

                    else
                    {
                        _loadingScreen.InternalFailed();
                    }
                }

                if (!string.IsNullOrEmpty(TxtExternalConnection.Text) || !string.IsNullOrEmpty(TxtExternalPort.Text))
                {
                    _externalConnection =
                        CheckSocket(TxtInternalConnection.Text, Convert.ToInt32(TxtInternalPort.Text));
                    if (_externalConnection != null)
                    {
                        if (_externalConnection == true)
                        {
                            _loadingScreen.ExternalSuccess();
                            _pumpConnection.ExternalPath = TxtExternalConnection.Text;
                            _pumpConnection.ExternalPort = Convert.ToInt32(TxtExternalPort.Text);
                            _pumpConnection.Mac = _mac;
                        }

                        else
                        {
                            _loadingScreen.ExternalFailed();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(TxtControllerCode.Text))
                {
                    try
                    {
                        _firebaseConnection =
                            Task.Run(() => new Authentication().IrrigationSystemPath(TxtControllerCode.Text)).Result;
                    }
                    catch (Exception e)
                    {
                        _firebaseConnection = false;
                    }

                    if (_firebaseConnection != false)
                    {
                        if (_firebaseConnection == true)
                        {
                            _loadingScreen.FirebaseSuccess();
                            _pumpConnection.RealTimeDatabase = true;
                            _pumpConnection.Mac = TxtControllerCode.Text;
                        }

                        else
                        {
                            _loadingScreen.FirebaseFailed();
                        }
                    }
                }

                _loadingScreen.StopActivityIndicator();
                if (_firebaseConnection == true || _externalConnection == true || _internalConnection == true)
                    AddPumpConnection(_pumpConnection);
            }
        }


        private string AddControllerValidator()
        {
            var notification = "";
            var code = true;
            bool? internalConnection = true;
            bool? externalConnection = true;

            if (string.IsNullOrEmpty(TxtControllerName.Text))
            {
                if (notification.Length < 1)
                    notification = "\u2022 Controller name required";
                else
                    notification += "\n\u2022 Controller name required";
                LabelControllerName.TextColor = Color.Red;
            }

            if (string.IsNullOrEmpty(TxtControllerCode.Text))
                code = false;

            if (string.IsNullOrEmpty(TxtInternalConnection.Text) || string.IsNullOrEmpty(TxtInternalPort.Text))
            {
                if (string.IsNullOrEmpty(TxtInternalConnection.Text) != string.IsNullOrEmpty(TxtInternalPort.Text))
                    internalConnection = null;
                else
                    internalConnection = false;
            }

            if (string.IsNullOrEmpty(TxtExternalConnection.Text) || string.IsNullOrEmpty(TxtExternalPort.Text))
            {
                if (string.IsNullOrEmpty(TxtExternalConnection.Text) != string.IsNullOrEmpty(TxtExternalPort.Text))
                    externalConnection = null;
                else
                    externalConnection = false;
            }

            if (externalConnection != null && internalConnection != null && code && internalConnection != false &&
                externalConnection != false) return notification;

            if (!code && internalConnection == false && externalConnection == false)
            {
                if (notification.Length < 1)
                    notification = "\u2022 Controller Code required";
                else
                    notification += "\n\u2022 Controller Code required";
                LabelControllerCode.TextColor = Color.Red;
            }

            if (internalConnection == null || (!code && internalConnection == false && externalConnection == false))
            {
                if (string.IsNullOrEmpty(TxtInternalConnection.Text))
                {
                    if (notification.Length < 1)
                        notification = "\u2022 Internal Connection required";
                    else
                        notification += "\n\u2022 Internal Connection required";
                    LabelTxtInternalConnection.TextColor = Color.Red;
                }

                if (string.IsNullOrEmpty(TxtInternalPort.Text))
                {
                    if (notification.Length < 1)
                        notification = "\u2022 Internal Port required";
                    else
                        notification += "\n\u2022 Internal Port required";
                    LabelInternalPort.TextColor = Color.Red;
                }
            }

            if (externalConnection == null || (!code && internalConnection == false && externalConnection == false))
            {
                if (string.IsNullOrEmpty(TxtExternalConnection.Text))
                {
                    if (notification.Length < 1)
                        notification = "\u2022 External Connection required";
                    else
                        notification += "\n\u2022 External Connection required";
                    LabelExternalConnection.TextColor = Color.Red;
                }

                if (string.IsNullOrEmpty(TxtExternalPort.Text))
                {
                    if (notification.Length < 1)
                        notification = "\u2022 External Port required";
                    else
                        notification += "\n\u2022 External Port required";
                    LabelExternalPort.TextColor = Color.Red;
                }
            }

            return notification;
        }

        private bool CheckSocket(string host, int port)
        {
            var socket = new SocketVerify(host, port);
            try
            {
                var response = socket.verifyConnection();
                if (response != "getMAC")
                {
                    _mac = response;
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }


        private void AddPumpConnection(PumpConnection pumpConnection)
        {
            var databaseController = new DatabaseController();
            databaseController.UpdateControllerConnection(pumpConnection);
            _notificationEvent.UpdateStatus();
        }


        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width == _width && height == _height) return;
            _width = width;
            _height = height;
            if (width > height)
            {
                LayoutAddController.Direction = FlexDirection.Row;
                LayoutAddController.HeightRequest = 200;
                // landscape
            }
            else
            {
                LayoutAddController.Direction = FlexDirection.Column;
                LayoutAddController.HeightRequest = -1;
                // portrait
            }
        }

        private void BtnBackAddConnectionScreen_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        public TapGestureRecognizer GetUpdateButton()
        {
            return FrameAddSystemTap;
        }

        private async void NotificationEventOnNewNotification(object sender, ControllerEventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private void BtnAdvancedConnectionScreen_OnClicked(object sender, EventArgs e)
        {
            StackLayoutLocalConnection.IsVisible = !StackLayoutLocalConnection.IsVisible;

            if (StackLayoutLocalConnection.IsVisible)
                BtnAdvancedConnectionScreen.Text = "Hide Network";
            else
                BtnAdvancedConnectionScreen.Text = "Show Network";
        }

        private void LabelBTScan_OnTapped(object sender, EventArgs e)
        {
            if(_timer.Enabled == false)
                BtScan();
        }
    }
}