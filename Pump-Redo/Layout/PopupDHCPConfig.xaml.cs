using System;
using System.Collections.Generic;
using System.Linq;
using Pump.Class;
using Pump.CustomRender;
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

        private void EntryDhcpConfig_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = (EntryOutlined)sender;
            ValidateIpTextChange(entry, e.NewTextValue);
        }

        private void ValidateIpTextChange(EntryOutlined entry, string textValue)
        {
            List<char> allowedCharacters = new List<char> { '0','1','2','3','4','5','6','7','8','9','.',',',' '};
            var invalid = false;
            foreach (var _ in textValue.Where(charValue => !allowedCharacters.Contains(charValue)))
            {
                invalid = true;
            }
            
            if (invalid == false)
                invalid = textValue.Length > 3 && !textValue.Contains(".");

            if (invalid == false)
            {
                var ipArray = textValue.Split('.');
                foreach (var subIp in ipArray)
                {
                    if (subIp.Length > 3)
                    {
                        invalid = true;
                        break;
                    }
                }
            }
                

            Device.BeginInvokeOnMainThread(() =>
            {
                if (invalid)
                {
                    entry.PlaceholderColor = Color.Red;
                    entry.BorderColor = Color.Red;
                }
                else
                {
                    entry.PlaceholderColor = Color.Navy;
                    entry.BorderColor = Color.Black;

                }
            });
        }
    }
}