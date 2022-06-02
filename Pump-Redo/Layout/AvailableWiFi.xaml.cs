﻿using System;
using System.Collections.Generic;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AvailableWiFi : ContentPage
    {
        private readonly List<WiFiContainer> _wiFiContainers;

        public AvailableWiFi(List<WiFiContainer> wiFiContainers)
        {
            InitializeComponent();
            _wiFiContainers = wiFiContainers;
            Populate();
        }

        private void Populate()
        {
            foreach (var wiFi in _wiFiContainers) ScrollViewWiFiDetail.Children.Add(new ViewWiFi(wiFi));
        }

        private void ButtonCancel_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        public IList<View> GetChildren()
        {
            return ScrollViewWiFiDetail.Children;
        }
    }
}