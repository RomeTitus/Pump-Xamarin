﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingPage : ContentPage
    {
        public SettingPage()
        {
            InitializeComponent();
        }

        private void BtnConnectionDetail_OnPressed(object sender, EventArgs e)
        {
            Navigation.PopAsync();
            Navigation.PushModalAsync(new ConnectionScreen());
        }

        private void BtnScheduleDetail_OnPressed(object sender, EventArgs e)
        {
            //Navigation.PopAsync();
            Navigation.PushModalAsync(new ViewScheduleScreen());
        }

        private void BtnGraphSummary_OnPressed(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new ViewGraphSummaryScreen());
        }
    }
}