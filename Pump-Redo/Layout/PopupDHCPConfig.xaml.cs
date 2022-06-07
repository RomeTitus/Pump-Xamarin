using System;
using System.Collections.Generic;
using Pump.Class;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PopupDHCPConfig : PopupPage
    {
        private readonly DHCPConfig _dhcpConfig;

        public PopupDHCPConfig(DHCPConfig dhcpConfig = null)
        {
            InitializeComponent();
            _dhcpConfig = dhcpConfig;
            DhcpPicker.SelectedIndex = 0;
            if(dhcpConfig != null)
                PopulateExisting();
        }

        private void PopulateExisting()
        {
            StackLayoutDhcpConfig.IsVisible = true;
            LabelHeading.Text = _dhcpConfig.DHCPinterface;
            
        }

        private void ButtonCancel_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }
    }
}