using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Pump.Database;
using Pump.Database.Table;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeScreen : TabbedPage
    {
        private Alive alive = new Alive();
        private readonly DatabaseController _databaseController = new DatabaseController();


        public HomeScreen()
        {
            InitializeComponent();
            if (Device.RuntimePlatform == Device.iOS)
            {
                ScheduleStatusTab.IconImageSource = new FileImageSource();
                ManualStatusTab.IconImageSource = new FileImageSource();
                SettingTab.IconImageSource = new FileImageSource();
            }


            if (_databaseController.GetControllerConnectionSelection() == null)
            {
                _databaseController.SetActivityStatus(new ActivityStatus(false));
                Navigation.PushModalAsync(new AddController(true));

            }
            else
            {
                if (new DatabaseController().IsRealtimeFirebaseSelected())
                    new Thread(LastOnline).Start();
                    
                var sendToken = new Thread(() => SentNotificationToken());
                sendToken.Start();
            }
        }

        public void SentNotificationToken()
        {
            try
            {
                new SocketMessage().Message(
                    new SocketCommands().setToken(
                        _databaseController.GetNotificationToken().token));
            }
            catch
            {
            }
        }

        private void LastOnline()
        {
            var auth = new Authentication();
            /*
            alive = Task.Run(() => auth.GetLastOnRequest()).Result;
            Device.BeginInvokeOnMainThread(() =>
            {
                var now = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                if (alive == null)
                {
                    auth.SetLastOnRequest();
                    TabPageMain.BackgroundColor = Color.Crimson;
                }
                else if (alive.ResponseTime > (now - 60))
                    TabPageMain.BackgroundColor = Color.DeepSkyBlue;
                else
                {
                    auth.SetLastOnRequest();
                }
            });
            oldAlive = alive;
            */
            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Alive")
                .AsObservable<JObject>()
                .Subscribe(x =>
                {
                    if (x.Object != null)
                        alive = auth.GetJsonLastOnRequest(x.Object, alive);
                });

            MonitorConnectionStatus();
        }

        private void MonitorConnectionStatus()
        {
            var firstLoop = true;
            while (new DatabaseController().GetActivityStatus().status)
            {
                if (!new DatabaseController().IsRealtimeFirebaseSelected())
                    continue;

                try
                {
                    Device.BeginInvokeOnMainThread(() =>
                        {
                            if (alive != null && alive.ResponseTime != 0)
                            {
                                var now = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                                if (alive.ResponseTime < (now - 30))
                                    new Authentication().SetLastOnRequest();
                                if (alive.ResponseTime > (now - 45))
                                    TabPageMain.BackgroundColor = Color.DeepSkyBlue;
                                else if (alive.ResponseTime < (now - 60))
                                    TabPageMain.BackgroundColor = Color.Coral;
                                
                            }
                            else
                            {
                                new Authentication().SetLastOnRequest();
                                if (firstLoop)
                                    TabPageMain.BackgroundColor = Color.Crimson;
                            }
                                
                        });
                }
                
                catch
                {
                    
                }

                firstLoop = false;
                Thread.Sleep(5000);
            }
        }
    }
}