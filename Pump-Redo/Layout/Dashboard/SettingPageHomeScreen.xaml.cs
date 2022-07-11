using System;
using System.Collections.Generic;
using Pump.Database.Table;
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
        private readonly KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation> _observableFilterKeyValuePair;
        private readonly SocketPicker _socketPicker;

        public SettingPageHomeScreen(ObservableIrrigation observableIrrigation,
            KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation> observableFilterKeyValuePair, SocketPicker socketPicker)
        {
            InitializeComponent();
            _socketPicker = socketPicker;
            _observableFilterKeyValuePair = observableFilterKeyValuePair;
            _observableIrrigation = observableIrrigation;
        }

        private void BtnRecordSummary_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new RecordScreen(_observableFilterKeyValuePair.Value));
        }

        private void BtnEquipmentDetail_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new EquipmentScreen(_observableFilterKeyValuePair,
                _socketPicker));
        }

        public Button GetSiteButton()
        {
            return BtnSites;
        }
    }
}