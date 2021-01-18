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
    public partial class SetupNewSystem : ContentPage
    {
        public SetupNewSystem()
        {
            InitializeComponent();
        }

        private void BtnScan_OnClicked(object sender, EventArgs e)
        {
            DisplayAlert("SCAN Thing", "Scanned something", "Understood");
            ScrollViewSetupSystem.Children.Clear();
            var loadingIcon = new ActivityIndicator
            {
                AutomationId = "ActivityIndicatorSiteLoading",
                HorizontalOptions = LayoutOptions.Center,
                IsEnabled = true,
                IsRunning = true,
                IsVisible = true,
                VerticalOptions = LayoutOptions.Center
            };
            ScrollViewSetupSystem.Children.Add(loadingIcon);
        }
    }
}