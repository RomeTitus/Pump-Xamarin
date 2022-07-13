using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Timers;
using Plugin.BLE.Abstractions.Contracts;
using Pump.Class;
using Pump.Database;
using Pump.Database.Table;
using Pump.Layout.Views;
using Pump.SocketController.BT;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace Pump.Layout
{
    public partial class ScanBluetooth : ContentPage
    {
        private readonly BluetoothManager _bluetoothManager;
        private readonly NotificationEvent _notificationEvent;
        private readonly Timer _timer;
        private readonly List<IrrigationConfiguration> _irrigationConfiguration;
        private int _scanCounter;

        public ScanBluetooth(List<IrrigationConfiguration> irrigationConfiguration, NotificationEvent notificationEvent, BluetoothManager bluetoothManager,
            DatabaseController database)
        {
            InitializeComponent();
            _bluetoothManager = bluetoothManager;
            _irrigationConfiguration = irrigationConfiguration;
            _timer = new Timer(300); // 0.3 seconds
            _timer.Elapsed += ScanTimerEvent;
            _bluetoothManager.AdapterBle.ScanTimeoutElapsed += AdapterBleOnScanTimeoutElapsed;
            StackLayoutBack.IsVisible = database.GetIrrigationConfigurationList().Any();
            BtScan();
            _notificationEvent = notificationEvent;
            _notificationEvent.OnUpdateStatus += NotificationEventOnNewNotification;
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
                        if (template != null)
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
                    var loadingScreen = new PopupLoading { CloseWhenBackgroundIsClicked = false };
                    await PopupNavigation.Instance.PushAsync(loadingScreen);
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        await _bluetoothManager.ConnectToDevice(blueToothDevice, 3);
                    });
                    await PopupNavigation.Instance.PopAllAsync();

                    if (!await _bluetoothManager.IsValidController())
                        if (!await DisplayAlert("Irrigation", "Not verified controller", "Continue", "Cancel"))
                            return;

                    var existing = _irrigationConfiguration.Any(x => new Guid(x.DeviceGuid) == _bluetoothManager.BleDevice.Id);
                    
                    await Navigation.PushModalAsync(new SetupSystem(_bluetoothManager, _notificationEvent, existing));
                }

                catch (Exception exception)
                {
                    await PopupNavigation.Instance.PopAllAsync();
                    await DisplayAlert("Connect Exception!", exception.Message, "Understood");
                }
        }

        private async void NotificationEventOnNewNotification(object sender, ControllerEventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private void LabelBTScan_OnTapped(object sender, EventArgs e)
        {
            if (_timer.Enabled == false)
                BtScan();
        }

        private async void ButtonBack_OnPressed(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}