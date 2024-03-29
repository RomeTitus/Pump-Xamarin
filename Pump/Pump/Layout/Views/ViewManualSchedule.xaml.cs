﻿using System;
using Pump.Class;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewManualSchedule : ContentView
    {
        private readonly ManualSchedule _manualSchedule;
        private static System.Timers.Timer _timer;
        public ViewManualSchedule(ManualSchedule manual)
        {
            InitializeComponent();
            _manualSchedule = manual;
            AutomationId = _manualSchedule.ID;
            SetManualText();
        }

        private void SetManualText()
        {
            LableManual.Text = _manualSchedule.RunWithSchedule ? "Manual Running with Schedule" : "Manual Running without Schedule";

            timer_Elapsed(null, null);
            StartEvent();
            //LableManualTime.Text = "Duration: " + ScheduleTime.ConvertTimeSpanToString(ScheduleTime.FromUnixTimeStampUtc(_manualSchedule.EndTime) - DateTime.UtcNow);
        }
        
        private void StartEvent()
        {
            _timer = new System.Timers.Timer(1000); // 1 seconds
            _timer.Elapsed += timer_Elapsed;
            _timer.Enabled = true;
        }
        
        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            
            string duration;
            
                Device.BeginInvokeOnMainThread(() =>
                {
                    var span = ScheduleTime.FromUnixTimeStampUtc(_manualSchedule.EndTime) - DateTime.UtcNow;
                    duration = $"Time left: {span:hh\\:mm\\:ss}";
                    LableManualTime.Text = duration;
                });

        }
    }
}