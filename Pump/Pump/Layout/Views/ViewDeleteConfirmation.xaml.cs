using System;
using System.Collections.Generic;
using Pump.Database;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewDeleteConfirmation : ContentView
    {
        public ViewDeleteConfirmation(CustomSchedule schedule)
        {
            InitializeComponent();
            ScheduleName.Text = schedule.NAME;
            DeleteScheduleButton.AutomationId = schedule.ID;
        }
        public ViewDeleteConfirmation(IrrigationController.Schedule schedule)
        {
            InitializeComponent();
            ScheduleName.Text = schedule.NAME;
            DeleteScheduleButton.AutomationId = schedule.ID;
        }

        public Button GetDeleteButton()
        {
            return DeleteScheduleButton;
        }

        private void ButtonCancel_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }
    }
}