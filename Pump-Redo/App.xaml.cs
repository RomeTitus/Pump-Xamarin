using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Pump.Layout;

namespace Pump_Redo
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AuthenticationScreen();
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
