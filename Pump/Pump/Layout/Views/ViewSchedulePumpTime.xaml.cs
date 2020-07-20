using System;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.MaskedEntry;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSchedulePumpTime : ContentView
    {
        public ViewSchedulePumpTime(string pumpName, bool isEdit)
        {
            InitializeComponent();
            PumpPicker.Items.Add(pumpName);
            PumpPicker.SelectedIndex = 0;
            if (isEdit)
                ButtonEditSchedulePump.Text = "EDIT SCHEDULE";
            else
                ButtonEditSchedulePump.Text = "CREATE SCHEDULE";
        }

        private void ButtonCancel_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        public MaskedEntry getPumpDurationTime()
        {
            return MaskedEntryTime;
        }

        public Button GetPumpDurationButton()
        {
            return ButtonEditSchedulePump;
        }

        public Picker getPumpPicker()
        {
            return PumpPicker;
        }
    }
}