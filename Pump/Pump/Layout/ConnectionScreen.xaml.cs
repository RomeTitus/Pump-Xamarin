using System;
using System.Threading;
using Pump.Database;
using Pump.Droid.Database.Table;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConnectionScreen : ContentPage
    {
        private bool _showAdvanced = false;
        private PumpConnection _connection;
        public ConnectionScreen()
        {
            InitializeComponent();
            GetSelectedPumpDetail();
        }

        private void StackLayoutConnectionView_OnTapped(object sender, EventArgs e)
        {
            if (_showAdvanced)
            {
                ConnectionViewImage.Rotation = 0;
                _showAdvanced = false;
                ConnectionDetailLayout.IsVisible = false;
            }
            else
            {
                ConnectionViewImage.Rotation = 180;
                _showAdvanced = true;
                ConnectionDetailLayout.IsVisible = true;
            }
                
            //throw new NotImplementedException();
        }

        private void GetSelectedPumpDetail()
        {
            var controller = new DatabaseController();
            _connection = controller.GetPumpSelection();

            TxtInternalConnection.Text = _connection.InternalPath;
            if (_connection.InternalPort != -1)
                TxtInternalPort.Text = _connection.InternalPort.ToString();
            TxtExternalConnection.Text = _connection.ExternalPath;
            if(_connection.ExternalPort != -1)
                TxtExternalPort.Text = _connection.ExternalPort.ToString();
        }

        private void BtnUpdateController_OnClicked(object sender, EventArgs e)
        {
            int externalPort = 0;
            int internalPort = 0;
            if ((string.IsNullOrWhiteSpace(TxtInternalConnection.Text) || string.IsNullOrWhiteSpace(TxtInternalPort.Text)||
                !int.TryParse(TxtInternalPort.Text, out internalPort)) &&
                (TxtExternalConnection.Text == null || TxtExternalPort.Text == null ||
                 !int.TryParse(TxtExternalPort.Text, out externalPort)))
                OutLineIncorrectFields(internalPort, externalPort);

            else
            {
                var loadingScreen = new VerifyConnections();
                PopupNavigation.Instance.PushAsync(loadingScreen);

                if ((!string.IsNullOrWhiteSpace(TxtInternalConnection.Text)  && !string.IsNullOrWhiteSpace(TxtInternalPort.Text)) &&
                     (!string.IsNullOrWhiteSpace(TxtExternalConnection.Text) && !string.IsNullOrWhiteSpace(TxtExternalPort.Text)))
                    new Thread(() => CheckConnectionInternalAndExternal(TxtInternalConnection.Text,
                        int.Parse(TxtInternalPort.Text),
                        TxtExternalConnection.Text, int.Parse(TxtExternalPort.Text), loadingScreen)).Start();
                else if((!string.IsNullOrWhiteSpace(TxtInternalConnection.Text) && !string.IsNullOrWhiteSpace(TxtInternalPort.Text)))
                    new Thread(() => CheckConnection(TxtInternalConnection.Text,
                        int.Parse(TxtInternalPort.Text),
                         loadingScreen, true)).Start();
                else if ((!string.IsNullOrWhiteSpace(TxtExternalConnection.Text) && !string.IsNullOrWhiteSpace(TxtExternalPort.Text)))
                    new Thread(() => CheckConnection(TxtExternalConnection.Text,
                        int.Parse(TxtExternalPort.Text),
                        loadingScreen, false)).Start();
            }
        }

        private void OutLineIncorrectFields(int internalPort, int externalPort)
        {
            if (TxtInternalConnection.Text == null)
                LabelTxtInternalConnection.TextColor = Color.Red;
            if (TxtInternalPort.Text == null || internalPort == 0)
                LabelInternalPort.TextColor = Color.Red;
            if (TxtExternalConnection.Text == null)
                LabelExternalConnection.TextColor = Color.Red;
            if (TxtExternalPort.Text == null || externalPort == 0)
                LabelExternalPort.TextColor = Color.Red;
        }



        private void CheckConnectionInternalAndExternal(string internalHost, int internalPort, string externalHost, int externalPort, VerifyConnections loadingScreen)
        {

            string mac = null;

            var internalConnection = checkConnection(internalHost, internalPort);

            var externalConnection = checkConnection(externalHost, externalPort);

            if (internalConnection != null)
            {
                mac = internalConnection;
                _connection.InternalPath = internalHost;
                _connection.InternalPort = internalPort;
            }


            if (externalConnection != null)
            {
                mac = externalConnection;
                _connection.ExternalPath = externalHost;
                _connection.ExternalPort = externalPort;
            }


            if (mac != null)
            {
                var database = new DatabaseController();
                database.UpdatePumpConnection(_connection);

            }

            Device.BeginInvokeOnMainThread(() =>
            {
                ConnectionViewImage.Rotation = 0;
                _showAdvanced = false;
                ConnectionDetailLayout.IsVisible = false;

                loadingScreen.stopActivityIndicatior();

                if (internalConnection != null)
                    loadingScreen.InternalSuccess();
                else
                    loadingScreen.InternalFailed();
                if (externalConnection != null)
                    loadingScreen.ExternalSuccess();
                else
                    loadingScreen.ExternalFailed();

                //PopupNavigation.Instance.PopAsync();
            });
        }

        private void CheckConnection(string host, int port, VerifyConnections loadingScreen, bool isInternal)
        {

            if (isInternal)
            {
                _connection.InternalPath = host;
                _connection.InternalPort = port;
            }
            else
            {
                _connection.ExternalPath = host;
                _connection.ExternalPort = port;
            }

            var mac = checkConnection(host, port);

            
            
            if (mac != null)
            {
                var database = new DatabaseController();
                database.UpdatePumpConnection(_connection);

            }

            Device.BeginInvokeOnMainThread(() =>
            {
                ConnectionViewImage.Rotation = 0;
                _showAdvanced = false;
                ConnectionDetailLayout.IsVisible = false;

                loadingScreen.stopActivityIndicatior();

                if (isInternal)
                {
                    if (host != null)
                        loadingScreen.InternalSuccess();
                    else
                        loadingScreen.InternalFailed();
                }
                else
                {
                    if (host != null)
                        loadingScreen.ExternalSuccess();
                    else
                        loadingScreen.ExternalFailed();
                }
            });
        }


        private string checkConnection(string host, int port)
        {
            var socket = new SocketController.SocketVerify(host, port);
            try
            {
                var result = socket.verifyConnection();
                return result != "getMAC" ? result : null;
            }
            catch
            {
                return null;
            }

        }


        private void BtnBackConnectionScreen_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
            //Navigation.PushModalAsync(new HomeScreen());
            
        }
    }
}