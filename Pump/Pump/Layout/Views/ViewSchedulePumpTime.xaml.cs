﻿using System;
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
        private Schedule _schedule;
        private  Equipment _equipment;
        public ViewSchedulePumpTime(Schedule schedule, Equipment equipment)
        {
            InitializeComponent();
            _schedule = schedule;
            _equipment = equipment;
            PumpPicker.Items.Add(equipment.NAME);
            PumpPicker.SelectedIndex = 0;
            ButtonEditSchedulePump.Text = schedule.ID != null ? "Save" : "CREATE SCHEDULE";
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