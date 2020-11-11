using System;
using System.Collections.ObjectModel;
using Pump.IrrigationController;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingPageHomeScreen : ContentPage
    {
        private readonly ObservableCollection<Equipment> _equipmentList;
        private readonly ObservableCollection<Sensor> _sensorList;
        private readonly ObservableCollection<SubController> _subControllerList;
        public SettingPageHomeScreen(ObservableCollection<Equipment> equipmentList, ObservableCollection<Sensor> sensorList, ObservableCollection<SubController> subControllerList)
        {
            _equipmentList = equipmentList;
            _sensorList = sensorList;
            _subControllerList = subControllerList;
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
            Navigation.PushModalAsync(new EquipmentScreen(_equipmentList, _sensorList, _subControllerList));
        }

        public Button GetSiteButton()
        {
            return BtnSites;
        }
    }
}