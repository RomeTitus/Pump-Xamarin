using System;
using System.Collections.Generic;
using System.Linq;
using Pump.Droid.Database.Table;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSiteSummary : ContentView
    {
        public readonly Site _site;
        public readonly Sensor _sensor;
        public readonly List<Schedule> _schedules;
        public readonly List<CustomSchedule> _customSchedules;
        public readonly List<Equipment> _equipments;
        public readonly List<ManualSchedule> _manualSchedules;


        public ViewSiteSummary(Site site, Sensor sensor, List<Schedule> schedules, List<CustomSchedule> customSchedules, List<Equipment> equipments, List<ManualSchedule> manualSchedules)
        {
            InitializeComponent();
            _site = site;
            _sensor = sensor;
            _schedules = schedules;
            _customSchedules = customSchedules;
            _equipments = equipments;
            _manualSchedules = manualSchedules;
            AutomationId = _site.ID;
            StackLayoutSiteSummary.AutomationId = _site.ID;
            Populate();
        }
        public void Populate()
        {
            LabelSiteName.Text = _site.NAME;
            LabelSiteDescription.Text = _site.Description;

            if (_customSchedules.Any(customSchedule =>  _site.Attachments.Contains(customSchedule.id_Pump) && RunningCustomSchedule.GetCustomScheduleDetailRunning(customSchedule) != null))
                SetScheduleRunning(true);
            
            
            if(new RunningSchedule(_schedules.Where(x => _site.Attachments.Contains(x.id_Pump)), _equipments).GetRunningSchedule().ToList().Any())
                SetScheduleRunning(true);

            var manualSchedule = _manualSchedules.FirstOrDefault(x => x.ManualDetails.Any(z => _site.Attachments.Contains(z.id_Equipment)));
            if(manualSchedule != null)
                SetScheduleRunning(true);

            if (_sensor != null)
            {

                if (_sensor.TYPE == "Pressure Sensor")
                {
                    PressureSensor();
                }

            }
        }

        private void PressureSensor()
        {
            var reading = Convert.ToDouble(_sensor.LastReading);

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