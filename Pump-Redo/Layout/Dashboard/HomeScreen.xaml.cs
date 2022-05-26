using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EmbeddedImages;
using Pump.Class;
using Pump.Database;
using Pump.IrrigationController;
using Pump.SocketController;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Dashboard
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeScreen : ContentPage
    {
        private readonly DatabaseController _databaseController = new DatabaseController();
        private readonly ObservableIrrigation _observableIrrigation;
        private readonly ObservableSiteIrrigation _observableSiteIrrigation;
        private readonly SocketPicker _socketPicker;
        private SettingPageHomeScreen _settingPageHomeScreen;

        public HomeScreen(ObservableIrrigation observableIrrigation, ObservableSiteIrrigation observableSiteIrrigation,
            SocketPicker socketPicker)
        {
            _observableIrrigation = observableIrrigation;
            _observableSiteIrrigation = observableSiteIrrigation;
            _socketPicker = socketPicker;
            InitializeComponent();
            
            subscribeToOnlineStatus(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            _observableIrrigation.AliveList.CollectionChanged += subscribeToOnlineStatus;
            HomeScreenSite(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            _observableIrrigation.SiteList.CollectionChanged += HomeScreenSite;
            SetUpNavigationPage();
        }

        private void SetUpNavigationPage()
        {
            var scheduleStatusHomeScreen = new ScheduleStatusHomeScreen(_observableSiteIrrigation);
            var manualScheduleHomeScreen = new ManualScheduleHomeScreen(_observableSiteIrrigation, _socketPicker);
            var customScheduleHomeScreen = new CustomScheduleHomeScreen(_observableSiteIrrigation, _socketPicker);
            var scheduleHomeScreen = new ScheduleHomeScreen(_observableSiteIrrigation, _socketPicker);
            _settingPageHomeScreen =
                new SettingPageHomeScreen(_observableIrrigation, _observableSiteIrrigation, _socketPicker);

            var navigationScheduleStatusHomeScreen = new TabViewItem
            {
                Content = scheduleStatusHomeScreen,
                Text = "Summary",
                TextColor = Color.AliceBlue,
                Icon = ImageSource.FromResource(
                    "Pump-Redo.Icons.Home.png",
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly)
            };

            var navigationManualScheduleHomeScreen = new TabViewItem
            {
                Content = manualScheduleHomeScreen,
                Text = "Manual",
                TextColor = Color.AliceBlue,
                Icon = ImageSource.FromResource(
                    "Pump-Redo.Icons.ManualSchedule.png",
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly)
            };

            var navigationCustomScheduleHomeScreen = new TabViewItem
            {
                Content = customScheduleHomeScreen,
                Text = "Custom",
                TextColor = Color.AliceBlue,
                Icon = ImageSource.FromResource(
                    "Pump-Redo.Icons.CustomSchedule.png",
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly)
            };

            var navigationScheduleHomeScreen = new TabViewItem
            {
                Content = scheduleHomeScreen,
                Text = "Schedule",
                TextColor = Color.AliceBlue,
                Icon = ImageSource.FromResource(
                    "Pump-Redo.Icons.FieldSun.png",
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly)
            };

            var navigationSettingPageHomeScreen = new TabViewItem
            {
                Content = _settingPageHomeScreen,
                Text = "Settings",
                TextColor = Color.AliceBlue,
                Icon = ImageSource.FromResource(
                    "Pump-Redo.Icons.setting.png",
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly)
            };

            TabViewHome.TabItems.Add(navigationScheduleStatusHomeScreen);
            TabViewHome.TabItems.Add(navigationManualScheduleHomeScreen);
            TabViewHome.TabItems.Add(navigationCustomScheduleHomeScreen);
            TabViewHome.TabItems.Add(navigationScheduleHomeScreen);
            TabViewHome.TabItems.Add(navigationSettingPageHomeScreen);
            //TabViewHome.SelectedIndex = 0;
        }

        public Button GetSiteButton()
        {
            return _settingPageHomeScreen.GetSiteButton();
        }

        private async void subscribeToOnlineStatus(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_observableIrrigation.AliveList.Any() && !_observableIrrigation.AliveList.Contains(null))
            {
                var result = await ConnectionSuccessful();
                Device.BeginInvokeOnMainThread(() =>
                {
                    SignalImage.Source = ImageSource.FromResource(
                        result ? "Pump-Redo.Icons.Signal_5.png" : "Pump-Redo.Icons.Signal_NoSignal.png",
                        typeof(ImageResourceExtention).GetTypeInfo().Assembly);
                    BackgroundColor = result ? Color.DeepSkyBlue : Color.Crimson;
                });
            }
        }

        private void HomeScreenSite(object sender, NotifyCollectionChangedEventArgs e)
        {
            var site = _observableIrrigation.SiteList.FirstOrDefault(x =>
                x?.ID == _databaseController.GetControllerConnectionSelection().SiteSelectedId);
            if (site == null || LabelSite.Text == site.NAME)
                return;
            Device.BeginInvokeOnMainThread(() => { LabelSite.Text = site.NAME; });
        }

        private async Task<bool> ConnectionSuccessful()
        {
            _observableIrrigation.AliveList.CollectionChanged -= subscribeToOnlineStatus;
            var oldTime = ScheduleTime.GetUnixTimeStampUtcNow();
            var now = ScheduleTime.GetUnixTimeStampUtcNow();
            var count = 1;
            var requestedOnlineStatus = false;
            var delay = 16;
            while (now < oldTime + delay) //seconds Delay
            {
                //See if Requested in Greater than response :/
                var aliveStatus = _observableIrrigation.AliveList.First();

                //No Point in trying to request OnlineStatus if someone else has already tried and failed 1-delay seconds ago
                if (aliveStatus.ResponseTime < aliveStatus.RequestedTime - delay &&
                    aliveStatus.RequestedTime > now - delay && !requestedOnlineStatus)
                {
                    _observableIrrigation.AliveList.CollectionChanged += subscribeToOnlineStatus;
                    return false;
                }

                if (aliveStatus.ResponseTime <= now - 600 &&
                    aliveStatus.ResponseTime >=
                    aliveStatus.RequestedTime) // 10 Minutes before We try Request Online Status Again
                {
                    aliveStatus.RequestedTime = now;
                    requestedOnlineStatus = true;
                    await _socketPicker.SendCommand(aliveStatus);
                    oldTime = now;
                }
                else if (aliveStatus.ResponseTime >= aliveStatus.RequestedTime && aliveStatus.ResponseTime >= now - 599)
                {
                    _observableIrrigation.AliveList.CollectionChanged += subscribeToOnlineStatus;
                    return true;
                }

                now = ScheduleTime.GetUnixTimeStampUtcNow();
                var count1 = count;
                Device.BeginInvokeOnMainThread(() =>
                {
                    SignalImage.Source = ImageSource.FromResource(
                        "Pump-Redo.Icons.Signal_" + count1 + ".png",
                        typeof(ImageResourceExtention).GetTypeInfo().Assembly);
                });
                count++;
                if (count > 5)
                    count = 1;
                await Task.Delay(400);
            }

            _observableIrrigation.AliveList.CollectionChanged += subscribeToOnlineStatus;
            return false;
        }
    }
}