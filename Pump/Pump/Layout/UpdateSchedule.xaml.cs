using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdateSchedule : ContentPage
    {
        private readonly List<Equipment> _equipmentList;
        private Schedule _schedule;
        private readonly List<string> _pumpIdList = new List<string>();
        private ViewSchedulePumpTime _pumpSelectedTime;

        public UpdateSchedule(List<Equipment> equipmentList, Schedule schedule = null)
        {
            InitializeComponent();
            _equipmentList = equipmentList;
            if (schedule == null)
                schedule = new Schedule();
            _schedule = schedule;

            //SetUpWeekDays(); //Not sure where this should go
            new Thread(SetUpWeekDays).Start();

            PopulateEquipment();
            ButtonCreateSchedule.IsEnabled = true;
            PumpPicker.IsEnabled = true;
            
            if (schedule != null)
            {
                ButtonCreateSchedule.Text = "SAVE";
                ScheduleName.Text = _schedule.NAME;
                MaskedEntryTime.Text = _schedule.TIME;
                var selectWeekThread = new Thread(SetSelectedWeek);
                selectWeekThread.Start();
            }

        }

        private void PopulateEquipment()
        {
            foreach (var equipment in _equipmentList.Where(equipment => equipment.isPump))
            {
                PumpPicker.Items.Add(equipment.NAME);
                _pumpIdList.Add(equipment.ID);
                if (_schedule.id_Pump != null && _schedule.id_Pump == equipment.ID)
                {
                    PumpPicker.SelectedIndex = (PumpPicker.Items.Count - 1);
                }
            }
            if(PumpPicker.SelectedIndex == -1 && PumpPicker.Items.Count>0)
                PumpPicker.SelectedIndex = 0;
            try
            {
                ScrollViewZoneDetail.Children.Clear();
                if (_equipmentList.Count(equipment => equipment.isPump == false) == 0)
                    ScrollViewZoneDetail.Children.Add(new ViewEmptySchedule("No Zones Found"));
                foreach (var equipment in _equipmentList.Where(equipment => equipment.isPump == false))
                {
                    var scheduleDetail =
                        _schedule.ScheduleDetails.FirstOrDefault(x => x.id_Equipment == equipment.ID);
                    ScrollViewZoneDetail.Children.Add(new ViewZoneAndTimeGrid(scheduleDetail, equipment, true));
                }
            }
            catch
            {
                ScrollViewZoneDetail.Children.Clear();
                ScrollViewZoneDetail.Children.Add(new ViewException());
            }
        }

        private void SetUpWeekDays()
        {
            var labels = new List<Label>
            {
                LabelSunday,
                LabelMonday,
                LabelTuesday,
                LabelWednesday,
                LabelThursday,
                LabelFriday,
                LabelSaturday
            };

            foreach (var label in labels)
            {
                label.TextColor = Color.Gray;
                label.BackgroundColor = Color.DeepSkyBlue;
            }

            var frames = new List<Frame>
            {
                FrameSunday,
                FrameMonday,
                FrameTuesday,
                FrameWednesday,
                FrameThursday,
                FrameFriday,
                FrameSaturday
            };

            try
            {
                FramesLoaded(frames);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            foreach (var frame in frames)
            {
                frame.BorderColor = Color.Gray;
                frame.BackgroundColor = Color.DeepSkyBlue;
            }
        }
        
        private static void FramesLoaded(IReadOnlyCollection<Frame> frames)
        {
            for (var i = 0; i < 100; i++)
            {
                foreach (var frame in frames)
                    if (frame.Height > -1 || frame.Width > -1)
                        return;
                Thread.Sleep(10);
            }
        }

        private void Frame_OnTap(object sender, EventArgs e)
        {
            var frame = (Frame)sender;
            var weekday = frame.AutomationId;

            if (_schedule.WEEK.Split(',').Contains(weekday)) 
                _schedule.WEEK = _schedule.WEEK.Replace(weekday+",","");
            else
                _schedule.WEEK += (weekday + ",");

            SetSelectedWeek();
        }
        private void SetSelectedWeek()
        {
            var frames = new List<Frame>
            {
                FrameSunday,
                FrameMonday,
                FrameTuesday,
                FrameWednesday,
                FrameThursday,
                FrameFriday,
                FrameSaturday
            };

            if (_schedule == null)
                return;
            foreach (var frame in frames)
            {
                ChangeWeekSelect(frame, _schedule.WEEK.Split(',').Contains(frame.AutomationId));
            }
        }
        private static void ChangeWeekSelect(Frame frame, bool isSelected)
        {
            var frameChildren = frame.Children;
            foreach (var element in frameChildren)
            {
                var child = (StackLayout)element;
                foreach (var view in child.Children)
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        var label = (Label)view;
                        frame.BackgroundColor = isSelected ? Color.White : Color.DeepSkyBlue;
                        frame.BorderColor = isSelected ? Color.Black : Color.Gray;
                        label.BackgroundColor = isSelected ? Color.White : Color.DeepSkyBlue;
                        label.TextColor = isSelected ? Color.Black : Color.Gray;
                    });
            }
        }

        private void ButtonUpdateBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void ButtonUpdateSchedule_OnClicked(object sender, EventArgs e)
        {


            var notification = SendUpdateScheduleValidate();
            notification = SendSelectedZonesValidate(notification);

            if (!string.IsNullOrWhiteSpace(notification))
            {
                DisplayAlert("Incomplete", notification, "Understood");
            }
            else
            {
                if (_schedule == null)
                    _schedule = new Schedule();
                _schedule.NAME = ScheduleName.Text;
                _schedule.id_Pump = _pumpIdList[PumpPicker.SelectedIndex];
                _schedule.TIME = MaskedEntryTime.Text;

                var scheduleDetail = GetSelectedZonesList();
                if (scheduleDetail.Count > 0)
                {
                    _schedule.ScheduleDetails = scheduleDetail;
                    var key = Task.Run(() => new Authentication().SetSchedule(_schedule)).Result;
                    Navigation.PopModalAsync();
                }
                else
                {
                    var floatingScreen = new FloatingScreen();
                    PopupNavigation.Instance.PushAsync(floatingScreen);
                    _pumpSelectedTime = new ViewSchedulePumpTime(_schedule, _equipmentList.First(x => x.ID == _schedule.id_Pump));
                    _pumpSelectedTime.GetPumpDurationButton().Pressed += UpdateSchedulePumpDuration_Pressed;
                    floatingScreen.SetFloatingScreen(new List<object> { _pumpSelectedTime });
                }
            }
        }


        private string SendUpdateScheduleValidate()
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

            if (string.IsNullOrWhiteSpace(MaskedEntryTime.Text) || MaskedEntryTime.Text.Length < 5)
            {
                if (notification.Length < 1)
                    notification = "\u2022 Schedule start time required";
                else
                    notification += "\n\u2022 Schedule start time required";
                MaskedEntryTime.PlaceholderColor = Color.Red;
            }

            if (PumpPicker.SelectedIndex == -1)
            {
                if (notification.Length < 1)
                    notification = "\u2022 Select a pump";
                else
                    notification += "\n\u2022 Select a pump";
                PumpPicker.BackgroundColor = Color.Red;
            }

            if (_schedule.WEEK.Split(',').ToList().Count == 0)
            {
                var frames = new List<Frame>
                {
                    FrameSunday,
                    FrameMonday,
                    FrameTuesday,
                    FrameWednesday,
                    FrameThursday,
                    FrameFriday,
                    FrameSaturday
                };

                foreach (var frame in frames) frame.BorderColor = Color.Red;

                if (notification.Length < 1)
                    notification = "\u2022 Select a weekday";
                else
                    notification += "\n\u2022 Select a weekday";
            }

            return notification;
        }

        private string SendSelectedZonesValidate(string notification)
        {
            foreach (var scrollViewZone in ScrollViewZoneDetail.Children)
            {
                var child = (ViewZoneAndTimeGrid) scrollViewZone;
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
            return (from ViewZoneAndTimeGrid child in ScrollViewZoneDetail.Children select child.GetMaskText() into maskTime where !string.IsNullOrWhiteSpace(maskTime.Text) select new ScheduleDetail {id_Equipment = maskTime.AutomationId, DURATION = maskTime.Text}).ToList();
        }

        private void UpdateSchedulePumpDuration_Pressed(object sender, EventArgs e)
        {
            var notification = "";
            notification += SendSelectedPumpValidate(notification, PumpPicker.Items[PumpPicker.SelectedIndex]);
            if (!string.IsNullOrWhiteSpace(notification))
            {
                DisplayAlert("Incomplete", notification, "Understood");
            }
            else
            {
                PopupNavigation.Instance.PopAsync();
                _schedule.ScheduleDetails = new List<ScheduleDetail>
                {
                    new ScheduleDetail()
                    {
                        id_Equipment = _pumpIdList[PumpPicker.SelectedIndex],
                        DURATION = _pumpSelectedTime.getPumpDurationTime().Text
                    }
                };
                var key = Task.Run(() => new Authentication().SetSchedule(_schedule)).Result;
            }

            Navigation.PopModalAsync();
            Navigation.PopModalAsync();
        }
        

        private string SendSelectedPumpValidate(string notification, string pumpName)
        {
            if (!string.IsNullOrWhiteSpace(_pumpSelectedTime.getPumpDurationTime().Text) &&
                _pumpSelectedTime.getPumpDurationTime().Text.Length >= 4) return notification;
            if (string.IsNullOrWhiteSpace(notification))
                notification = "\u2022 " + pumpName + " time format is incorrect";
            else
                notification += "\n\u2022 " + pumpName + " time format is incorrect";

            _pumpSelectedTime.getPumpDurationTime().TextColor = Color.Red;
            _pumpSelectedTime.getPumpDurationTime().PlaceholderColor = Color.Red;

            return notification;
        }
    }
}