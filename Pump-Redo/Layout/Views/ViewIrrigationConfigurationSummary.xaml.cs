using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Pump.Class;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewIrrigationConfigurationSummary
    {
        private readonly KeyValuePair<IrrigationConfiguration, ObservableIrrigation> _keyValueIrrigation;
        private readonly SocketPicker _socketPicker;
        
        public ViewIrrigationConfigurationSummary(KeyValuePair<IrrigationConfiguration, ObservableIrrigation> keyValueIrrigation, SocketPicker socketPicker)
        {
            InitializeComponent();
            _keyValueIrrigation = keyValueIrrigation;
            _socketPicker = socketPicker;
            AutomationId = keyValueIrrigation.Key.Path;
            Device.BeginInvokeOnMainThread(() =>
            {
                if (Device.RuntimePlatform == Device.UWP)
                    ActivityIndicatorUwpLoadingIndicator.IsVisible = true;
                else
                    ActivityIndicatorMobileLoadingIndicator.IsVisible = true;
                LabelControllerConfigName.Text = keyValueIrrigation.Key.Path;
                FrameSiteSummary.BackgroundColor = Color.Coral;
            });
            
            keyValueIrrigation.Value.AliveList.CollectionChanged += subscribeToOnlineStatus;
            subscribeToOnlineStatus(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            
            SetExistingSites();
        }

        private void SetExistingSites()
        {
            foreach (var controllerPair in _keyValueIrrigation.Key.ControllerPairs
                         .Where(x => SiteChildren.Children.Select(y => y.AutomationId).Contains(x.Key) == false))
            {
                var filteredSiteSummary = new ViewIrrigationSiteSummary(_keyValueIrrigation.Value, controllerPair);
                SiteChildren.Children.Add(filteredSiteSummary);
            }
        }

        public void Populate()
        {
            foreach (var view in SiteChildren.Children)
            {
                var siteView = (ViewIrrigationSiteSummary)view;
                siteView.SetConfigurationSummary();
            }

            if (_keyValueIrrigation.Value.LoadedData)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ActivityIndicatorUwpLoadingIndicator.IsVisible = false;
                    ActivityIndicatorMobileLoadingIndicator.IsVisible = false;
                });
            }
        }
        
        public List<TapGestureRecognizer> GetTapGestureRecognizerList()
        {
            return (from ViewIrrigationSiteSummary siteView in SiteChildren.Children select siteView.GestureRecognizer()).ToList();
        }

        public TapGestureRecognizer GetTapGestureSettings()
        {
            return IrrigationControllerSettingGesture;
        }

        private async void subscribeToOnlineStatus(object sender, NotifyCollectionChangedEventArgs e)
        {
            var result = await ConnectionSuccessful();
            
            Device.BeginInvokeOnMainThread(() =>
            {
                FrameSiteSummary.BackgroundColor = result ? Color.DeepSkyBlue : Color.Crimson;
                SetSignalStrength(result? 5 : 0);
            });
            
        }

        private void SetSignalStrength(int signal)
        {
            ImageSignalStrengthNoSignal.IsVisible = signal == 0;
            ImageSignalStrength1.IsVisible = signal == 1;
            ImageSignalStrength2.IsVisible = signal == 2;
            ImageSignalStrength3.IsVisible = signal == 3;
            ImageSignalStrength4.IsVisible = signal == 4;
            ImageSignalStrength5.IsVisible = signal == 5;
        }

        private async Task<bool> ConnectionSuccessful()
        {
            int signalStrength = 2;
            _keyValueIrrigation.Value.AliveList.CollectionChanged -= subscribeToOnlineStatus;
            var oldTime = ScheduleTime.GetUnixTimeStampUtcNow();
            var now = ScheduleTime.GetUnixTimeStampUtcNow();
            var requestedOnlineStatus = false;
            var delay = 16;
            while (now < oldTime + delay) //seconds Delay
            {
                //See if Requested in Greater than response :/
                var aliveStatus = _keyValueIrrigation.Value.AliveList.FirstOrDefault();

                //No Point in trying to request OnlineStatus if someone else has already tried and failed 1-delay seconds ago
                if (aliveStatus?.ResponseTime < aliveStatus?.RequestedTime - delay &&
                    aliveStatus.RequestedTime > now - delay && !requestedOnlineStatus)
                {
                    _keyValueIrrigation.Value.AliveList.CollectionChanged += subscribeToOnlineStatus;
                    return false;
                }

                if (aliveStatus?.ResponseTime <= now - 600 &&
                    aliveStatus.ResponseTime >=
                    aliveStatus.RequestedTime) // 10 Minutes before We try Request Online Status Again
                {
                    aliveStatus.RequestedTime = now;
                    requestedOnlineStatus = true;
                    await _socketPicker.SendCommand(aliveStatus, _keyValueIrrigation.Key);
                    oldTime = now;
                }
                else if (aliveStatus?.ResponseTime >= aliveStatus?.RequestedTime && aliveStatus.ResponseTime >= now - 599)
                {
                    _keyValueIrrigation.Value.AliveList.CollectionChanged += subscribeToOnlineStatus;
                    return true;
                }

                now = ScheduleTime.GetUnixTimeStampUtcNow();
                await Task.Delay(400);
                SetSignalStrength(signalStrength);
                signalStrength++;
                if (signalStrength > 5)
                    signalStrength = 1;
            }

            _keyValueIrrigation.Value.AliveList.CollectionChanged += subscribeToOnlineStatus;
            return false;
        }

        public KeyValuePair<IrrigationConfiguration, ObservableIrrigation> GetIrrigationConfigAndObservable()
        {
            return _keyValueIrrigation;
        }

        public KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation>  GetIrrigationFilterConfigAndObservable(ObservableFilteredIrrigation observableFiltered)
        {
            return new KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation>(_keyValueIrrigation.Key,
                observableFiltered);
        }

        public void ReLoadChildren()
        {
            SiteChildren.Children.Clear();
            SetExistingSites();
        }
    }
}