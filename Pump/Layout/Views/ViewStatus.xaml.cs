using System;
using System.Linq;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Timers;
using Rg.Plugins.Popup.Services;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewStatus
    {
        private ControllerStatus _status;
        private readonly Timer _timer;
        private readonly PopupControllerStatus _popupControllerStatus;
        public ViewStatus(ControllerStatus status)
        {
            InitializeComponent();
            _timer = new Timer(500);
            _timer.Elapsed += TimerOnElapsed;
            _popupControllerStatus = new PopupControllerStatus(status);
            UpdateView(status);
        }

        public void UpdateView(ControllerStatus status)
        {
            _status = status;
            _timer.Enabled = !_status.Complete;

            Device.BeginInvokeOnMainThread(() =>
            {
                if (_status.Complete)
                {
                    Transceiver1.IsVisible = false;
                        Transceiver2.IsVisible = false;
                        ImageFailed.IsVisible = _status.Failed;
                        TransceiverSuccess.IsVisible = !_status.Failed;
                }
                else
                {
                    Transceiver1.IsVisible = false;
                    Transceiver2.IsVisible = false;
                    ImageFailed.IsVisible = false;
                    TransceiverSuccess.IsVisible = false;
                }
            });
            _popupControllerStatus.Populate(status);
        }
        
        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (Transceiver1.IsVisible == Transceiver2.IsVisible)
                {
                    Transceiver2.IsVisible = !Transceiver1.IsVisible;
                }
                Transceiver1.IsVisible = !Transceiver1.IsVisible;
                Transceiver2.IsVisible = !Transceiver2.IsVisible;
            });
        }

        private async void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
        {
            if(PopupNavigation.Instance.PopupStack.FirstOrDefault(x => x.GetType() == typeof(PopupControllerStatus)) != null)
                return;
            _popupControllerStatus.Populate(_status);
            await PopupNavigation.Instance.PushAsync(_popupControllerStatus);
        }
    }
}