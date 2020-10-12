using System;
using System.Threading;
using Newtonsoft.Json.Linq;
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
        private Alive _alive = new Alive();
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
                else
                    TabPageMain.BackgroundColor = Color.DeepSkyBlue;

              
            }
        }


        private void LastOnline()
        {
            var auth = new Authentication();
            auth._FirebaseClient
                .Child(auth.getConnectedPi() + "/Alive")
                .AsObservable<Alive>()
                .Subscribe(x =>
                {
                    try
                    {
                        if (x.Object == null) return;
                        _alive = x.Object;
                        var now = ScheduleTime.GetUnixTimeStampUtcNow();
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            if (_alive.ResponseTime > (now - 100))
                                TabPageMain.BackgroundColor = Color.DeepSkyBlue;
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });

            MonitorConnectionStatus();
        }

        private void MonitorConnectionStatus()
        {
            while (true)
            {
                if (!new DatabaseController().IsRealtimeFirebaseSelected())
                    continue;
                try
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                            _alive.RequestedTime = ScheduleTime.GetUnixTimeStampUtcNow();
                            if (_alive == null || _alive.ResponseTime == 0)
                            {
                                new Authentication().SetAlive(_alive);
                                TabPageMain.BackgroundColor = Color.Crimson;
                            }
                            else
                            {
                                var now = ScheduleTime.GetUnixTimeStampUtcNow();
                                if (_alive.ResponseTime < (now - 60))
                                    new Authentication().SetAlive(_alive);
                                else if (_alive.ResponseTime < (now - 120))
                                    TabPageMain.BackgroundColor = Color.Coral;
                            }
                    });
                }
                catch
                {
                 continue;   
                }
                Thread.Sleep(50000);
            }
        }
    }
}