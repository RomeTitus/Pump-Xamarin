using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Timers;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewStatus
    {
        private ControllerStatus _status;
        private readonly Timer _timer;

        public ViewStatus(ControllerStatus status)
        {
            InitializeComponent();
            _timer = new Timer(500);
            _timer.Elapsed += TimerOnElapsed;
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
    }
}