using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pump.Class;
using Pump.CustomRender;
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
        private readonly List<string> _dhcpInterface;
        public PopupDHCPConfig(List<string> dhcpInterface, DHCPConfig dhcpConfig = null)
        {
            InitializeComponent();
            _dhcpConfig = dhcpConfig;
            _dhcpInterface = dhcpInterface;
            DhcpPicker.SelectedIndexChanged += DhcpPickerOnSelectedIndexChanged;
        }

        private void DhcpPickerOnSelectedIndexChanged(object sender, EventArgs e)
        {
            if(DhcpPicker.SelectedIndex == 0) 
                StackLayoutDhcpConfig.IsVisible = false;
            else
                StackLayoutDhcpConfig.IsVisible = true;
        }

        private async Task Populate()
        {
            LabelHeading.Text = _dhcpInterface[0];
            if (_dhcpConfig != null)
            {
                DhcpPicker.SelectedIndex = 1;
                await SetFocus(true);
                EntryIP.Text = _dhcpConfig.ip_address;
                EntryGateway.Text = _dhcpConfig.routers;
                EntryDNS.Text = _dhcpConfig.domain_name_servers;
            }
            else
            {
                await SetFocusIpAddress();
                EntryIP.Text = _dhcpInterface[1];
                DhcpPicker.SelectedIndex = 0;
            }
                
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(100);
            await Populate();
        }

        private async Task SetFocus(bool isFocused)
        {
            await Task.Delay(100);
            if (isFocused)
            {
                EntryIP.TextBox_Focused(this, new FocusEventArgs(this, true));
                EntryGateway.TextBox_Focused(this, new FocusEventArgs(this, true));
                EntryDNS.TextBox_Focused(this, new FocusEventArgs(this, true));
            }
            else
            {
                EntryIP.TextBox_Unfocused(this, new FocusEventArgs(this, false));
                EntryGateway.TextBox_Unfocused(this, new FocusEventArgs(this, false));
                EntryDNS.TextBox_Unfocused(this, new FocusEventArgs(this, false));
            }
            await Task.Delay(200);
        }
        
        private async Task SetFocusIpAddress()
        {
            await Task.Delay(100);
            EntryIP.TextBox_Focused(this, new FocusEventArgs(this, true));
            await Task.Delay(200);
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

        public Button GetSaveButtonDhcpSaveButton()
        {
            return ButtonDhcpSave;
        }

        public DHCPConfig GetDhcpConfig()
        {
            var dhcpConfig = new DHCPConfig
            {
                DHCPinterface = _dhcpInterface[0]
            };
            if (DhcpPicker.SelectedIndex == 0) 
                return dhcpConfig;
            dhcpConfig.ip_address = EntryIP.Text;
            dhcpConfig.routers = EntryGateway.Text;
            dhcpConfig.domain_name_servers = EntryDNS.Text;

            return dhcpConfig;
        }
    }
}