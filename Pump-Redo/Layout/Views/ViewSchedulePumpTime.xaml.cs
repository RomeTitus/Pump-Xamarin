using System;
using Pump.IrrigationController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.MaskedEntry;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSchedulePumpTime : ContentView
    {
        private Equipment _equipment;
        private IrrigationController.Schedule _schedule;

        public ViewSchedulePumpTime(IrrigationController.Schedule schedule, Equipment equipment)
        {
            InitializeComponent();
            _schedule = schedule;
            _equipment = equipment;
            PumpPicker.Items.Add(equipment.NAME);
            PumpPicker.SelectedIndex = 0;
            ButtonEditSchedulePump.Text = schedule.Id != null ? "Save" : "CREATE SCHEDULE";
        }

        private void ButtonCancel_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        public MaskedEntry GetPumpDurationTime()
        {
            return MaskedEntryTime;
        }

        public Button GetPumpDurationButton()
        {
            return ButtonEditSchedulePump;
        }
    }
}