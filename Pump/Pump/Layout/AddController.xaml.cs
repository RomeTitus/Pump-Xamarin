using Pump.Database;
using Pump.Droid.Database.Table;
using Pump.Layout;
using Rg.Plugins.Popup.Services;
using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace Pump
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class AddController : ContentPage
    {
        double width = 0;
        double height = 0;

        public AddController()
        {
            InitializeComponent();
        }

        private void BtnAddController_Clicked(object sender, EventArgs e)
        {
            //DisplayAlert("Title", "Hellow World", "OK");
            AddIrrigationController();
        }

        private void AddIrrigationController()
        {
            int ExternalPort = 0;
            int InternalPort = 0;

            if (TxtControllerName.Text == null || (TxtInternalConnection.Text == null || TxtInternalPort.Text== null || !int.TryParse(TxtInternalPort.Text, out InternalPort)) && (TxtExternalConnection.Text == null || TxtExternalPort.Text == null || !int.TryParse(TxtExternalPort.Text, out ExternalPort)))
                OutLineIncorrectFields(InternalPort, ExternalPort);
                
            else
            {
                var loadingScreen = new VerifyConnections();
                PopupNavigation.Instance.PushAsync(loadingScreen);

                if ((TxtInternalConnection.Text != null && TxtInternalPort.Text != null) && (TxtExternalConnection.Text != null && TxtExternalPort.Text != null))
                    new Thread(() => checkConnectionInternalAndExternal(TxtControllerName.Text, TxtInternalConnection.Text, int.Parse(TxtInternalPort.Text), 
                        TxtExternalConnection.Text, int.Parse(TxtExternalPort.Text), loadingScreen)).Start();
                

                else if (TxtInternalConnection.Text != null && TxtInternalPort.Text != null)
                    new Thread(() => checkConnectionInternal(TxtControllerName.Text, TxtInternalConnection.Text, int.Parse(TxtInternalPort.Text),
                        loadingScreen)).Start();


                else if (TxtExternalConnection.Text != null && TxtExternalPort.Text != null)
                    new Thread(() => checkConnectionExternal(TxtControllerName.Text, TxtExternalConnection.Text, int.Parse(TxtExternalPort.Text),
                        loadingScreen)).Start();

            }

           
        }

        private void OutLineIncorrectFields(int internalPort, int externalPort)
        {
            if (TxtControllerName.Text == null)
                LabelControllerName.TextColor = Color.Red;
            if (TxtInternalConnection.Text == null)
                LabelTxtInternalConnection.TextColor = Color.Red;
            if (TxtInternalPort.Text == null || internalPort == 0)
                LabelInternalPort.TextColor = Color.Red;
            if (TxtExternalConnection.Text == null)
                LabelExternalConnection.TextColor = Color.Red;
            if (TxtExternalPort.Text == null || externalPort == 0)
                LabelExternalPort.TextColor = Color.Red;
        }
        
        private void checkConnectionInternalAndExternal(string name, string internalHost, int internalPort, string externalHost, int externalPort, VerifyConnections loadingScreen)
        {

            string mac = null;
            string internalConnection;
            string externalConnection;
            
            internalConnection = checkConnection(internalHost, internalPort);

            externalConnection = checkConnection(externalHost, externalPort);

            if (internalConnection != null)
                mac = internalConnection;

            if (externalConnection != null)
                mac = externalConnection;

            if(mac != null)
                addPumpConnection(new PumpConnection(name, mac, internalHost, internalPort, externalHost, externalPort));
            
            Device.BeginInvokeOnMainThread(() =>
            {
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



        private void checkConnectionInternal(string name, string host, int port, VerifyConnections loadingScreen)
        {
            string internalConnection;

            internalConnection = checkConnection(host, port);
            if (internalConnection != null)
                addPumpConnection(new PumpConnection(name, internalConnection, host, port, null, -1));

            Device.BeginInvokeOnMainThread(() =>
            {
                loadingScreen.stopActivityIndicatior();

                if (internalConnection != null)
                    loadingScreen.InternalSuccess();
                else
                    loadingScreen.InternalFailed();
                //PopupNavigation.Instance.PopAsync();
            });
        }


        private void checkConnectionExternal(string name, string host, int port, VerifyConnections loadingScreen)
        {
            string externalConnection;

            externalConnection = checkConnection(host, port);

            if (externalConnection != null)
                addPumpConnection(new PumpConnection(name, externalConnection, null, -1, host, port));
            Device.BeginInvokeOnMainThread(() =>
            {
                loadingScreen.stopActivityIndicatior();

                if (externalConnection != null)
                    loadingScreen.ExternalSuccess();
                else
                    loadingScreen.ExternalFailed();
                //PopupNavigation.Instance.PopAsync();
            });
        }

        private void addPumpConnection(PumpConnection pumpConnection)
        {
            DatabaseController databaseController = new DatabaseController();
            databaseController.AddPumpConnection(pumpConnection);
        }
        private string checkConnection(string host, int port)
        {
            SocketController.SocketVerify socket = new SocketController.SocketVerify(host, port);
            try
            {
                string responce = socket.verifyConnection();
                if (responce != "getMAC")
                    return responce;
                else
                    return null;
            }
            catch
            {
                return null;
            }
            
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (width != this.width || height != this.height)
            {
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
        }

    }
}
