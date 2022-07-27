using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Timers;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewStatus : ContentView
    {
        private ControllerStatus _status;
        private readonly Timer _timer;

        public ViewStatus(ControllerStatus status)
        {
            InitializeComponent();
            _status = status;
            _timer = new Timer(1000);
            _timer.Elapsed += TimerOnElapsed;

            
            //observableFilteredIrrigation.ObservableUnfilteredIrrigation.ControllerStatusList.CollectionChanged += ControllerStatusListOnCollectionChanged;
            
            //var status = observableFilteredIrrigation.ObservableUnfilteredIrrigation.ControllerStatusList.FirstOrDefault(x =>
            //    x.EntityType + x.Id == key);
            //if(status != null)
            //    UpdateControllerStatus(status);
            
        }

        private void UpdateControllerStatus(ControllerStatus status)
        {
            _timer.Enabled = !status.Complete;

            if (status.Complete)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Transceiver1.IsVisible = false;
                    Transceiver2.IsVisible = false;
                    ImageFailed.IsVisible = status.Failed;
                    TransceiverSuccess.IsVisible = !status.Failed;
                });
            }
            
        }
        
        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Transceiver1.IsVisible = !Transceiver1.IsVisible;
                Transceiver2.IsVisible = !Transceiver2.IsVisible;
            });
        }
        

        
        
        
    }
}