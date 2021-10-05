using System;
using System.Collections.Generic;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AvailableWiFi : ContentPage
    {
        private List<WiFiContainer> _WiFiContainers;

        public AvailableWiFi(List<WiFiContainer> WiFiContainers)
        {
            InitializeComponent();
            _WiFiContainers = WiFiContainers;
            Populate();
        }

        public void Populate()
        {
            foreach (var wiFi in _WiFiContainers)
            {
                ScrollViewWiFiDetail.Children.Add(new ViewWiFi(wiFi));
            }
        }

        private void ButtonCancel_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        public IList<View> GetChildren()
        {
            return ScrollViewWiFiDetail.Children;
        }
    }
}