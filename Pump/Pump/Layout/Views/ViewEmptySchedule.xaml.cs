﻿using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewEmptySchedule : ContentView
    {
        public ViewEmptySchedule(string text)
        {
            InitializeComponent();
            EmptyScheduleLabel.Text = text;
            Populate();
        }

        public ViewEmptySchedule(string text, double size)
        {
            InitializeComponent();
            EmptyScheduleLabel.Text = text;
            EmptyScheduleLabel.FontSize *= size;
            this.HeightRequest = 150 * size;
            Populate();
        }

        private void Populate()
        {
            ID = "-849";
            AutomationId = ID;
            
        }

        public string ID { get; private set; }
    }
}