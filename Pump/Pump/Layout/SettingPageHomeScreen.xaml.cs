using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingPageHomeScreen : ContentPage
    {
        public SettingPageHomeScreen()
        {
            InitializeComponent();
        }

        private void BtnConnectionDetail_OnPressed(object sender, EventArgs e)
        {
            Navigation.PopAsync();
            Navigation.PushModalAsync(new ConnectionScreen());
        }

        private void BtnScheduleDetail_OnPressed(object sender, EventArgs e)
        {
            //Navigation.PopAsync();
            Navigation.PushModalAsync(new ViewScheduleHomeScreen());
        }

        private void BtnGraphSummary_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new ViewGraphSummaryScreen());
        }
    }
}