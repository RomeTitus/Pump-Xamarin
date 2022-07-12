using System.Collections.Generic;
using System.Reflection;
using EmbeddedImages;
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

        public HomeScreen(
            KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation> observableFilterKeyValuePair,
            SocketPicker socketPicker)
        {
            _observableFilterKeyValuePair = observableFilterKeyValuePair;
            _observableIrrigation = observableFilterKeyValuePair.Value.ObservableUnfilteredIrrigation;
            _socketPicker = socketPicker;
            InitializeComponent();
            SetUpNavigationPage();
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