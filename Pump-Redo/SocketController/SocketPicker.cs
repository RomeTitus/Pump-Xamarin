using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pump.Class;
using Pump.Database;
using Pump.IrrigationController;
using Pump.Layout;
using Pump.SocketController.BT;
using Pump.SocketController.Firebase;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace Pump.SocketController
{
    public class SocketPicker
    {
        private readonly InitializeBlueTooth _initializeBlueTooth;
        private readonly InitializeFirebase _initializeFirebase;
        private readonly InitializeNetwork _initializeNetwork;
        private readonly NotificationEvent _notificationEvent;
        private readonly ObservableIrrigation _observableIrrigation;
        private Label _activityLabel;
        private FloatingScreenScroll _floatingScreenScreen;

        public SocketPicker(ObservableIrrigation observableIrrigation)
        {
            _observableIrrigation = observableIrrigation;
            _initializeFirebase = new InitializeFirebase(observableIrrigation);
            _initializeNetwork = new InitializeNetwork(observableIrrigation);
            _initializeBlueTooth = new InitializeBlueTooth(observableIrrigation);
            _notificationEvent = new NotificationEvent();
        }

        public SocketPicker()
        {
        }


        public async void ConnectionPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = (Picker)sender;
            if (picker.SelectedIndex == -1)
                return;
            var controllerList = new DatabaseController().GetControllerConnectionList();
            var selectedConnection = controllerList[picker.SelectedIndex];
            Disposable();
            new DatabaseController().SetSelectedController(selectedConnection);
            await Subscribe();
        }

        private void Disposable()
        {
            _observableIrrigation.IsDisposable = true;
            var pumpConnection = new DatabaseController().GetControllerConnectionSelection();

            if (pumpConnection.ConnectionType == 0)
                _initializeFirebase.Disposable();
            else if (pumpConnection.ConnectionType == 1)
                _initializeNetwork.Disposable();
            else if (pumpConnection.ConnectionType == 2) _initializeBlueTooth.Disposable();
        }

        private async Task Subscribe()
        {
            _observableIrrigation.IsDisposable = false;
            var pumpConnection = new DatabaseController().GetControllerConnectionSelection();
            if (pumpConnection.ConnectionType == 0)
                _initializeFirebase.SubscribeFirebase();
            else if (pumpConnection.ConnectionType == 1)
                await _initializeNetwork.SubscribeNetwork();
            else if (pumpConnection.ConnectionType == 2) await _initializeBlueTooth.SubscribeBle();
        }

        public async Task<string> SendCommand(object sendObject, bool runInBackground = true)
        {
            var pumpConnection = new DatabaseController().GetControllerConnectionSelection();
            var buttonClosed = new Button
            {
                Text = "Close", VerticalOptions = LayoutOptions.EndAndExpand,
                HorizontalOptions = LayoutOptions.StartAndExpand, WidthRequest = 280, IsEnabled = false
            };

            _activityLabel = new Label { HorizontalOptions = LayoutOptions.CenterAndExpand, FontSize = 15 };
            var activityIndicator = new ActivityIndicator
            {
                Margin = new Thickness(0, 20, 0, 20), HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.StartAndExpand, Color = Color.Black, IsVisible = true, IsRunning = true
            };
            if (runInBackground == false)
            {
                _floatingScreenScreen = new FloatingScreenScroll { CloseWhenBackgroundIsClicked = false };
                _notificationEvent.OnUpdateStatus += NotificationEventOnUpdateStatus;
                buttonClosed.Clicked += (sender, args) => { PopupNavigation.Instance.PopAsync(); };
                object connectionStatusStackLayout = new StackLayout
                {
                    HeightRequest = 550,
                    WidthRequest = 400,
                    VerticalOptions = LayoutOptions.StartAndExpand,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    Children =
                    {
                        new Label
                        {
                            Text = sendObject.GetType().ToString().Split('.').Last(), FontSize = 24,
                            Margin = new Thickness(0, 0, 0, 20)
                        },
                        _activityLabel, activityIndicator, buttonClosed
                    }
                };


                _floatingScreenScreen.SetFloatingScreen(new List<object> { connectionStatusStackLayout });
                await PopupNavigation.Instance.PushAsync(_floatingScreenScreen);
            }

            var result = "Did Not Complete Action: " + sendObject;
            switch (pumpConnection?.ConnectionType)
            {
                case 0:
                    result = await new Authentication().Descript(sendObject, _notificationEvent);
                    break;
                case 1:
                    if (_initializeNetwork == null)
                        break;
                    _initializeNetwork.RequestIrrigationTimer.Restart();
                    _initializeNetwork.RequestNow = true;
                    result = await _initializeNetwork.NetworkManager.SendAndReceiveToNetwork(
                        SocketCommands.Descript(sendObject), pumpConnection);
                    break;
                case 2:
                    if (_initializeBlueTooth == null)
                        break;
                    _initializeBlueTooth.RequestIrrigationTimer.Restart();
                    result = await _initializeBlueTooth?.BlueToothManager.SendAndReceiveToBle(
                        SocketCommands.Descript(sendObject));
                    _initializeBlueTooth.RequestNow = true;
                    break;
                default:
                    result = "Unknown Operation/Could not Identify user operations";
                    break;
            }

            buttonClosed.IsEnabled = true;
            activityIndicator.IsVisible = false;
            if (_notificationEvent != null)
                _notificationEvent.OnUpdateStatus -= NotificationEventOnUpdateStatus;
            return result;
        }

        private void NotificationEventOnUpdateStatus(object sender, ControllerEventArgs e)
        {
            if (_activityLabel == null)
                return;

            Device.InvokeOnMainThreadAsync(() => { _activityLabel.Text += e.Status; });
        }

        public BluetoothManager BluetoothManager()
        {
            return _initializeBlueTooth.BlueToothManager;
        }
    }
}