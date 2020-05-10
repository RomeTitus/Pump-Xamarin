using Xamarin.Forms;

namespace Pump
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new HomeScreen();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
