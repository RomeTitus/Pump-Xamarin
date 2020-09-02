using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Pump.Database;
using Pump.Droid.Database.Table;
using Pump.FirebaseDatabase;
using Pump.Layout;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace Pump
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class AddController : ContentPage
    {

        bool? _internalConnection = null;
        bool? _externalConnection = null;
        bool? _firebaseConnection = null;
        private String _mac;
        readonly VerifyConnections _loadingScreen = new VerifyConnections { CloseWhenBackgroundIsClicked = false };
        PumpConnection _pumpConnection = new PumpConnection();
        private double height;
        private double width;

        public AddController(bool firstConnection)
        {
            InitializeComponent();
            if (!firstConnection)
                BtnBackAddConnectionScreen.IsVisible = true;
        }

        private void BtnAddController_Clicked(object sender, EventArgs e)
        {
            AddIrrigationController();
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
                    addPumpConnection(_pumpConnection);
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


        private void addPumpConnection(PumpConnection pumpConnection)
        {

            var databaseController = new DatabaseController();
            databaseController.AddControllerConnection(pumpConnection);

        }

       

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width == this.width && height == this.height) return;
            this.width = width;
            this.height = height;
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
            Navigation.PushModalAsync(new ConnectionScreen());
        }
    }
}