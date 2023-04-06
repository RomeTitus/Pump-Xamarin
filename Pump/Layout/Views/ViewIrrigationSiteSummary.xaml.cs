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
    public partial class ViewIrrigationSiteSummary
    {
        private readonly ObservableFilteredIrrigation _observableFiltered;
        public ObservableFilteredIrrigation ObservableFiltered => _observableFiltered;
        public ViewIrrigationSiteSummary(ObservableIrrigation observableFiltered, KeyValuePair<string, List<string>> keyValueSites)
        {
            InitializeComponent();
            Device.BeginInvokeOnMainThread(() =>
            {
                if (Device.RuntimePlatform == Device.UWP)
                    ActivityIndicatorUwpLoadingIndicator.IsVisible = true;
                else
                    ActivityIndicatorMobileLoadingIndicator.IsVisible = true;
            });
            
            _observableFiltered = new ObservableFilteredIrrigation(observableFiltered, GetSubControllerIds(keyValueSites.Value));
            LabelSiteName.Text = keyValueSites.Key;
            SetConfigurationSummary();
        }

        private List<string> GetSubControllerIds(List<string> subControllerIds)
        {
            var updatedSubControllerIds = new List<string>();
            foreach (var controllerId in subControllerIds)
            {
                updatedSubControllerIds.Add(controllerId == "MainController" ? null : controllerId);
            }

            return updatedSubControllerIds;
        }
        
        public void SetConfigurationSummary()
        {
            if (_observableFiltered.LoadedData == false)
                return;

            Device.BeginInvokeOnMainThread(() =>
            {
                    ActivityIndicatorUwpLoadingIndicator.IsVisible = false;
                    ActivityIndicatorMobileLoadingIndicator.IsVisible = false;
            });
            

            var scheduleRunning = RunningCustomSchedule
                .GetCustomScheduleDetailRunningList(_observableFiltered.CustomScheduleList.ToList()).Any();

            if (!scheduleRunning)
                scheduleRunning = new RunningSchedule(_observableFiltered.ScheduleList.ToList(),
                    _observableFiltered.EquipmentList.ToList()).GetRunningSchedule().Any();

            if (!scheduleRunning) scheduleRunning = _observableFiltered.ManualScheduleList.Any();

            Device.BeginInvokeOnMainThread(() =>
            {
                SetScheduleRunning(scheduleRunning);

                if (_observableFiltered.SensorList.Any())
                    PressureSensor(_observableFiltered.SensorList.ToList());
            });
        }

        public TapGestureRecognizer GestureRecognizer()
        {
            return StackLayoutViewSiteTapGesture;
        }
        private void PressureSensor(List<Sensor> sensorList)
        {
            var sensor = sensorList.FirstOrDefault(x => x.TYPE == "Pressure Sensor");
            if (sensor == null)
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
    }
}