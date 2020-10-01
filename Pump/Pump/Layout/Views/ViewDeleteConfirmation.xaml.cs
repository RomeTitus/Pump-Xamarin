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
        private readonly SocketCommands _command = new SocketCommands();
        private readonly IReadOnlyList<string> _schedule;
        private readonly SocketMessage _socket = new SocketMessage();

        public ViewDeleteConfirmation(IReadOnlyList<string> _schedule)
        {
            InitializeComponent();
            this._schedule = _schedule;
            ScheduleName.Text = _schedule[5];
            DeleteScheduleButton.Clicked += DeleteScheduleButton_OnClicked;
        }

        public ViewDeleteConfirmation(CustomSchedule schedule)
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

        private void DeleteScheduleButton_OnClicked(object sender, EventArgs e)
        {
            DeleteSchedule(_schedule[3]);
            PopupNavigation.Instance.PopAsync();
            Navigation.PopModalAsync();
            Navigation.PushModalAsync(new ViewScheduleHomeScreen());
        }

        private void DeleteSchedule(string id)
        {
            if (!new DatabaseController().IsRealtimeFirebaseSelected())
                _socket.Message(_command.deleteSchedule(id));
            else
            {
                new Authentication().DeleteSchedule(new Schedule {ID = id});
            }
        }
    }
}