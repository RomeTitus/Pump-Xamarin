using System;
using System.Collections.Generic;
using System.Linq;
using Pump.Database.Table;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Schedule
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomScheduleUpdate : ContentPage
    {
        private readonly KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation>
            _observableFilterKeyValuePair;

        private readonly List<string> _pumpIdList = new List<string>();
        private readonly SocketPicker _socketPicker;
        private CustomSchedule _customSchedule;

        public CustomScheduleUpdate(
            KeyValuePair<IrrigationConfiguration, ObservableFilteredIrrigation> observableFilterKeyValuePair,
            SocketPicker socketPicker,
            CustomSchedule schedule = null)
        {
            InitializeComponent();
            _socketPicker = socketPicker;
            _observableFilterKeyValuePair = observableFilterKeyValuePair;
            if (schedule != null)
            {
                ScheduleName.Text = schedule.NAME;
                ButtonCreateCustomSchedule.Text = "SAVE";
            }
            else
            {
                schedule = new CustomSchedule();
            }

            _customSchedule = schedule;
            MaskedEntryRepeat.Text = _customSchedule.Repeat.ToString();
            PopulateEquipment();
            ButtonCreateCustomSchedule.IsEnabled = true;
            CustomPumpPicker.IsEnabled = true;
        }

        private void PopulateEquipment()
        {
            foreach (var equipment in _observableFilterKeyValuePair.Value.EquipmentList
                         .Where(equipment => equipment.isPump).OrderBy(c => c.NAME.Length)
                         .ThenBy(c => c.NAME))
            {
                CustomPumpPicker.Items.Add(equipment.NAME);
                _pumpIdList.Add(equipment.Id);
                if (_customSchedule.id_Pump != null && _customSchedule.id_Pump == equipment.Id)
                    CustomPumpPicker.SelectedIndex = CustomPumpPicker.Items.Count - 1;

                if (CustomPumpPicker.SelectedIndex == -1 && CustomPumpPicker.Items.Count > 0)
                    CustomPumpPicker.SelectedIndex = 0;
            }

            try
            {
                ScrollViewZoneDetail.Children.Clear();
                if (_observableFilterKeyValuePair.Value.EquipmentList.Count(equipment => equipment.isPump == false) ==
                    0)
                    ScrollViewZoneDetail.Children.Add(new ViewEmptySchedule("No Zones Found"));
                foreach (var equipment in _observableFilterKeyValuePair.Value.EquipmentList
                             .Where(equipment => equipment.isPump == false)
                             .OrderBy(c => c.NAME.Length).ThenBy(c => c.NAME))
                {
                    var scheduleDetail =
                        _customSchedule.ScheduleDetails.FirstOrDefault(x => x.id_Equipment == equipment.Id);
                    ScrollViewZoneDetail.Children.Add(new ViewZoneAndTimeGrid(scheduleDetail, equipment, true));
                }
            }
            catch (Exception e)
            {
                ScrollViewZoneDetail.Children.Clear();
                ScrollViewZoneDetail.Children.Add(new ViewException(e));
            }
        }

        private string CustomScheduleValidate()
        {
            var notification = "";

            if (string.IsNullOrWhiteSpace(ScheduleName.Text))
            {
                if (notification.Length < 1)
                    notification = "\u2022 Schedule name required";
                else
                    notification += "\n\u2022 Schedule name required";
                ScheduleName.PlaceholderColor = Color.Red;
                ScheduleName.Placeholder = "Schedule name";
            }

            if (CustomPumpPicker.SelectedIndex == -1)
            {
                if (notification.Length < 1)
                    notification = "\u2022 Select a pump";
                else
                    notification += "\n\u2022 Select a pump";
                CustomPumpPicker.BackgroundColor = Color.Red;
            }

            if (MaskedEntryRepeat.Text.Length == 0)
            {
                if (notification.Length < 1)
                    notification = "\u2022 Select repeat amount";
                else
                    notification += "\n\u2022 Select repeat amount";
                CustomPumpPicker.BackgroundColor = Color.Red;
            }

            return notification;
        }

        private string SendSelectedZonesValidate(string notification)
        {
            foreach (var scrollViewZone in ScrollViewZoneDetail.Children)
            {
                var child = (ViewZoneAndTimeGrid)scrollViewZone;
                var maskTime = child.GetMaskText();
                if (string.IsNullOrWhiteSpace(maskTime.Text) || maskTime.Text.Length >= 4) continue;
                if (string.IsNullOrWhiteSpace(notification))
                    notification = "\u2022 " + child.GetZoneNameText().Text + " time format is incorrect";
                else
                    notification += "\n\u2022 " + child.GetZoneNameText().Text + " time format is incorrect";
                child.GetZoneNameText().TextColor = Color.Red;
            }

            return notification;
        }

        private List<ScheduleDetail> GetSelectedZonesList()
        {
            var scheduleDetailList = (from ViewZoneAndTimeGrid child in ScrollViewZoneDetail.Children
                select child.GetMaskText()
                into maskTime
                where !string.IsNullOrWhiteSpace(maskTime.Text)
                select
                    new ScheduleDetail { id_Equipment = maskTime.AutomationId, DURATION = maskTime.Text }).ToList();

            return scheduleDetailList;
        }

        private async void ButtonCreateCustomSchedule_OnClicked(object sender, EventArgs e)
        {
            var notification = CustomScheduleValidate();
            notification = SendSelectedZonesValidate(notification);

            if (!string.IsNullOrWhiteSpace(notification))
            {
                await DisplayAlert("Incomplete", notification, "Understood");
            }
            else
            {
                if (_customSchedule == null)
                    _customSchedule = new CustomSchedule();
                _customSchedule.NAME = ScheduleName.Text;
                _customSchedule.id_Pump = _pumpIdList[CustomPumpPicker.SelectedIndex];
                long.TryParse(MaskedEntryRepeat.Text, out var repeat);
                _customSchedule.Repeat = repeat;
                var scheduleDetail = GetSelectedZonesList();
                if (scheduleDetail.Count > 0)
                {
                    _customSchedule.ScheduleDetails = scheduleDetail;
                    await _socketPicker.SendCommand(_customSchedule, _observableFilterKeyValuePair.Key);
                    await Navigation.PopModalAsync();
                }
                else
                {
                    await DisplayAlert("Incomplete", "\u2022 One or more zones are required!", "Understood");
                }
            }
        }

        private async void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }
    }
}