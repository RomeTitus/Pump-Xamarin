using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewDeleteConfirmation : ContentView
    {
        private readonly IReadOnlyList<string> _schedule;
        private readonly IrrigationCommands _command = new IrrigationCommands();
        private readonly SocketMessage _socket = new SocketMessage();
        public ViewDeleteConfirmation(IReadOnlyList<string> _schedule)
        {
            InitializeComponent();
            this._schedule = _schedule;
            ScheduleName.Text = _schedule[5];
        }

        private void ButtonCancel_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        private void DeleteScheduleButton_OnClicked(object sender, EventArgs e)
        {
            DeleteSchedule(Convert.ToInt32(_schedule[3]));
            PopupNavigation.Instance.PopAsync();
            Navigation.PopModalAsync();
            Navigation.PushModalAsync(new ViewScheduleScreen());
            
        }

        private void DeleteSchedule(int id)
        {
             var result = _socket.Message(_command.deleteSchedule(id));

        }
    }
}