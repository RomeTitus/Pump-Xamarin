﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeScreen : TabbedPage
    {
        public HomeScreen()
        {
            InitializeComponent();
            this.Title = "Ping";
            //ScheduleStatusTab.IconImageSource = "Icons/ic_action_pump.png";
        }


       
    }
}