using System;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingPageHomeScreen : ContentPage
    {
        private readonly ObservableIrrigation _observableIrrigation;
        public SettingPageHomeScreen(ObservableIrrigation observableIrrigation)
        {
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
            Navigation.PushModalAsync(new EquipmentScreen(_observableIrrigation));
        }

        public Button GetSiteButton()
        {
            return BtnSites;
        }
    }
}