using System;
using System.Collections.Specialized;
using System.Linq;
using Pump.Class;
using Pump.Layout.Views;
using Pump.SocketController.BT;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BlueToothScan : ContentPage
    {
        private readonly BluetoothManager _bluetoothManager;
        private readonly NotificationEvent _notificationEvent;

        public BlueToothScan(NotificationEvent notificationEvent)
        {
            InitializeComponent();
            _notificationEvent = notificationEvent;
            _notificationEvent.OnUpdateStatus += NotificationEventOnNewNotification;
            _bluetoothManager = new BluetoothManager();
            _bluetoothManager.IrrigationDeviceBt.CollectionChanged += PopulateBluetoothDeviceEvent;
            _bluetoothManager.AdapterBle.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            BtnScan_OnClicked(new object(), new EventArgs());
        }

        private async void BtnScan_OnClicked(object sender, EventArgs e)
        {
            ScrollViewSetupSystem.Children.Clear();
            if (_bluetoothManager.AdapterBle.IsScanning)
            {
                BtnScan.Text = "Start Scan";
            }
            else
                BtnScan.Text = "Stop Scan";

            await _bluetoothManager.StartScanning();
        }

        private void PopulateBluetoothDeviceEvent(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateBluetoothDevice);
        }

        private async void ViewSiteScreen_Tapped(object sender, EventArgs e)
        {
            var viewBlueTooth = (StackLayout)sender;
            var blueToothDevice =
                _bluetoothManager.IrrigationDeviceBt.First(x => x?.Id.ToString() == viewBlueTooth.AutomationId);

            var result = await DisplayAlert("Connect?", "You have selected to connect to " + blueToothDevice.Name,
                "Accept", "Cancel");
            if (result)
            {
                try
                {
                    await _bluetoothManager.ConnectToDevice(blueToothDevice);
                    var isController = await _bluetoothManager.IsController();
                    if (isController)
                    {
                        await Navigation.PushModalAsync(new SetupSystem(_bluetoothManager, _notificationEvent));
                    }
                    else
                        await DisplayAlert("Irrigation", "Not verified controller", "Understood");
                }
                catch (Exception exception)
                {
                    await DisplayAlert("Connect Exception!", exception.Message, "Understood");
                }
            }
        }

        private void PopulateBluetoothDevice()
        {
            ScreenCleanup();
            foreach (var bluetooth in _bluetoothManager.IrrigationDeviceBt)
            {
                var existingView = false;
                foreach (var view in ScrollViewSetupSystem.Children)
                {
                    if (view.GetType() != typeof(ViewBluetoothSummary)) continue;
                    var existingViewBluetoothSummary = (ViewBluetoothSummary)view;
                    if (existingViewBluetoothSummary.BluetoothDevice.Id == bluetooth.Id)
                    {
                        existingView = true;
                        existingViewBluetoothSummary.BluetoothDevice = bluetooth;
                        existingViewBluetoothSummary.Populate();
                        break;
                    }
                }

                if (existingView) continue;
                var blueToothView = new ViewBluetoothSummary(bluetooth);
                blueToothView.GetTapGestureRecognizer().Tapped += ViewSiteScreen_Tapped;
                ScrollViewSetupSystem.Children.Add(blueToothView);
            }
        }

        private void ScreenCleanup()
        {
            foreach (var viewBlueTooth in ScrollViewSetupSystem.Children)
            {
                var existingView = false;
                foreach (var bluetooth in _bluetoothManager.IrrigationDeviceBt)
                {
                    if (viewBlueTooth.GetType() != typeof(ViewBluetoothSummary)) continue;
                    var existingViewBluetoothSummary = (ViewBluetoothSummary)viewBlueTooth;
                    if (existingViewBluetoothSummary.BluetoothDevice.Id == bluetooth.Id)
                    {
                        existingView = true;
                        existingViewBluetoothSummary.BluetoothDevice = bluetooth;
                        existingViewBluetoothSummary.Populate();
                        break;
                    }
                }

                if (existingView) continue;
                ScrollViewSetupSystem.Children.Remove(viewBlueTooth);
            }
        }

        void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            BtnScan.Text = "Start Scan";
        }

        private async void NotificationEventOnNewNotification(object sender, ControllerEventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}