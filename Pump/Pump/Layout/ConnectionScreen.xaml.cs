using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Pump.Database;
using Pump.Droid.Database.Table;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Switch = Xamarin.Forms.Switch;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConnectionScreen : ContentPage
    {

        private List<PumpConnection> ControllerList = new List<PumpConnection>();

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private PumpConnection _connection;
        private string _externalConnection;
        private string _internalConnection;
        private bool _showAdvanced;

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
            PopulateControllerList();

            var controller = new DatabaseController();
            _connection = controller.GetControllerConnectionSelection();
            for (var i = 0; i < ControllerList.Count; i++)
            {
                if(ControllerList[i].ID == _connection.ID)
                    ControllerPicker.SelectedIndex = i;
            }
            
            TxtInternalConnection.Text = _connection.InternalPath;
            if (_connection.InternalPort != -1)
                TxtInternalPort.Text = _connection.InternalPort.ToString();
            TxtExternalConnection.Text = _connection.ExternalPath;
            if (_connection.ExternalPort != -1)
                TxtExternalPort.Text = _connection.ExternalPort.ToString();

            if (_connection.RealTimeDatabase == null) return;
            SwitchRealTimeDatabase.Toggled -= SwitchRealTimeDatabase_OnToggled;
            SwitchRealTimeDatabase.IsToggled = (bool) _connection.RealTimeDatabase;
            SwitchRealTimeDatabase.Toggled += SwitchRealTimeDatabase_OnToggled;
        }

        private void PopulateControllerList()
        {
            ControllerList = new DatabaseController().GetControllerConnectionList();
            ControllerPicker.Items.Clear();
            foreach (var equipment in ControllerList)
            {
                ControllerPicker.Items.Add(string.IsNullOrEmpty(equipment.Name) ? "Name is missing" : equipment.Name);
            }

            if (ControllerPicker.Items.Count > 0)
            { 
                var selectedController =  new DatabaseController().GetControllerConnectionSelection();
                for (var i = 0; i < ControllerList.Count; i++)
                {
                    if (ControllerList[i].ID != selectedController.ID) continue;
                    ControllerPicker.SelectedIndex = i;
                    break;

                }
               
            }
                
            BtnDeleteSelectedController.IsEnabled = ControllerPicker.Items.Count > 1;
        }

        private void BtnUpdateController_OnClicked(object sender, EventArgs e)
        {
            var externalPort = 0;
            var internalPort = 0;
            if ((string.IsNullOrWhiteSpace(TxtInternalConnection.Text) ||
                 string.IsNullOrWhiteSpace(TxtInternalPort.Text) ||
                 !int.TryParse(TxtInternalPort.Text, out internalPort)) &&
                (TxtExternalConnection.Text == null || TxtExternalPort.Text == null ||
                 !int.TryParse(TxtExternalPort.Text, out externalPort)))
            {
                OutLineIncorrectFields(internalPort, externalPort);
            }

            else
            {
                var loadingScreen = new VerifyConnections();
                PopupNavigation.Instance.PushAsync(loadingScreen);

                if (!string.IsNullOrWhiteSpace(TxtInternalConnection.Text) &&
                    !string.IsNullOrWhiteSpace(TxtInternalPort.Text) &&
                    !string.IsNullOrWhiteSpace(TxtExternalConnection.Text) &&
                    !string.IsNullOrWhiteSpace(TxtExternalPort.Text))
                    new Thread(() => CheckConnectionInternalAndExternal(TxtInternalConnection.Text,
                        int.Parse(TxtInternalPort.Text),
                        TxtExternalConnection.Text, int.Parse(TxtExternalPort.Text), loadingScreen)).Start();
                else if (!string.IsNullOrWhiteSpace(TxtInternalConnection.Text) &&
                         !string.IsNullOrWhiteSpace(TxtInternalPort.Text))
                    new Thread(() => CheckConnection(TxtInternalConnection.Text,
                        int.Parse(TxtInternalPort.Text),
                        loadingScreen, true)).Start();
                else if (!string.IsNullOrWhiteSpace(TxtExternalConnection.Text) &&
                         !string.IsNullOrWhiteSpace(TxtExternalPort.Text))
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


        private void CheckConnectionInternalAndExternal(string internalHost, int internalPort, string externalHost,
            int externalPort, VerifyConnections loadingScreen)
        {
            var internalThread = new Thread(() => CheckConnection(internalHost, internalPort, true));
            var externalThread = new Thread(() => CheckConnection(externalHost, externalPort, false));
            internalThread.Start();
            externalThread.Start();
            _stopwatch.Start();
            var aliveConnection = true;
            while (aliveConnection)
            {
                if (!internalThread.IsAlive && !externalThread.IsAlive)
                    aliveConnection = false;
                if (_stopwatch.Elapsed > TimeSpan.FromSeconds(5))
                    aliveConnection = false;
                //just waiting for the threads to finish
            }

            _stopwatch.Stop();
            Thread.Sleep(500);

            string mac = null;

            if (_internalConnection != null)
            {
                mac = _internalConnection;
                _connection.InternalPath = internalHost;
                _connection.InternalPort = internalPort;
            }


            if (_externalConnection != null)
            {
                mac = _externalConnection;
                _connection.ExternalPath = externalHost;
                _connection.ExternalPort = externalPort;
            }


            if (mac != null)
            {
                var database = new DatabaseController();
                database.UpdateControllerConnection(_connection);
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                ConnectionViewImage.Rotation = 0;
                _showAdvanced = false;
                ConnectionDetailLayout.IsVisible = false;

                loadingScreen.StopActivityIndicator();

                if (_internalConnection != null)
                    loadingScreen.InternalSuccess();
                else
                    loadingScreen.InternalFailed();
                if (_externalConnection != null)
                    loadingScreen.ExternalSuccess();
                else
                    loadingScreen.ExternalFailed();
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

            CheckConnection(host, port, isInternal);
            string mac;
            if (isInternal)
                mac = _internalConnection;
            else
                mac = _externalConnection;

            if (mac != null)
            {
                var database = new DatabaseController();
                database.UpdateControllerConnection(_connection);
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                ConnectionViewImage.Rotation = 0;
                _showAdvanced = false;
                ConnectionDetailLayout.IsVisible = false;

                loadingScreen.StopActivityIndicator();

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


        private void CheckConnection(string host, int port, bool isInternal)
        {
            var socket = new SocketVerify(host, port);
            try
            {
                var result = socket.verifyConnection();
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (isInternal)
                    {
                        if (result != "getMAC")
                            _internalConnection = result;
                    }
                    else
                    {
                        if (result != "getMAC")
                            _externalConnection = result;
                    }
                });
            }
            catch
            {
                // ignored
            }
        }


        private void BtnBackConnectionScreen_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void SwitchRealTimeDatabase_OnToggled(object sender, ToggledEventArgs e)
        {
            var toggleRealTimeDatabase = (Switch) sender;
            var controller = new DatabaseController();
            var pumpConnection = controller.GetControllerConnectionSelection();
            if (pumpConnection == null) return;
            pumpConnection.RealTimeDatabase = toggleRealTimeDatabase.IsToggled;
            controller.UpdateControllerConnection(pumpConnection);
        }

        private void BtnAddController_OnClicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new AddExistingController(false));
        }

        private void BtnDeleteSelectedController_OnClicked(object sender, EventArgs e)
        {
            new DatabaseController().DeleteControllerConnection(ControllerList[ControllerPicker.SelectedIndex]);
            PopulateControllerList();
        }


        private void ControllerPicker_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            //if(ControllerPicker.SelectedIndex != -1)
            //    new DatabaseController().SetSelectedController(ControllerList[ControllerPicker.SelectedIndex]);

        }
    }
}