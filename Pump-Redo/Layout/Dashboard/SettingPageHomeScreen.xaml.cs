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
        private readonly ObservableSiteIrrigation _observableSiteIrrigation;
        private readonly SocketPicker _socketPicker;

        public SettingPageHomeScreen(ObservableIrrigation observableIrrigation,
            ObservableSiteIrrigation observableSiteIrrigation, SocketPicker socketPicker)
        {
            InitializeComponent();
            _socketPicker = socketPicker;
            _observableSiteIrrigation = observableSiteIrrigation;
            _observableIrrigation = observableIrrigation;
        }

        private void BtnRecordSummary_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new RecordScreen(_observableSiteIrrigation));
        }

        private void BtnEquipmentDetail_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new EquipmentScreen(_observableIrrigation, _observableSiteIrrigation,
                _socketPicker));
        }

        public Button GetSiteButton()
        {
            return BtnSites;
        }
    }
}