using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using EmbeddedImages;
using Pump.Class;
using Pump.Database;
using Pump.Database.Table;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeScreen : TabbedPage
    {
        private readonly ObservableCollection<Equipment> _equipmentList;
        private readonly ObservableCollection<Alive> _aliveList;
        private readonly ObservableCollection<Sensor> _sensorList;
        private readonly ObservableCollection<ManualSchedule> _manualScheduleList;
        private readonly ObservableCollection<Schedule> _scheduleList;
        private readonly ObservableCollection<CustomSchedule> _customScheduleList;
        private readonly ObservableCollection<Site> _siteList;
        private readonly ObservableCollection<SubController> _subController;

        private readonly DatabaseController _databaseController = new DatabaseController();
        public HomeScreen(ObservableCollection<Equipment> equipmentList, ObservableCollection<Sensor> sensorList,
            ObservableCollection<ManualSchedule> manualScheduleList, ObservableCollection<Schedule> scheduleList,
            ObservableCollection<CustomSchedule> customScheduleList, ObservableCollection<Site> siteList, ObservableCollection<Alive> aliveList,
            ObservableCollection<SubController> subController)
        {
            _equipmentList = equipmentList;
            _sensorList = sensorList;
            _manualScheduleList = manualScheduleList;
            _scheduleList = scheduleList;
            _customScheduleList = customScheduleList;
            _siteList = siteList;
            _aliveList = aliveList;
            _subController = subController;

            InitializeComponent();
            if (Device.RuntimePlatform == Device.iOS)
            {
                
            }


            if (_databaseController.GetControllerConnectionSelection() == null)
            {
                _databaseController.SetActivityStatus(new ActivityStatus(false));
                Navigation.PushModalAsync(new AddController(true));

            }
            else
            {
                if (new DatabaseController().IsRealtimeFirebaseSelected())
                    new Thread(MonitorConnectionStatus).Start();
                else
                    TabPageMain.BackgroundColor = Color.DeepSkyBlue;

              
            }



            setUpNavigationPage();
        }

        private void setUpNavigationPage()
        {
            var scheduleStatusHomeScreen = new ScheduleStatusHomeScreen(_equipmentList, _manualScheduleList, _scheduleList, _sensorList, _siteList);
            var manualScheduleHomeScreen = new ManualScheduleHomeScreen(_manualScheduleList, _equipmentList, _siteList);
            var customScheduleHomeScreen = new CustomScheduleHomeScreen(_customScheduleList, _equipmentList, _siteList);
            var scheduleHomeScreen = new ScheduleHomeScreen(_scheduleList, _equipmentList, _siteList);
            var settingPageHomeScreen = new SettingPageHomeScreen(_equipmentList, _sensorList, _subController);
            var navigationScheduleStatusHomeScreen = new NavigationPage(scheduleStatusHomeScreen)
            {
                IconImageSource = ImageSource.FromResource(
                    "Pump.Icons.Home.png",
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly)
            };

            var navigationManualScheduleHomeScreen = new NavigationPage(manualScheduleHomeScreen)
            {
                IconImageSource = ImageSource.FromResource(
                    "Pump.Icons.ManualSchedule.png",
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly)
            };

            var navigationCustomScheduleHomeScreen = new NavigationPage(customScheduleHomeScreen)
            {
                IconImageSource = ImageSource.FromResource(
                    "Pump.Icons.CustomSchedule.png",
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly)
            };

            var navigationScheduleHomeScreen = new NavigationPage(scheduleHomeScreen)
            {
                IconImageSource = ImageSource.FromResource(
                    "Pump.Icons.FieldSun.png",
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly)
            };

            var navigationSettingPageHomeScreen = new NavigationPage(settingPageHomeScreen)
            {
                IconImageSource = ImageSource.FromResource(
                    "Pump.Icons.setting.png",
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly)
            };


            Children.Add(navigationScheduleStatusHomeScreen);
            Children.Add(navigationManualScheduleHomeScreen);
            Children.Add(navigationCustomScheduleHomeScreen);
            Children.Add(navigationScheduleHomeScreen);
            Children.Add(navigationSettingPageHomeScreen);

            settingPageHomeScreen.GetSiteButton().Pressed += BtnSite_OnPressed;
        }


        private void BtnSite_OnPressed(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Navigation.PopAsync();
            });
            
        }

        private void LastOnline()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (_aliveList[0] == null)
                {
                    TabPageMain.BackgroundColor = Color.DarkOrange;
                    return;
                }
                    
                _aliveList[0].RequestedTime = ScheduleTime.GetUnixTimeStampUtcNow();
                if (_aliveList[0].ResponseTime == 0)
                {
                    TabPageMain.BackgroundColor = Color.Crimson;
                }
                else
                {
                    var now = ScheduleTime.GetUnixTimeStampUtcNow();

                    TabPageMain.BackgroundColor = _aliveList[0].ResponseTime > (now - 100) ? Color.DeepSkyBlue : Color.Crimson;
                }
            });
        }

        private void MonitorConnectionStatus()
        {
            while (true)
            {
                try
                {
                    LastOnline();

                    if (_aliveList[0] != null)
                    {

                        Device.BeginInvokeOnMainThread(() =>
                        {

                            _aliveList[0].RequestedTime = ScheduleTime.GetUnixTimeStampUtcNow();
                            if (_aliveList[0] == null || _aliveList[0].ResponseTime == 0)
                            {
                                new Authentication().SetAlive(_aliveList[0]);
                                TabPageMain.BackgroundColor = Color.Crimson;
                            }
                            else
                            {
                                var now = ScheduleTime.GetUnixTimeStampUtcNow();
                                if (_aliveList[0].ResponseTime < (now - 60))
                                    new Authentication().SetAlive(_aliveList[0]);
                                else if (_aliveList[0].ResponseTime < (now - 120))
                                    TabPageMain.BackgroundColor = Color.Coral;
                            }
                        });
                    }
                }
                catch
                {
                    // ignored
                }

                Thread.Sleep(50000);
            }
        }

    }
}