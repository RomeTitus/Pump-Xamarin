using System;
using System.Linq;
using Plugin.BLE.Abstractions.EventArgs;
using Pump.Layout.Views;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BluetoothScan : ContentPage
    {
        private BluetoothManager _bluetoothManager;
        public BluetoothScan()
        {
            InitializeComponent();
            _bluetoothManager = new BluetoothManager();
            _bluetoothManager.DeviceList.CollectionChanged += PopulateBluetoothDeviceEvent;
            _bluetoothManager.AdapterBLE.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;
            _bluetoothManager.AdapterBLE.DeviceConnected += AdapterBLE_DeviceConnected;
        }

        private void AdapterBLE_DeviceConnected(object sender, DeviceEventArgs e)
        {
            DisplayAlert("Bluetooth", e.Device.Name + ": Connected!", "Understood");
        }

        private async void BtnScan_OnClicked(object sender, EventArgs e)
        {
            ScrollViewSetupSystem.Children.Clear();
            if (_bluetoothManager.AdapterBLE.IsScanning)
            {
                BtnScan.Text = "Start Scan";
                
            }
            else
                BtnScan.Text = "Stop Scan";
            
            await _bluetoothManager.StartScanning();
            
        }

        private void PopulateBluetoothDeviceEvent(object sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(PopulateBluetoothDevice);
        }

        private void ViewSiteScreen_Tapped(object sender, EventArgs e)
        {
            var viewBlueTooth = (StackLayout)sender;
            var blueToothDevice = _bluetoothManager.DeviceList.First(x => x.Id.ToString() == viewBlueTooth.AutomationId);
            Device.BeginInvokeOnMainThread(async () =>
            {
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
                            await Navigation.PushModalAsync(new SetupSystem(_bluetoothManager));
                        }
                        else
                            await DisplayAlert("Irrigation", "Not verified controller", "Understood");
                    }
                    catch (Exception exception)
                    {
                        await DisplayAlert("Connect Exception!", exception.Message, "Understood");
                    }
                    
                }
            });
        }

        private void PopulateBluetoothDevice()
        {
            ScreenCleanup();
            foreach (var bluetooth in _bluetoothManager.DeviceList)
            {
                var existingView = false;
                foreach (var view in ScrollViewSetupSystem.Children)
                {
                    if (view.GetType() != typeof(ViewBluetoothSummary)) continue;
                    var existingViewBluetoothSummary = (ViewBluetoothSummary) view;
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
                foreach (var bluetooth in _bluetoothManager.DeviceList)
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
    }
}