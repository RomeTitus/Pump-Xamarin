using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSiteSummary : ContentView
    {
        public readonly Site Site;
        public List<CustomSchedule> CustomSchedules;
        public List<Equipment> Equipments;
        public List<ManualSchedule> ManualSchedules;
        public List<IrrigationController.Schedule> Schedules;
        public Sensor sensor;


        public ViewSiteSummary(Site site)
        {
            InitializeComponent();
            if (Device.RuntimePlatform == Device.UWP)
                ActivityIndicatorUWPLoadingIndicator.IsVisible = true;
            else
                ActivityIndicatorMobileLoadingIndicator.IsVisible = true;
            Site = site;
            AutomationId = Site.ID;
            StackLayoutSiteSummary.AutomationId = Site.ID;
            LabelSiteName.Text = Site.NAME;
            LabelSiteDescription.Text = Site.Description;
        }

        public void Populate()
        {
            var scheduleRunning = false;

            if (Device.RuntimePlatform == Device.UWP)
                ActivityIndicatorUWPLoadingIndicator.IsVisible = false;
            else
                ActivityIndicatorMobileLoadingIndicator.IsVisible = false;

            LabelPressure.IsVisible = true;
            if (CustomSchedules.Any(customSchedule => Site.Attachments.Contains(customSchedule.id_Pump) &&
                                                      RunningCustomSchedule.GetCustomScheduleDetailRunning(
                                                          customSchedule) != null))
                scheduleRunning = true;
            if (new RunningSchedule(Schedules.Where(x => Site.Attachments.Contains(x.id_Pump)), Equipments)
                .GetRunningSchedule().ToList().Any())
                scheduleRunning = true;
            var manualSchedule =
                ManualSchedules.FirstOrDefault(x =>
                    x.ManualDetails.Any(z => Site.Attachments.Contains(z.id_Equipment)));
            if (manualSchedule != null)
                scheduleRunning = true;

            SetScheduleRunning(scheduleRunning);

            if (sensor != null)
                if (sensor.TYPE == "Pressure Sensor")
                    PressureSensor();
        }

        private void PressureSensor()
        {
            var reading = Convert.ToDouble(sensor.LastReading, CultureInfo.InvariantCulture);

            var voltage = reading * 5.0 / 1024.0;

            var pressurePascal = 3.0 * (voltage - 0.47) * 1000000.0;

            var bars = pressurePascal / 10e5;
            LabelPressure.Text = bars.ToString("0.##") + " Bar";
        }

        private void SetScheduleRunning(bool running)
        {
            FrameScheduleStatus.BackgroundColor = running ? Color.LawnGreen : Color.White;
        }

        public void SetBackgroundColor(Color color)
        {
            FrameSiteSummary.BackgroundColor = color;
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return StackLayoutViewSiteTapGesture;
        }
    }
}