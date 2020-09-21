using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pump.Database;
using Pump.FirebaseDatabase;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Pump.SocketController;
using Rg.Plugins.Popup.Services;
using SQLitePCL;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdateSchedule : ContentPage
    {
        private readonly List<Equipment> _equipmentList = new List<Equipment>();
        private readonly SocketCommands _command = new SocketCommands();
        private readonly List<string> _pumpIdList = new List<string>();
        private readonly List<string> _zoneDetailList = new List<string>();
        private readonly SocketMessage _socket = new SocketMessage();
        private readonly string _id;

        private ViewSchedulePumpTime _pumpSelectedTime;
        private List<string> _weekdayList = new List<string>();

        public UpdateSchedule()
        {
            InitializeComponent();
            new Thread(ThreadController).Start();
        }

        

        public UpdateSchedule(IReadOnlyList<string> scheduleDetailList)
        {
            InitializeComponent();
            ButtonCreateSchedule.Text = "SAVE";
            _id = scheduleDetailList[3];
            ScheduleName.Text = scheduleDetailList[5];
            MaskedEntryTime.Text = scheduleDetailList[1];
            new Thread(() => ThreadController(scheduleDetailList)).Start();
        }

        public UpdateSchedule(List<Equipment> equipmentList)
        {
            InitializeComponent();
            _equipmentList = equipmentList;
            new Thread(SetUpWeekDays).Start();
            
            PopulateEquipment();
            ButtonCreateSchedule.IsEnabled = true;
            PumpPicker.IsEnabled = true;
        }

        public UpdateSchedule(IReadOnlyList<string> scheduleDetailList, List<Equipment> equipmentList)
        {
            InitializeComponent();
            _equipmentList = equipmentList;

            var selectWeekThread = new Thread(() =>
                SetSelectedWeek(scheduleDetailList[0].Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList()));
            selectWeekThread.Start();


            for (var i = 6; i < scheduleDetailList.Count; i++) _zoneDetailList.Add(scheduleDetailList[i]);
            PopulateEquipment();
            ButtonCreateSchedule.Text = "SAVE";
            _id = scheduleDetailList[3];
            ScheduleName.Text = scheduleDetailList[5];
            MaskedEntryTime.Text = scheduleDetailList[1];
            ButtonCreateSchedule.IsEnabled = true;
            PumpPicker.IsEnabled = true;

        }

        private void PopulateEquipment()
        {
            foreach (var equipment in _equipmentList.Where(equipment => equipment.isPump))
            {
                PumpPicker.Items.Add(equipment.NAME);
                _pumpIdList.Add(equipment.ID);
            }

            if (PumpPicker.Items.Count > 0)
                PumpPicker.SelectedIndex = 0;

            ScrollViewZoneDetail.Children.Clear();

            var zone = "";
            foreach (var equipment in _equipmentList.Where(equipment => equipment.isPump == false))
            {
                zone += equipment.ID + ',' + equipment.NAME + ',' + equipment.GPIO;
                if (equipment.AttachedPiController == null)
                    zone +=  ",0#";
                else
                    zone += "," + equipment.AttachedPiController + "#";
            }

            var zoneDetailObject = _zoneDetailList.Count<1 ? getZoneDetailObject(zone) : getZoneDetailObject(zone, _zoneDetailList);
            foreach (View view in zoneDetailObject) ScrollViewZoneDetail.Children.Add(view);

        }

       

        private void ThreadController(IReadOnlyList<string> scheduleDetailList)
        {
            var selectWeekThread = new Thread(() =>
                SetSelectedWeek(scheduleDetailList[0].Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList()));

            var zoneDetailList = new List<string>();
            for (var i = 6; i < scheduleDetailList.Count; i++) zoneDetailList.Add(scheduleDetailList[i]);
            var populateZoneThread = new Thread(() => PopulateZone(zoneDetailList));

            var populatePumpThread = new Thread(() => PopulatePump(scheduleDetailList));

            selectWeekThread.Start();
            populateZoneThread.Start();
            populatePumpThread.Start();

            while (selectWeekThread.IsAlive || populateZoneThread.IsAlive || populatePumpThread.IsAlive)
                Thread.Sleep(300);
            Device.BeginInvokeOnMainThread(() =>
            {
                ButtonCreateSchedule.IsEnabled = true;
                PumpPicker.IsEnabled = true;
            });
        }

        private void ThreadController()
        {
            var selectWeekThread = new Thread(SetUpWeekDays);

            var populateZoneThread = new Thread(PopulateZone);

            var populatePumpThread = new Thread(PopulatePump);

            selectWeekThread.Start();
            populateZoneThread.Start();
            populatePumpThread.Start();

            while (selectWeekThread.IsAlive || populateZoneThread.IsAlive || populatePumpThread.IsAlive)
                Thread.Sleep(300);
            Device.BeginInvokeOnMainThread(() =>
            {
                ButtonCreateSchedule.IsEnabled = true;
                PumpPicker.IsEnabled = true;
            });
        }


        private void PopulateZone()
        {
            try
            {
                var zone = _socket.Message(_command.getValves());
                Device.BeginInvokeOnMainThread(() =>
                {
                    ScrollViewZoneDetail.Children.Clear();


                    var scheduleList = getZoneDetailObject(zone);
                    foreach (View view in scheduleList) ScrollViewZoneDetail.Children.Add(view);
                });
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ScrollViewZoneDetail.Children.Clear();
                    ScrollViewZoneDetail.Children.Add(new ViewNoConnection());
                });
            }
        }

        private void PopulateZone(IReadOnlyList<string> zoneDetailList)
        {
            try
            {
                var zone = _socket.Message(_command.getValves());
                Device.BeginInvokeOnMainThread(() =>
                {
                    ScrollViewZoneDetail.Children.Clear();


                    var scheduleList = getZoneDetailObject(zone, zoneDetailList);
                    foreach (View view in scheduleList) ScrollViewZoneDetail.Children.Add(view);
                });
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ScrollViewZoneDetail.Children.Clear();
                    ScrollViewZoneDetail.Children.Add(new ViewNoConnection());
                });
            }
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
                zoneListObject = new List<object> {new ViewNoConnection()};
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
                zoneListObject = new List<object> {new ViewNoConnection()};
                return zoneListObject;
            }
        }


        private void PopulatePump()
        {
            try
            {
                var pumps = _socket.Message(_command.getPumps());
                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (var pumpDetail in pumps.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList()
                        .Select(pump => pump.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList()))
                    {
                        PumpPicker.Items.Add(pumpDetail[1]);
                        _pumpIdList.Add(pumpDetail[0]);
                    }

                    if (PumpPicker.Items.Count > 0)
                        PumpPicker.SelectedIndex = 0;
                });
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ScrollViewZoneDetail.Children.Clear();
                    ScrollViewZoneDetail.Children.Add(new ViewNoConnection());
                });
            }
        }

        private void PopulatePump(IReadOnlyList<string> zoneDetailList)
        {
            try
            {
                var pumps = _socket.Message(_command.getPumps());
                Device.BeginInvokeOnMainThread(() =>
                {
                    PumpPicker.Items.Clear();
                    foreach (var pumpDetail in pumps.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList()
                        .Select(pump => pump.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList()))
                    {
                        PumpPicker.Items.Add(pumpDetail[1]);
                        _pumpIdList.Add(pumpDetail[0]);
                    }

                    for (var i = 0; i < _pumpIdList.Count; i++)
                        if (_pumpIdList[i] == zoneDetailList[4])
                            PumpPicker.SelectedIndex = i;
                });
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ScrollViewZoneDetail.Children.Clear();
                    ScrollViewZoneDetail.Children.Add(new ViewNoConnection());
                });
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


        private void SetSelectedWeek(IReadOnlyList<string> weekdaysList)
        {
            //SetUpWeekDays();
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
            foreach (var frame in frames)
            {
                ChangeWeekSelect(frame, weekdaysList.Contains(frame.AutomationId));

                _weekdayList = (List<string>) weekdaysList;
            }
        }

        private static void FramesLoaded(IReadOnlyCollection<Frame> frames)
        {
            for (var i = 0; i < 100; i++)
            {
                foreach (var frame in frames)
                    if (frame.Height > -1 || frame.Width > -1)
                        return;
                Thread.Sleep(30);
            }
        }

        private void Frame_OnTap(object sender, EventArgs e)
        {
            var frame = (Frame) sender;
            var weekday = frame.AutomationId;

            if (_weekdayList.Contains(weekday))
                _weekdayList.Remove(weekday);
            else
                _weekdayList.Add(weekday);

            SetSelectedWeek(_weekdayList);
        }

        private static void ChangeWeekSelect(Frame frame, bool isSelected)
        {
            var frameChildren = frame.Children;
            foreach (var element in frameChildren)
            {
                var child = (StackLayout) element;
                foreach (var view in child.Children)
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        var label = (Label) view;
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
            SendUpdateSchedule();
        }

        private void SendUpdateSchedule()
        {
            var notification = SendUpdateScheduleValidate();
            notification = SendSelectedZonesValidate(notification);

            if (!string.IsNullOrWhiteSpace(notification))
            {
                DisplayAlert("Incomplete", notification, "Understood");
            }
            else
            {
                if (new DatabaseController().IsRealtimeFirebaseSelected())
                {
                    var schedule = new Schedule
                    {
                        ID = _id,
                        NAME = ScheduleName.Text,
                        TIME = MaskedEntryTime.Text,
                        id_Pump = _pumpIdList[PumpPicker.SelectedIndex],
                        isActive = "1",
                        WEEK = _weekdayList.Aggregate("", (current, week) => current + (week + ','))
                    };
                    var scheduleDetail = GetSelectedZonesList();
                    if (scheduleDetail.Count > 0)
                    {
                        schedule.ScheduleDetails = scheduleDetail;
                        var key = Task.Run(() => new Authentication().SetSchedule(schedule)).Result;
                        Navigation.PopModalAsync();
                        Navigation.PopModalAsync();
                        Navigation.PushModalAsync(new ViewScheduleHomeScreen());
                    }
                    else
                        GetViewSchedulePumpTime();
                }
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

            if (_weekdayList.Count == 0)
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

        private string GetSelectedZones()
        {
            var zoneTime = "";


            foreach (var scrollViewZone in ScrollViewZoneDetail.Children)
            {
                var child = (ViewZoneAndTimeGrid) scrollViewZone;
                var maskTime = child.getMaskText();
                if (!string.IsNullOrWhiteSpace(maskTime.Text))
                {
                    if (zoneTime.Length < 1)
                        zoneTime = maskTime.AutomationId + "," + maskTime.Text;
                    else
                        zoneTime += "#" + maskTime.AutomationId + "," + maskTime.Text;
                }
            }

            return zoneTime;
        }

        private List<ScheduleDetail> GetSelectedZonesList()
        {
            var scheduleDetailList = (from ViewZoneAndTimeGrid child in ScrollViewZoneDetail.Children select child.getMaskText() into maskTime where !string.IsNullOrWhiteSpace(maskTime.Text) select new ScheduleDetail {id_Equipment = maskTime.AutomationId, DURATION = maskTime.Text}).ToList();

            return scheduleDetailList;
        }
        private void GetViewSchedulePumpTime()
        {
            var floatingScreen = new FloatingScreen();
            PopupNavigation.Instance.PushAsync(floatingScreen);
            _pumpSelectedTime = new ViewSchedulePumpTime(PumpPicker.Items[PumpPicker.SelectedIndex], _id != null);
            var scheduleSummaryListObject = new List<object> {_pumpSelectedTime};
            _pumpSelectedTime.GetPumpDurationButton().Pressed += UpdateSchedulePumpDuration_Pressed;
            floatingScreen.SetFloatingScreen(scheduleSummaryListObject);
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
                if (new DatabaseController().IsRealtimeFirebaseSelected())
                {
                    var schedule = new Schedule
                    {
                        ID = _id,
                        isActive = "1",
                        NAME = ScheduleName.Text,
                        TIME = MaskedEntryTime.Text,
                        id_Pump = _pumpIdList[PumpPicker.SelectedIndex],
                        WEEK = _weekdayList.Aggregate("", (current, week) => current + (week + ',')),
                        ScheduleDetails = new List<ScheduleDetail>() { new ScheduleDetail() { id_Equipment = _pumpIdList[PumpPicker.SelectedIndex], DURATION = _pumpSelectedTime.getPumpDurationTime().Text } }
                    };
                    var key = Task.Run(() => new Authentication().SetSchedule(schedule)).Result;
                }
                else
                {
                    var schedule = ScheduleName.Text;
                    schedule += "#" + MaskedEntryTime.Text;
                    schedule += "#" + _pumpIdList[PumpPicker.SelectedIndex] + "#";
                    schedule = _weekdayList.Aggregate(schedule, (current, week) => current + "," + week);
                    schedule += "#" + _pumpIdList[PumpPicker.SelectedIndex] + "," +
                                _pumpSelectedTime.getPumpDurationTime().Text;
                    new Thread(() => SendScheduleSocket(schedule)).Start();
                }

                Navigation.PopModalAsync();
                Navigation.PopModalAsync();
                Navigation.PushModalAsync(new ViewScheduleHomeScreen());
            }
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

        private void SendScheduleSocket(string schedule)
        {
            var result = _socket.Message(_id == null
                ? _command.addSchedule(schedule)
                : _command.updateSchedule(Convert.ToInt32(_id), schedule));

            Device.BeginInvokeOnMainThread(() =>
            {
                if (result != "success") DisplayAlert("Warning!!!", result, "Understood");
            });
        }
    }
}