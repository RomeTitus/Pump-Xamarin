﻿using Pump.Layout;
using Xamarin.Forms;

namespace Pump
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Device.SetFlags(new[]
            {
                "CarouselView_Experimental",
                "IndicatorView_Experimental"
            });
            MainPage = new SiteScreen();
        }


        protected override void OnStart()
        {
            //_databaseController.SetActivityStatus(new ActivityStatus(true));
        }

        protected override void OnSleep()
        {
            // _databaseController.SetActivityStatus(new ActivityStatus(false));
        }

        protected override void OnResume()
        {
            //_databaseController.SetActivityStatus(new ActivityStatus(true));
        }
    }
}