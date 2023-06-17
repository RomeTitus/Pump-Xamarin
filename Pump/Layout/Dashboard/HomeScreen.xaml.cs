using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EmbeddedImages;
using Pump.Class;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.SocketController;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Dashboard
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeScreen
    {
        private readonly KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation>
            _observableFilterKeyValuePair;

        private readonly ObservableIrrigation _observableIrrigation;
        private readonly SocketPicker _socketPicker;
        private SettingPageHomeScreen _settingPageHomeScreen;
        private readonly ControllerSignalEvent _controllerSignalEvent;

        public HomeScreen(
            KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation> observableFilterKeyValuePair,
            SocketPicker socketPicker, ControllerSignalEvent controllerSignalEvent)
        {
            _observableFilterKeyValuePair = observableFilterKeyValuePair;
            _observableIrrigation = observableFilterKeyValuePair.Value.ObservableUnfilteredIrrigation;
            _socketPicker = socketPicker;
            _controllerSignalEvent = controllerSignalEvent;
            _controllerSignalEvent.StatusChanged += _controllerSignalEvent_StatusChanged;
            InitializeComponent();
            SetSiteName();
            SetSignalStatus(_controllerSignalEvent);
            SetUpNavigationPage();
        }

        private void SetSiteName()
        {
            var controllerPairs = _observableFilterKeyValuePair.Key.ControllerPairs;
            var subControllers = _observableFilterKeyValuePair.Value.SubControllerList;
            if(subControllers.Count == 0)
            {
                LabelSite.Text = controllerPairs.First(x => x.Value.Contains("MainController")).Key;
            }
            else
            {
                LabelSite.Text = controllerPairs.First(x => x.Value.Contains(subControllers.First().Id)).Key;
            }

        }

        private void _controllerSignalEvent_StatusChanged(object sender, System.EventArgs e)
        {
            var signalEvent = (ControllerSignalEvent)sender;
            SetSignalStatus(signalEvent);
        }

        private void SetSignalStatus(ControllerSignalEvent signalEvent)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var signalStatus = signalEvent.GetSignalStrengthy();
                if (signalStatus.signalVerified != null)
                {
                    TabPageMain.BackgroundColor = signalStatus.signalVerified.Value ? Color.DeepSkyBlue : Color.Crimson;
                    SetSignalStrength(signalStatus.signalVerified.Value ? 5 : 0);
                }
                else
                    SetSignalStrength(signalStatus.signalStrength);
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

        private void SetUpNavigationPage()
        {
            var scheduleStatusHomeScreen = new ScheduleStatusHomeScreen(_observableFilterKeyValuePair);
            var manualScheduleHomeScreen = new ManualScheduleHomeScreen(_observableFilterKeyValuePair, _socketPicker);
            var customScheduleHomeScreen = new CustomScheduleHomeScreen(_observableFilterKeyValuePair, _socketPicker);
            var scheduleHomeScreen = new ScheduleHomeScreen(_observableFilterKeyValuePair, _socketPicker);
            _settingPageHomeScreen =
                new SettingPageHomeScreen(_observableIrrigation, _observableFilterKeyValuePair, _socketPicker);

            var navigationScheduleStatusHomeScreen = new TabViewItem
            {
                Content = scheduleStatusHomeScreen,
                Text = "Summary",
                TextColor = Color.AliceBlue,
                Icon = ImageSource.FromResource(
                    "Pump.Icons.Home.png",
                    typeof(ImageResourceExtension).GetTypeInfo().Assembly)
            };

            var navigationManualScheduleHomeScreen = new TabViewItem
            {
                Content = manualScheduleHomeScreen,
                Text = "Manual",
                TextColor = Color.AliceBlue,
                Icon = ImageSource.FromResource(
                    "Pump.Icons.ManualSchedule.png",
                    typeof(ImageResourceExtension).GetTypeInfo().Assembly)
            };

            var navigationCustomScheduleHomeScreen = new TabViewItem
            {
                Content = customScheduleHomeScreen,
                Text = "Custom",
                TextColor = Color.AliceBlue,
                Icon = ImageSource.FromResource(
                    "Pump.Icons.CustomSchedule.png",
                    typeof(ImageResourceExtension).GetTypeInfo().Assembly)
            };

            var navigationScheduleHomeScreen = new TabViewItem
            {
                Content = scheduleHomeScreen,
                Text = "Schedule",
                TextColor = Color.AliceBlue,
                Icon = ImageSource.FromResource(
                    "Pump.Icons.FieldSun.png",
                    typeof(ImageResourceExtension).GetTypeInfo().Assembly)
            };

            var navigationSettingPageHomeScreen = new TabViewItem
            {
                Content = _settingPageHomeScreen,
                Text = "Settings",
                TextColor = Color.AliceBlue,
                Icon = ImageSource.FromResource(
                    "Pump.Icons.setting.png",
                    typeof(ImageResourceExtension).GetTypeInfo().Assembly)
            };

            TabViewHome.TabItems.Add(navigationScheduleStatusHomeScreen);
            TabViewHome.TabItems.Add(navigationManualScheduleHomeScreen);
            TabViewHome.TabItems.Add(navigationCustomScheduleHomeScreen);
            TabViewHome.TabItems.Add(navigationScheduleHomeScreen);
            TabViewHome.TabItems.Add(navigationSettingPageHomeScreen);
        }
    }
}