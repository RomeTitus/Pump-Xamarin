﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pump.Database;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeScreen : TabbedPage
    {
        DatabaseController databaseController = new DatabaseController();
        
        
        public HomeScreen()
        {
            InitializeComponent();

            if (databaseController.GetPumpSelection() == null)
            {
                Navigation.PushModalAsync(new AddController());
               // Navigation.PopModalAsync();
            }
        }
  
    }
}