using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Pump.Database;
using Pump.Database.Table;
using Xamarin.Forms;

namespace Pump
{
    public partial class App : Application
    {
        DatabaseController databaseController = new DatabaseController();
        public App()
        {
            InitializeComponent();

            MainPage = new HomeScreen();
        }

        protected override void OnStart()
        {
            databaseController.SetActivityStatus(new ActivityStatus(true));
            AppCenter.Start("android=d17bbea9-cef0-492c-b36d-f122eb46c43d;" +
                            "uwp=68836fdd-0dbe-411d-b1e8-89288071b168;" +
                            "ios=989d9753-7678-418a-bc0f-870c22fdb6e5",
                typeof(Analytics), typeof(Crashes));

        }

        protected override void OnSleep()
        {
            databaseController.SetActivityStatus(new ActivityStatus(false));
        }

        protected override void OnResume()
        {
            databaseController.SetActivityStatus(new ActivityStatus(true));
        }
    }
}
