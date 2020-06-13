﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewZoneAndTimeGrid : ContentView
    {
        public ViewZoneAndTimeGrid(IReadOnlyList<string> zoneAndTimeList, bool isTimeSet)
        {
            InitializeComponent();

            LabelZoneTime.AutomationId = zoneAndTimeList[0];
            LabelZoneName.Text = zoneAndTimeList[1];
            if (isTimeSet)
                LabelZoneTime.Text = zoneAndTimeList[2];
            
        }

    }
}