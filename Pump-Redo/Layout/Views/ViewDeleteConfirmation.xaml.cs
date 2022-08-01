using System;
using Pump.IrrigationController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewDeleteConfirmation : ContentView
    {
        private readonly CustomSchedule _schedule;
        public ViewDeleteConfirmation(CustomSchedule schedule)
        {
            InitializeComponent();
            _schedule = schedule;
            ScheduleName.Text = schedule.NAME;
            DeleteScheduleButton.AutomationId = schedule.Id;
        }

        public ViewDeleteConfirmation(IrrigationController.Schedule schedule)
        {
            InitializeComponent();
            ScheduleName.Text = schedule.NAME;
            DeleteScheduleButton.AutomationId = schedule.Id;
        }

        public Button GetDeleteButton()
        {
            return DeleteScheduleButton;
        }

        private void ButtonCancel_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        public CustomSchedule GetCustomSchedule()
        {
            return _schedule;
        }
    }
}