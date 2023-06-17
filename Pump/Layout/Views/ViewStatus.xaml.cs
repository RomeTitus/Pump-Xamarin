using System;
using System.Collections.Generic;
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
        
        private readonly List<Image> _imageList;
        public ViewStatus(ControllerStatus status)
        {
            InitializeComponent();
            VerticalOptions = LayoutOptions.CenterAndExpand;
            HorizontalOptions = LayoutOptions.Center;
            _timer = new Timer(500);
            _timer.Elapsed += TimerOnElapsed;
            _popupControllerStatus = new PopupControllerStatus(status);
            _imageList = new List<Image>{Transceiver1, Transceiver2, TimeWarningFailed, ImageFailed, TransceiverSuccess};
            UpdateView(status);
        }

        public void UpdateView(ControllerStatus status)
        {
            _status = status;
            _timer.Enabled = !_status.Complete;

            Device.BeginInvokeOnMainThread(() =>
            {
                _imageList.ForEach(x => x.IsVisible = false);
                if (!_status.Complete)
                    return;

                switch (_status.StatusType)
                {
                    case StatusTypeEnum.Success:
                        TransceiverSuccess.IsVisible = true;
                        break;
                    case StatusTypeEnum.ReachFail or StatusTypeEnum.UnknownFailure:
                        ImageFailed.IsVisible = true;
                        break;
                    case StatusTypeEnum.TimeAdjusted:
                        TimeWarningFailed.IsVisible = true;
                        break;
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