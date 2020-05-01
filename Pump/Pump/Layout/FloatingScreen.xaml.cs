﻿using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FloatingScreen : PopupPage
    {
        public FloatingScreen(List<Object> screens)
        {
            InitializeComponent();

            foreach (View screen in screens){
                ScrollViewFloatingPage.Children.Add(screen);
            }
            
        }
    }
}