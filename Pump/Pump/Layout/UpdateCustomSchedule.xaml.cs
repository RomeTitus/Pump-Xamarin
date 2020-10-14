using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdateCustomSchedule : ContentPage
    {
        private readonly List<Equipment> _equipmentList;
        private CustomSchedule _customSchedule;
        private readonly List<string> _pumpIdList = new List<string>();
        public UpdateCustomSchedule(List<Equipment> equipmentList, CustomSchedule schedule = null)
        {
            InitializeComponent();
            _equipmentList = equipmentList;
            _customSchedule = schedule;
            PopulateEquipment();
            ButtonCreateCustomSchedule.IsEnabled = true;
            CustomPumpPicker.IsEnabled = true;
            
            if (schedule == null) return;
            ScheduleName.Text = _customSchedule.NAME;
            ButtonCreateCustomSchedule.Text = "SAVE";
        }
        
        private void PopulateEquipment()
        {
            foreach (var equipment in _equipmentList.Where(equipment => equipment.isPump))
            {
                CustomPumpPicker.Items.Add(equipment.NAME);
                _pumpIdList.Add(equipment.ID);
                if (_customSchedule.id_Pump != null && _customSchedule.id_Pump == equipment.ID)
                {
                    CustomPumpPicker.SelectedIndex = (CustomPumpPicker.Items.Count -1);
                }
            }

            try
            {
                ScrollViewZoneDetail.Children.Clear();
                if (_equipmentList.Count(equipment => equipment.isPump == false) == 0)
                    ScrollViewZoneDetail.Children.Add(new ViewEmptySchedule("No Zones Found"));
                foreach (var equipment in _equipmentList.Where(equipment => equipment.isPump == false))
                {
                    var scheduleDetail =
                        _customSchedule.ScheduleDetails.FirstOrDefault(x => x.id_Equipment == equipment.ID);
                    ScrollViewZoneDetail.Children.Add(new ViewZoneAndTimeGrid(scheduleDetail, equipment, true));
                }
            }
            catch
            {
                ScrollViewZoneDetail.Children.Clear();
                ScrollViewZoneDetail.Children.Add(new ViewException());
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
        private void ButtonCreateCustomSchedule_OnClicked(object sender, EventArgs e)
        {
            var notification = CustomScheduleValidate();
            notification = SendSelectedZonesValidate(notification);

            if (!string.IsNullOrWhiteSpace(notification))
            {
                DisplayAlert("Incomplete", notification, "Understood");
            }
            else
            {
                if (_customSchedule == null)
                    _customSchedule = new CustomSchedule();
                _customSchedule.NAME = ScheduleName.Text;
                _customSchedule.id_Pump = _pumpIdList[CustomPumpPicker.SelectedIndex];

                var scheduleDetail = GetSelectedZonesList();
                if (scheduleDetail.Count > 0)
                {
                    _customSchedule.ScheduleDetails = scheduleDetail;
                    var key = Task.Run(() => new Authentication().SetCustomSchedule(_customSchedule)).Result;
                    Navigation.PopModalAsync();
                }
                else
                    DisplayAlert("Incomplete", "\u2022 One or more zones are required!", "Understood");
            }
        }
        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}