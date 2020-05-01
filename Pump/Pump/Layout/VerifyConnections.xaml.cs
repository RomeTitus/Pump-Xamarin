using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VerifyConnections : PopupPage
    {
        public VerifyConnections()
        {
            InitializeComponent();
        }


        public void stopActivityIndicatior()
        {
            staclLayoutConnectionInfo.IsVisible = true;
            ActivityIndicatiorScreen.IsEnabled = false;
            ActivityIndicatiorScreen.IsRunning = false;
            ActivityIndicatiorScreen.IsVisible = false;
        }

        public void InternalSuccess()
        {
            labelInternalConnection.Text = "Internal Connection was successful";
            
        }
        public void ExternalSuccess()
        {
            labelExternalConnection.Text = "External Connection was successful" ;
        }

        public void InternalFailed()
        {
            labelInternalConnection.Text = "Internal Connection Failed" ;
        }
        public void ExternalFailed()
        {
            labelExternalConnection.Text = "External Connection Failed" ;
        }
    }
}