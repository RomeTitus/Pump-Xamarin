using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Database;
using Microcharts;
using Newtonsoft.Json.Linq;
using Pump.IrrigationController;
using Pump.SocketController.Firebase;
using Rg.Plugins.Popup.Services;
using SkiaSharp;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecordScreen : ContentPage
    {
        private readonly List<ChartEntry> _chartEntries = new List<ChartEntry>();
        private readonly List<string> _excludedEntries = new List<string>();
        private readonly FloatingScreen _floatingScreen = new FloatingScreen();
        private readonly ObservableSiteIrrigation _observableIrrigation;

        public RecordScreen(ObservableSiteIrrigation observableIrrigation)
        {
            InitializeComponent();
            _observableIrrigation = observableIrrigation;
        }

        private void OnDateSelected(object sender, DateChangedEventArgs args)
        {
            Recalculate();
        }

        private void Recalculate()
        {
            BtnViewChart.IsEnabled = true;
            BtnFilterViewChart.IsVisible = false;
            var timeSpan = endDatePicker.Date - startDatePicker.Date;

            resultLabel.Text = $"{timeSpan.Days} day{(timeSpan.Days == 1 ? "" : "s")} between dates";
        }

        private void BtnBack_OnPressed(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private async void BtnViewChart_OnPressed(object sender, EventArgs e)
        {
            /*
            BtnViewChart.IsEnabled = false;
            BtnFilterViewChart.IsVisible = true;
            _excludedEntries.Clear();
            var endTime = ((DateTimeOffset)endDatePicker.Date).ToUnixTimeSeconds();
            var startTime = ((DateTimeOffset)startDatePicker.Date).ToUnixTimeSeconds();

            var loadingScreen = new PopupLoading { CloseWhenBackgroundIsClicked = false };
            await PopupNavigation.Instance.PushAsync(loadingScreen);
            var recordJObjectList = await new FirebaseManager().GetRecordingBetweenDates(startTime, endTime);
            await PopupNavigation.Instance.PopAllAsync();
            var allRecordList = CalculateHistoryRecording(recordJObjectList);
            if (!allRecordList.Any()) return;
            var recordList = allRecordList.Where(x =>
                _observableIrrigation.SiteList.First().Attachments.Contains(x.id_Equipment));

            _chartEntries.Clear();

            foreach (var record in recordList)
            {
                var random = new Random();
                var color = $"#{random.Next(0x1000000):X6}";
                var equipment = _observableIrrigation.EquipmentList.FirstOrDefault(x => x.Id == record.id_Equipment);
                if (equipment != null)
                    _chartEntries.Add(new ChartEntry(record.Duration)
                    {
                        Label = equipment.NAME,
                        ValueLabel = record.Duration.ToString(),
                        Color = SKColor.Parse(color),
                        TextColor = SKColor.Parse(color),
                        ValueLabelColor = SKColor.Parse(color)
                    });
            }

            RecordChartView.Chart = new BarChart
            {
                Entries = _chartEntries, BackgroundColor = SKColor.Parse("#00bfff"),
                LabelColor = SKColor.Parse("#FFFFFF"), LabelTextSize = 30 //, ValueLabelTextSize = 30
            };
            */
        }

        private List<Record> CalculateHistoryRecording(IReadOnlyCollection<FirebaseObject<JObject>> recordJObjectList)
        {
            var historyList = new List<Record>();
            foreach (var recordJObject in recordJObjectList)
            foreach (var recordJToken in recordJObject.Object)
            {
                var elementId = recordJToken.Key;
                var recordDetailJObject = (JObject)recordJToken.Value;
                //Skip Sensor Readings for now
                if (!recordDetailJObject.ContainsKey("Duration")) continue;
                foreach (var recordDurationJObject in recordDetailJObject)
                {
                    var record = new Record
                    {
                        id_Equipment = elementId,
                        Duration = recordDurationJObject.Value.ToObject<long>()
                    };
                    var existingRecording = historyList.FirstOrDefault(x => x.id_Equipment == elementId);
                    if (existingRecording == null)
                        historyList.Add(record);
                    else
                        existingRecording.Duration += record.Duration;
                }
            }

            return historyList;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            //Landscape
            if (width > height)
            {
                recordScreenStackLayout.Orientation = StackOrientation.Horizontal;
                RecordChartView.HeightRequest = 200;
            }
            else
            {
                recordScreenStackLayout.Orientation = StackOrientation.Vertical;
                RecordChartView.HeightRequest = 250;
            }

            base.OnSizeAllocated(width, height); //must be called
        }

        private void BtnFilterViewChart_OnPressed(object sender, EventArgs e)
        {
            var chartFilter = new ChartEntryFilterScreen(_chartEntries, _excludedEntries);
            var chartLayout = new List<object> { chartFilter.GetLayout() };
            _floatingScreen.SetFloatingScreen(chartLayout);
            foreach (var checkBox in chartFilter.GetCheckBoxes()) checkBox.CheckedChanged += CheckBox_CheckedChanged;

            PopupNavigation.Instance.PushAsync(_floatingScreen);
        }

        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            if (checkBox.IsChecked)

                _excludedEntries.Remove(checkBox.AutomationId);
            else
                _excludedEntries.Add(checkBox.AutomationId);
            RecordChartView.Chart = new BarChart
            {
                Entries = _chartEntries.Where(x => _excludedEntries.Contains(x.Label) == false),
                BackgroundColor = SKColor.Parse("#00bfff"), LabelColor = SKColor.Parse("#FFFFFF"), LabelTextSize = 30
                //ValueLabelTextSize = 30
            };
        }
    }
}