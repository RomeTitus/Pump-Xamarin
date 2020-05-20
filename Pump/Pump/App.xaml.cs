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
