using System;
using Pump.IrrigationController;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Dashboard
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingPageHomeScreen : ContentView
    {
        private readonly ObservableIrrigation _observableIrrigation;
        private readonly ObservableSiteFilteredIrrigation _observableSiteFilteredIrrigation;
        private readonly SocketPicker _socketPicker;

        public SettingPageHomeScreen(ObservableIrrigation observableIrrigation,
            ObservableSiteFilteredIrrigation observableSiteFilteredIrrigation, SocketPicker socketPicker)
        {
            InitializeComponent();
            _socketPicker = socketPicker;
            _observableSiteFilteredIrrigation = observableSiteFilteredIrrigation;
            _observableIrrigation = observableIrrigation;
        }

        private void BtnRecordSummary_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new RecordScreen(_observableSiteFilteredIrrigation));
        }

        private void BtnEquipmentDetail_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new EquipmentScreen(_observableIrrigation, _observableSiteFilteredIrrigation,
                _socketPicker));
        }

        public Button GetSiteButton()
        {
            return BtnSites;
        }
    }
}