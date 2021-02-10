using System.Reflection;
using System.Threading;
using EmbeddedImages;
using Pump.Class;
using Pump.Database;
using Pump.Database.Table;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeScreen : TabbedPage
    {
        private readonly ObservableIrrigation _observableIrrigation;
        private SettingPageHomeScreen _settingPageHomeScreen;
        private bool _hasSentUpdateRequest;
        private bool _firstRun = true;
        
        private readonly DatabaseController _databaseController = new DatabaseController();
        public HomeScreen(ObservableIrrigation observableIrrigation)
        {
            _observableIrrigation = observableIrrigation;
            InitializeComponent();
            if (Device.RuntimePlatform == Device.iOS)
            {
                
            }


            if (_databaseController.GetControllerConnectionSelection() == null)
            {
                _databaseController.SetActivityStatus(new ActivityStatus(false));
                //Navigation.PushModalAsync(new AddExistingController(true));

            }
            else
            {
                if (new DatabaseController().IsRealtimeFirebaseSelected())
                    new Thread(MonitorConnectionStatus).Start();
                else
                    TabPageMain.BackgroundColor = Color.DeepSkyBlue;

              
            }



            SetUpNavigationPage();
            observableIrrigation.AliveList.CollectionChanged += subscribeToLastOnline;
            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
        }

        private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                DisplayAlert("No Internet", "We are unable to connect to the Internet \n Trying again", "Understood");
            else if(Connectivity.NetworkAccess == NetworkAccess.Internet)
                DisplayAlert("Internet", "We are Connected!", "Understood");
        }

        private void SetUpNavigationPage()
        {
            var scheduleStatusHomeScreen = new ScheduleStatusHomeScreen(_observableIrrigation);
            var manualScheduleHomeScreen = new ManualScheduleHomeScreen(_observableIrrigation);
            var customScheduleHomeScreen = new CustomScheduleHomeScreen(_observableIrrigation);
            var scheduleHomeScreen = new ScheduleHomeScreen(_observableIrrigation);
            _settingPageHomeScreen = new SettingPageHomeScreen(_observableIrrigation);
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

            var navigationSettingPageHomeScreen = new NavigationPage(_settingPageHomeScreen)
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
        }


        public Button GetSiteButton()
        {
            return _settingPageHomeScreen.GetSiteButton();
        }


        private void subscribeToLastOnline(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!_hasSentUpdateRequest) return;
            _firstRun = false;
            _hasSentUpdateRequest = false;

        }

        private void LastOnline()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                

                if (_observableIrrigation.AliveList[0] == null)
                {
                    TabPageMain.BackgroundColor = Color.DarkOrange;
                    return;
                }

                _observableIrrigation.AliveList[0].RequestedTime = ScheduleTime.GetUnixTimeStampUtcNow();
                if (_observableIrrigation.AliveList[0].ResponseTime == 0)
                {
                    TabPageMain.BackgroundColor = Color.Crimson;
                }
                else
                {
                    var now = ScheduleTime.GetUnixTimeStampUtcNow();

                    TabPageMain.BackgroundColor = _observableIrrigation.AliveList[0].ResponseTime > (now - 100) ? Color.DeepSkyBlue : Color.Crimson;
                }
            });
        }

        private void MonitorConnectionStatus()
        {
            while (true)
            {
                try
                {
                    if (!_hasSentUpdateRequest)
                    {

                        if(!_firstRun)
                            LastOnline();

                        if (_observableIrrigation.AliveList[0] != null)
                        {
                            _observableIrrigation.AliveList[0].RequestedTime =
                                ScheduleTime.GetUnixTimeStampUtcNow();
                            if (_observableIrrigation.AliveList[0] == null ||
                                _observableIrrigation.AliveList[0].ResponseTime == 0)
                            {
                                new Authentication().SetAlive(_observableIrrigation.AliveList[0]);
                                _hasSentUpdateRequest = true;
                            }
                            else
                            {
                                var now = ScheduleTime.GetUnixTimeStampUtcNow();
                                if (_observableIrrigation.AliveList[0].ResponseTime < (now - 60))
                                {
                                    new Authentication().SetAlive(_observableIrrigation.AliveList[0]);
                                    _hasSentUpdateRequest = true;
                                }
                            }
                        }
                        else
                        {
                            new Authentication().SetAlive(_observableIrrigation.AliveList[0]);
                            _hasSentUpdateRequest = true;
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                Thread.Sleep(1000);
            }
        }

    }
}