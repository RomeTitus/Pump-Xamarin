using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pump.Database;
using Pump.Database.Table;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VerifyConnections : PopupPage
    {
        private bool success = false;
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
            new DatabaseController().SetActivityStatus(new ActivityStatus(true));
            success = true;
        }
        public void ExternalSuccess()
        {
            labelExternalConnection.Text = "External Connection was successful" ;
            new DatabaseController().SetActivityStatus(new ActivityStatus(true));
            success = true;
        }

        public void InternalFailed()
        {
            labelInternalConnection.Text = "Internal Connection Failed" ;
        }
        public void ExternalFailed()
        {
            labelExternalConnection.Text = "External Connection Failed" ;
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            if (success)
            {
                PopupNavigation.Instance.PopAsync();
                Navigation.PopModalAsync();
            }
            else
            {
                PopupNavigation.Instance.PopAsync();
            }
            
        }
    }
}