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
    public partial class ViewIrrigationConfigurationSummary
    {
        private readonly KeyValuePair<IrrigationConfiguration, ObservableIrrigation> _keyValueIrrigation;
        public ViewIrrigationConfigurationSummary(KeyValuePair<IrrigationConfiguration, ObservableIrrigation> keyValueIrrigation)
        {
            InitializeComponent();
            if (Device.RuntimePlatform == Device.UWP)
                ActivityIndicatorUwpLoadingIndicator.IsVisible = true;
            else
                ActivityIndicatorMobileLoadingIndicator.IsVisible = true;
            
            _keyValueIrrigation = keyValueIrrigation;
            StackLayoutSiteSummary.AutomationId = keyValueIrrigation.Key.Id.ToString();
            LabelSiteName.Text = keyValueIrrigation.Key.Path;
        }
        
        public void Populate()
        {
            if(_keyValueIrrigation.Value.LoadedData == false)
                return;
            
            
            var scheduleRunning = RunningCustomSchedule.GetCustomScheduleDetailRunningList(_keyValueIrrigation.Value.CustomScheduleList.ToList()).Any();

            if (!scheduleRunning)
            {
                scheduleRunning = new RunningSchedule(_keyValueIrrigation.Value.ScheduleList.ToList(),
                        _keyValueIrrigation.Value.EquipmentList.ToList()).GetRunningSchedule().Any();
            }

            if (!scheduleRunning)
            {
                scheduleRunning = _keyValueIrrigation.Value.ManualScheduleList.Any();
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                if (Device.RuntimePlatform == Device.UWP)
                    ActivityIndicatorUwpLoadingIndicator.IsVisible = false;
                else
                    ActivityIndicatorMobileLoadingIndicator.IsVisible = false;
                
                SetScheduleRunning(scheduleRunning);

                if (_keyValueIrrigation.Value.SensorList.Any())
                    PressureSensor(_keyValueIrrigation.Value.SensorList.ToList());

            });
        }

        private void PressureSensor(List<Sensor> sensorList)
        {
            var sensor = sensorList.FirstOrDefault(x => x.TYPE == "Pressure Sensor");
            if(sensor == null)
                return;
            
            LabelPressure.IsVisible = true;
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