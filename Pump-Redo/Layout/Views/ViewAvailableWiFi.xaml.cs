using System;
using System.Collections.Generic;
using Pump.IrrigationController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewAvailableWiFi : ContentView
    {
        private readonly List<WiFiContainer> _WiFiContainers;

        public ViewAvailableWiFi(List<WiFiContainer> WiFiContainers)
        {
            InitializeComponent();
            _WiFiContainers = WiFiContainers;
            Populate();
        }

        public void Populate()
        {
            foreach (var wiFi in _WiFiContainers) ScrollViewWiFiDetail.Children.Add(new ViewWiFi(wiFi));
        }

        private void ButtonCancel_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        public IEnumerable<View> GetChildren()
        {
            return ScrollViewWiFiDetail.Children;
        }
    }
}