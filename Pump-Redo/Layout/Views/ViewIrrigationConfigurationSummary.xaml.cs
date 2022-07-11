using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
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
        private readonly ObservableFilteredIrrigation _observableFiltered;
        private readonly SocketPicker _socketPicker;
        public ViewIrrigationConfigurationSummary(KeyValuePair<IrrigationConfiguration, ObservableIrrigation> keyValueIrrigation, SocketPicker socketPicker, string siteKey = null)
        {
            InitializeComponent();
            if (Device.RuntimePlatform == Device.UWP)
                ActivityIndicatorUwpLoadingIndicator.IsVisible = true;
            else
                ActivityIndicatorMobileLoadingIndicator.IsVisible = true;
            
            
            _keyValueIrrigation = keyValueIrrigation;
            _socketPicker = socketPicker;
            ImageSetting.IsVisible = siteKey == null;
            FrameSchedule.IsVisible = siteKey != null || keyValueIrrigation.Key.ControllerPairs.Count == 1;

            if (siteKey != null)
            {
                AutomationId = siteKey;
                LabelSiteName.Text = siteKey;
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    AutomationId = keyValueIrrigation.Key.ControllerPairs.Keys.First();
                    LabelSiteName.Text = keyValueIrrigation.Key.ControllerPairs.Keys.First();
                    FrameSiteSummary.BackgroundColor = Color.Coral;
                    keyValueIrrigation.Value.AliveList.CollectionChanged += subscribeToOnlineStatus;
                    subscribeToOnlineStatus(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                });
                
            }

            if(siteKey == null && keyValueIrrigation.Key.ControllerPairs.Count == 1)
                _observableFiltered = new ObservableFilteredIrrigation(keyValueIrrigation.Value, keyValueIrrigation.Key.ControllerPairs.Values.First());
            else if(siteKey != null)
            {
                _observableFiltered = new ObservableFilteredIrrigation(keyValueIrrigation.Value, keyValueIrrigation.Key.ControllerPairs.First(x => x.Key == siteKey).Value);
            }

            if (siteKey == null)
            {
                SetSites();
            }
        }

        private void SetSites()
        {
            if (_keyValueIrrigation.Key.ControllerPairs.Count <= 1)
                return;

            foreach (var controllerPair in _keyValueIrrigation.Key.ControllerPairs
                         .Where(x => SiteChildren.Children.Select(y => y.AutomationId).Contains(x.Key) == false))
            {
                var filteredSiteSummary = new ViewIrrigationConfigurationSummary(_keyValueIrrigation, _socketPicker, controllerPair.Key);
                SiteChildren.Children.Add(filteredSiteSummary);
            }
        }

        public void Populate()
        {
            foreach (var view in SiteChildren.Children)
            {
                var siteView = (ViewIrrigationConfigurationSummary)view;
                siteView.SetConfigurationSummary();
            }

            SetConfigurationSummary();
        }
        private void SetConfigurationSummary()
        {
            if(_keyValueIrrigation.Value.LoadedData == false)
                return;
            
            Device.BeginInvokeOnMainThread(() =>
            {
                if (Device.RuntimePlatform == Device.UWP)
                    ActivityIndicatorUwpLoadingIndicator.IsVisible = false;
                else
                    ActivityIndicatorMobileLoadingIndicator.IsVisible = false;

            });

            if(_observableFiltered == null)
                return;
            
            var scheduleRunning = RunningCustomSchedule.GetCustomScheduleDetailRunningList(_observableFiltered.CustomScheduleList.ToList()).Any();

            if (!scheduleRunning)
            {
                scheduleRunning = new RunningSchedule(_observableFiltered.ScheduleList.ToList(),
                        _observableFiltered.EquipmentList.ToList()).GetRunningSchedule().Any();
            }

            if (!scheduleRunning)
            {
                scheduleRunning = _observableFiltered.ManualScheduleList.Any();
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                SetScheduleRunning(scheduleRunning);

                if (_observableFiltered.SensorList.Any())
                    PressureSensor(_observableFiltered.SensorList.ToList());

            });
        }

        private void PressureSensor(List<Sensor> sensorList)
        {
            var sensor = sensorList.FirstOrDefault(x => x.TYPE == "Pressure Sensor");
            if(sensor == null)
                return;
            
            LabelPressure.IsVisible = true;
            var reading = Convert.ToDouble(sensor.LastReading, CultureInfo.InvariantCulture);

            var voltage = reading * 5.0 / 1024.0;

            var pressurePascal = 3.0 * (voltage - 0.47) * 1000000.0;

            var bars = pressurePascal / 10e5;
            LabelPressure.Text = bars.ToString("0.##") + " Bar";
        }
        
        private void SetScheduleRunning(bool running)
        {
            FrameScheduleStatus.BackgroundColor = running ? Color.LawnGreen : Color.White;
        }

        public void SetBackgroundColor(Color color)
        {
            FrameSiteSummary.BackgroundColor = color;
        }

        public List<TapGestureRecognizer> GetTapGestureRecognizerList()
        {
            var tapGestureList = new List<TapGestureRecognizer> { StackLayoutViewSiteTapGesture };
            foreach (var view in SiteChildren.Children)
            {
                var siteView = (ViewIrrigationConfigurationSummary)view;
                tapGestureList.Add(siteView.StackLayoutViewSiteTapGesture);
            }

            return tapGestureList;
        }
        
        public TapGestureRecognizer GetTapGestureSettings()
        { 
            return IrrigationControllerSettingGesture;
        }
        private async void subscribeToOnlineStatus(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_keyValueIrrigation.Value.AliveList.Any())
            {
                var result = await ConnectionSuccessful();
                Device.BeginInvokeOnMainThread(() =>
                {
                    FrameSiteSummary.BackgroundColor = result ? Color.DeepSkyBlue : Color.Crimson;
                });
            }
        }
        
        private async Task<bool> ConnectionSuccessful()
        {
            _keyValueIrrigation.Value.AliveList.CollectionChanged -= subscribeToOnlineStatus;
            var oldTime = ScheduleTime.GetUnixTimeStampUtcNow();
            var now = ScheduleTime.GetUnixTimeStampUtcNow();
            var requestedOnlineStatus = false;
            var delay = 16;
            while (now < oldTime + delay) //seconds Delay
            {
                //See if Requested in Greater than response :/
                var aliveStatus = _keyValueIrrigation.Value.AliveList.First();

                //No Point in trying to request OnlineStatus if someone else has already tried and failed 1-delay seconds ago
                if (aliveStatus.ResponseTime < aliveStatus.RequestedTime - delay &&
                    aliveStatus.RequestedTime > now - delay && !requestedOnlineStatus)
                {
                    _keyValueIrrigation.Value.AliveList.CollectionChanged += subscribeToOnlineStatus;
                    return false;
                }
                if (aliveStatus.ResponseTime <= now - 600 &&
                    aliveStatus.ResponseTime >=
                    aliveStatus.RequestedTime) // 10 Minutes before We try Request Online Status Again
                {
                    aliveStatus.RequestedTime = now;
                    requestedOnlineStatus = true;
                    await _socketPicker.SendCommand(aliveStatus, _keyValueIrrigation.Key);
                    oldTime = now;
                }
                else if (aliveStatus.ResponseTime >= aliveStatus.RequestedTime && aliveStatus.ResponseTime >= now - 599)
                {
                    _keyValueIrrigation.Value.AliveList.CollectionChanged += subscribeToOnlineStatus;
                    return true;
                }

                now = ScheduleTime.GetUnixTimeStampUtcNow();
                await Task.Delay(400);
            }

            _keyValueIrrigation.Value.AliveList.CollectionChanged += subscribeToOnlineStatus;
            return false;
        }

        public KeyValuePair<IrrigationConfiguration, ObservableIrrigation> GetIrrigationConfigAndObservable()
        {
            return _keyValueIrrigation;
        }

        public KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation> GetIrrigationFilterConfigAndObservable()
        {
            return new KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation>(_keyValueIrrigation.Key, _observableFiltered);
        }
    }
}