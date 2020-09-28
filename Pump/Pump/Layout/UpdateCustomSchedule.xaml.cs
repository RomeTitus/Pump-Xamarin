using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pump.Database;
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
        private readonly string _id;
        private readonly List<Equipment> _equipmentList = new List<Equipment>();
        private readonly List<CustomSchedule> _customSchedulesList = new List<CustomSchedule>();
        private readonly List<string> _pumpIdList = new List<string>();
        private readonly List<string> _zoneDetailList = new List<string>();


        public UpdateCustomSchedule(List<Equipment> equipmentList)
        {
            InitializeComponent();
            _equipmentList = equipmentList;
            PopulateEquipment();
            ButtonCreateCustomSchedule.IsEnabled = true;
            CustomPumpPicker.IsEnabled = true;
           
        }

        private void PopulateEquipment()
        {
            foreach (var equipment in _equipmentList.Where(equipment => equipment.isPump))
            {
                CustomPumpPicker.Items.Add(equipment.NAME);
                _pumpIdList.Add(equipment.ID);
            }
            

            if (CustomPumpPicker.Items.Count > 0)
                CustomPumpPicker.SelectedIndex = 0;

            ScrollViewZoneDetail.Children.Clear();

            var zone = "";
            foreach (var equipment in _equipmentList.Where(equipment => equipment.isPump == false))
            {
                zone += equipment.ID + ',' + equipment.NAME + ',' + equipment.GPIO;
                if (equipment.AttachedPiController == null)
                    zone += ",0#";
                else
                    zone += "," + equipment.AttachedPiController + "#";
            }

            var zoneDetailObject = _zoneDetailList.Count < 1 ? getZoneDetailObject(zone) : getZoneDetailObject(zone, _zoneDetailList);
            foreach (View view in zoneDetailObject) ScrollViewZoneDetail.Children.Add(view);
        }


        private List<object> getZoneDetailObject(string zone)
        {
            var zoneListObject = new List<object>();
            try
            {
                if (zone == "No Data" || zone == "")
                {
                    zoneListObject.Add(new ViewEmptySchedule("No Zones Found"));
                    return zoneListObject;
                }


                var zoneList = zone.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                zoneListObject.AddRange(zoneList.Select(schedule =>
                    new ViewZoneAndTimeGrid(schedule.Split(',').ToList(), false)));
                return zoneListObject;
            }
            catch
            {
                zoneListObject = new List<object> { new ViewNoConnection() };
                return zoneListObject;
            }
        }

        private List<object> getZoneDetailObject(string zones, IReadOnlyList<string> zoneDetailList)
        {
            var zoneListObject = new List<object>();
            try
            {
                if (zones == "No Data" || zones == "")
                {
                    zoneListObject.Add(new ViewEmptySchedule("No Zones Found"));
                    return zoneListObject;
                }


                var zonesList = zones.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                foreach (var zone in zonesList)
                {
                    var hasProperty = false;
                    var zoneList = zone.Split(',').ToList();

                    foreach (var zoneDetail in zoneDetailList)
                    {
                        var existingZone = zoneDetail.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                        if (existingZone[0] == zoneList[0])
                        {
                            zoneList[2] = existingZone[2];
                            zoneListObject.Add(new ViewZoneAndTimeGrid(zoneList, true));
                            hasProperty = true;
                        }
                    }

                    if (!hasProperty)
                        zoneListObject.Add(new ViewZoneAndTimeGrid(zoneList, false));
                }

                return zoneListObject;
            }
            catch
            {
                zoneListObject = new List<object> { new ViewNoConnection() };
                return zoneListObject;
            }
        }

        private void ButtonCreateCustomSchedule_OnClicked(object sender, EventArgs e)
        {
            SendCustomSchedule();
        }

        private void SendCustomSchedule()
        {
            var notification = CustomScheduleValidate();
            notification = SendSelectedZonesValidate(notification);

            if (!string.IsNullOrWhiteSpace(notification))
            {
                DisplayAlert("Incomplete", notification, "Understood");
            }
            else
            {
                if (new DatabaseController().IsRealtimeFirebaseSelected())
                {
                    var schedule = new CustomSchedule
                    {
                        ID = _id,
                        NAME = ScheduleName.Text,
                        id_Pump = _pumpIdList[CustomPumpPicker.SelectedIndex]
                    };
                    var scheduleDetail = GetSelectedZonesList();
                    if (scheduleDetail.Count > 0)
                    {
                        schedule.ScheduleDetails = scheduleDetail;
                        var key = Task.Run(() => new Authentication().SetCustomSchedule(schedule)).Result;
                        Navigation.PopModalAsync();
                    }
                    else
                        DisplayAlert("Incomplete", "\u2022 One or more zones are required!", "Understood");
                }
                /*
                else
                {
                    var schedule = ScheduleName.Text;
                    schedule += "#" + MaskedEntryTime.Text;
                    schedule += "#" + _pumpIdList[PumpPicker.SelectedIndex] + "#";
                    schedule = _weekdayList.Aggregate(schedule, (current, week) => current + "," + week);
                    var zoneSchedule = GetSelectedZones();
                    if (string.IsNullOrWhiteSpace(zoneSchedule))
                    {
                        GetViewSchedulePumpTime();
                    }
                    else
                    {
                        schedule += "#" + zoneSchedule;
                        new Thread(() => SendScheduleSocket(schedule)).Start();
                        Navigation.PopModalAsync();
                        Navigation.PopModalAsync();
                        Navigation.PushModalAsync(new ViewScheduleHomeScreen());
                    }
                }
                */
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
                var maskTime = child.getMaskText();
                if (string.IsNullOrWhiteSpace(maskTime.Text) || maskTime.Text.Length >= 4) continue;
                if (string.IsNullOrWhiteSpace(notification))
                    notification = "\u2022 " + child.getZoneNameText().Text + " time format is incorrect";
                else
                    notification += "\n\u2022 " + child.getZoneNameText().Text + " time format is incorrect";
                child.getZoneNameText().TextColor = Color.Red;
            }

            return notification;
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private List<ScheduleDetail> GetSelectedZonesList()
        {
            var scheduleDetailList = (from ViewZoneAndTimeGrid child in ScrollViewZoneDetail.Children select child.getMaskText() into maskTime where !string.IsNullOrWhiteSpace(maskTime.Text) select new ScheduleDetail { id_Equipment = maskTime.AutomationId, DURATION = maskTime.Text }).ToList();

            return scheduleDetailList;
        }
    }
}