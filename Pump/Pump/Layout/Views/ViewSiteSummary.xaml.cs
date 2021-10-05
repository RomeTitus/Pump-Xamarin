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
        public readonly Sensor Sensor;
        public readonly Site Site;
        public List<CustomSchedule> CustomSchedules;
        public List<Equipment> Equipments;
        public List<ManualSchedule> ManualSchedules;
        public List<IrrigationController.Schedule> Schedules;


        public ViewSiteSummary(Site site, Sensor sensor, List<IrrigationController.Schedule> schedules,
            List<CustomSchedule> customSchedules, List<Equipment> equipments, List<ManualSchedule> manualSchedules)
        {
            InitializeComponent();
            Site = site;
            Sensor = sensor;
            Schedules = schedules;
            CustomSchedules = customSchedules;
            Equipments = equipments;
            ManualSchedules = manualSchedules;
            AutomationId = Site.ID;
            StackLayoutSiteSummary.AutomationId = Site.ID;
            Populate();
        }

        public void Populate()
        {
            bool scheduleRunning = false;
            LabelSiteName.Text = Site.NAME;
            LabelSiteDescription.Text = Site.Description;

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

            if (Sensor != null)
            {
                if (Sensor.TYPE == "Pressure Sensor")
                {
                    PressureSensor();
                }
            }
        }

        private void PressureSensor()
        {
            var reading = Convert.ToDouble(Sensor.LastReading, CultureInfo.InvariantCulture);

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