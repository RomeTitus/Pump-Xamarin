using System;
using Pump.IrrigationController;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingPageHomeScreen : ContentPage
    {
        private readonly ObservableIrrigation _observableIrrigation;
        private readonly SocketPicker _socketPicker;
        public SettingPageHomeScreen(ObservableIrrigation observableIrrigation, SocketPicker socketPicker)
        {
            _socketPicker = socketPicker;
            _observableIrrigation = observableIrrigation;
            InitializeComponent();
        }

        private void BtnConnectionDetail_OnPressed(object sender, EventArgs e)
        {
            Navigation.PopAsync();
            Navigation.PushModalAsync(new ConnectionScreen());
        }

        private void BtnScheduleDetail_OnPressed(object sender, EventArgs e)
        {
            
            //Navigation.PushModalAsync(new ScheduleHomeScreen());
        }

        private void BtnGraphSummary_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new GraphSummaryScreen());
        }

        private void BtnEquipmentDetail_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new EquipmentScreen(_observableIrrigation, _socketPicker));
        }

        public Button GetSiteButton()
        {
            return BtnSites;
        }
    }
}