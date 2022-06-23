using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Pump.Database.Table;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewIrrigationConfigurationSummary : ContentView
    {
        public readonly IrrigationConfiguration IrrigationConfiguration;
        public List<CustomSchedule> CustomSchedules;
        public List<Equipment> Equipments;
        public List<ManualSchedule> ManualSchedules;
        public List<IrrigationController.Schedule> Schedules;
        public Sensor sensor;


        public ViewIrrigationConfigurationSummary(IrrigationConfiguration irrigationConfiguration)
        {
            InitializeComponent();
            if (Device.RuntimePlatform == Device.UWP)
                ActivityIndicatorUWPLoadingIndicator.IsVisible = true;
            else
                ActivityIndicatorMobileLoadingIndicator.IsVisible = true;
            
            IrrigationConfiguration = irrigationConfiguration;
            AutomationId = irrigationConfiguration.Id.ToString();
            StackLayoutSiteSummary.AutomationId = irrigationConfiguration.Id.ToString();
            LabelSiteName.Text = "TEST_SITE_NAME"; //irrigationConfiguration.Path;
        }

        /*
        public void Populate()
        {
            var scheduleRunning = false;

            if (Device.RuntimePlatform == Device.UWP)
                ActivityIndicatorUWPLoadingIndicator.IsVisible = false;
            else
                ActivityIndicatorMobileLoadingIndicator.IsVisible = false;

            LabelPressure.IsVisible = true;
            if (CustomSchedules.Any(customSchedule => IrrigationConfiguration.Attachments.Contains(customSchedule.id_Pump) &&
                                                      RunningCustomSchedule.GetCustomScheduleDetailRunning(
                                                          customSchedule) != null))
                scheduleRunning = true;
            if (new RunningSchedule(Schedules.Where(x => IrrigationConfiguration.Attachments.Contains(x.id_Pump)), Equipments)
                .GetRunningSchedule().ToList().Any())
                scheduleRunning = true;
            var manualSchedule =
                ManualSchedules.FirstOrDefault(x =>
                    x.ManualDetails.Any(z => IrrigationConfiguration.Attachments.Contains(z.id_Equipment)));
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

*/
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