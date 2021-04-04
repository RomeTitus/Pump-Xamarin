using System;
using System.Threading.Tasks;
using Pump.Class;
using Pump.Database;
using Pump.Database.Table;
using Pump.FirebaseDatabase;
using Pump.Layout;
using Pump.SocketController;
using Pump.SocketController.Firebase;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace Pump
{
    public partial class ExistingController : ContentPage
    {
        private bool? _internalConnection;
        private bool? _externalConnection;
        private bool? _firebaseConnection;
        private string _mac;
        private readonly VerifyConnections _loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
        private readonly PumpConnection _pumpConnection = new PumpConnection();
        private double _height;
        private double _width;
        private readonly NotificationEvent _notificationEvent;

        public ExistingController(bool firstConnection, NotificationEvent notificationEvent, PumpConnection pumpConnection = null)
        {
            InitializeComponent();
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

        private void FrameAddSystemTap_Tapped(object sender, EventArgs e)
        {
            AddIrrigationController();
        }

        private void ConnectionPickerOnSelectedIndexChanged(object sender, EventArgs e)
        {
            if(ConnectionPicker.SelectedIndex == -1)
                return;
            _pumpConnection.ConnectionType = ConnectionPicker.SelectedIndex;
            new DatabaseController().UpdateControllerConnection(_pumpConnection);
            //TODO Throw event to change connection Type otherwise user needs to reload application
        }

        

        private void PopulateElements()
        {
            foreach (var ConnectionType in _pumpConnection.ConnectionTypeList)
            {
                ConnectionPicker.Items.Add(ConnectionType);
            }
            
            ConnectionPicker.SelectedIndex = _pumpConnection.ConnectionType;

            TxtControllerName.Text = _pumpConnection.Name;
            TxtInternalConnection.Text = _pumpConnection.InternalPath;
            TxtInternalPort.Text = _pumpConnection.InternalPort.ToString();
            TxtExternalConnection.Text = _pumpConnection.ExternalPath;
            TxtExternalPort.Text = _pumpConnection.ExternalPort.ToString();
            TxtControllerCode.Text = _pumpConnection.Mac;
            TxtControllerCode.IsEnabled = false;
            BtnAddController.Text = "Update";
            BtnNewController.IsVisible = false;
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
                    if (_internalConnection != null)
                    {
                        if (_internalConnection == true)
                        {
                            _loadingScreen.InternalSuccess();
                            _pumpConnection.InternalPath = TxtInternalConnection.Text;
                            _pumpConnection.InternalPort = Convert.ToInt32(TxtInternalPort.Text);
                            _pumpConnection.Mac = _mac;
                        }

                        else
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
                            _loadingScreen.ExternalFailed();
                    }
                }

                if (!string.IsNullOrEmpty(TxtControllerCode.Text))
                {
                    try
                    {
                        _firebaseConnection =
                            Task.Run(() => new Authentication().IrrigationSystemPath(TxtControllerCode.Text)).Result.Object;


                    }
                    catch
                    {
                        _firebaseConnection = false;
                    }

                    if (_firebaseConnection != null)
                    {
                        if (_firebaseConnection == true)
                        {
                            _loadingScreen.FirebaseSuccess();
                            _pumpConnection.RealTimeDatabase = true;
                            _pumpConnection.Mac = TxtControllerCode.Text;
                        }

                        else
                            _loadingScreen.FirebaseFailed();
                    }
                }

                _loadingScreen.StopActivityIndicator();
                if (_firebaseConnection == true || _externalConnection == true || _internalConnection == true)
                {
                    AddPumpConnection(_pumpConnection);
                }
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

            if (externalConnection != null && internalConnection != null &&
                (code && internalConnection != false && externalConnection != false))
            {
                return notification;
            }

            if (!code && internalConnection == false && externalConnection == false)
            {
                if (notification.Length < 1)
                    notification = "\u2022 Controller Code required";
                else
                    notification += "\n\u2022 Controller Code required";
                LabelControllerCode.TextColor = Color.Red;

            }

            if (internalConnection == null || !code && internalConnection == false && externalConnection == false)
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

            if (externalConnection == null || !code && internalConnection == false && externalConnection == false)
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
            if (width == this._width && height == this._height) return;
            this._width = width;
            this._height = height;
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

        private void BtnNewController_OnClicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new BlueToothScan(_notificationEvent));
        }
        private async void NotificationEventOnNewNotification(object sender, ControllerEventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        private void BtnAdvancedConnectionScreen_OnClicked(object sender, EventArgs e)
        {
            StackLayoutLocalConnection.IsVisible = !StackLayoutLocalConnection.IsVisible;
            
            if (StackLayoutLocalConnection.IsVisible)
            {
                BtnAdvancedConnectionScreen.Text = "Hide Network";
            }
            else
            {
                BtnAdvancedConnectionScreen.Text = "Show Network";
            }
        }
    }
}