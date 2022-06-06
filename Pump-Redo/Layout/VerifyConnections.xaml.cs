using System;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class VerifyConnections : PopupPage
    {
        private bool _success;

        public VerifyConnections()
        {
            InitializeComponent();
        }

        public void StopActivityIndicator()
        {
            StackLayoutConnectionInfo.IsVisible = true;
            ActivityIndicatorScreen.IsEnabled = false;
            ActivityIndicatorScreen.IsRunning = false;
            ActivityIndicatorScreen.IsVisible = false;
        }

        public void InternalSuccess()
        {
            LabelInternalConnection.Text = "Internal Connection was successful";
            _success = true;
        }

        public void ExternalSuccess()
        {
            LabelExternalConnection.Text = "External Connection was successful";
            _success = true;
        }

        public void FirebaseSuccess()
        {
            LabelFirebaseConnection.Text = "Online Connection was successful";
            _success = true;
        }

        public void InternalFailed()
        {
            LabelInternalConnection.Text = "Internal Connection Failed";
        }

        public void ExternalFailed()
        {
            LabelExternalConnection.Text = "External Connection Failed";
        }

        public void FirebaseFailed()
        {
            LabelFirebaseConnection.Text = "Online Connection Failed";
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            if (_success)
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