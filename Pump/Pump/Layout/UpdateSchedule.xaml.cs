using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pump.Layout.Views;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdateSchedule : ContentPage
    {
        private List<string> _weekdayList= new List<string>();
        public readonly List<string> _pumpIdList = new List<string>();


        readonly SocketCommands _command = new SocketCommands();
        readonly SocketMessage _socket = new SocketMessage();
        public UpdateSchedule()
        {
            InitializeComponent();
            new Thread(SetUpWeekDays).Start();
            new Thread(PopulateZone).Start();
            new Thread(PopulatePump).Start();
        }

        public UpdateSchedule(IReadOnlyList<string> scheduleDetailList)
        {
            InitializeComponent();

            new Thread(() => SetUpWeekDays(scheduleDetailList[0].Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList())).Start();

            List<string> zoneDetailList = new List<string>();
            for (int i = 6; i < scheduleDetailList.Count; i++)
            {
                zoneDetailList.Add(scheduleDetailList[i]);
            }
            new Thread(() => PopulateZone(zoneDetailList)).Start();

            new Thread(() => PopulatePump(scheduleDetailList)).Start();

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
                        foreach (View view in scheduleList)
                        {
                            ScrollViewZoneDetail.Children.Add(view);
                        }

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
                    foreach (View view in scheduleList)
                    {
                        ScrollViewZoneDetail.Children.Add(view);
                    }

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
            List<object> zoneListObject = new List<object>();
            try
            {
                if (zone == "No Data" || zone == "")
                {
                    zoneListObject.Add(new ViewEmptySchedule("No Zones Found"));
                    return zoneListObject;
                }


                var zoneList = zone.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();

                zoneListObject.AddRange(zoneList.Select(schedule => new ViewZoneAndTimeGrid(schedule.Split(',').ToList(), false)).Cast<object>());
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
            List<object> zoneListObject = new List<object>();
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
                    bool hasProperty = false;
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
                    if(!hasProperty)
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


        private void PopulatePump()
        {
            try
            {
                var pumps = _socket.Message(_command.getPumps());
                Device.BeginInvokeOnMainThread(() =>
                {
                    foreach (var pumpDetail in pumps.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList().Select(pump => pump.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList()))
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
                    foreach (var pumpDetail in pumps.Split('#').Where(x => !string.IsNullOrWhiteSpace(x)).ToList().Select(pump => pump.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).ToList()))
                    {
                        PumpPicker.Items.Add(pumpDetail[1]);
                        _pumpIdList.Add(pumpDetail[0]);
                    }

                    for (int i = 0; i < _pumpIdList.Count; i++)
                    {
                        if (_pumpIdList[i] == zoneDetailList[4])
                            PumpPicker.SelectedIndex = i;
                    }
                    
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
               
                frame.BorderColor = Color.Black;
                frame.BackgroundColor = Color.DeepSkyBlue;
            }
        }

        private void setSelectedWeek(IReadOnlyList<string> weekdaysList)
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
            foreach (var frame in frames)
            {
                foreach (var weekday in weekdaysList)
                {
                    
                    if (frame.AutomationId.Contains(weekday))
                    {
                        ChangeWeekSelect(frame, true);
                    }
                }

                _weekdayList = (List<string>) weekdaysList;
            }
        }

        private void SetUpWeekDays(IReadOnlyList<string> weekdaysList)
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

                frame.BorderColor = Color.Black;
                frame.BackgroundColor = Color.DeepSkyBlue;
            }

            setSelectedWeek(weekdaysList);
        }

        private static void FramesLoaded(IReadOnlyCollection<Frame> frames)
        {
            for (int i = 0; i < 100; i++)
            {
                foreach (var frame in frames)
                {
                    if (frame.Height > -1 || frame.Width > -1)
                        return;
                }
                Thread.Sleep(30);
            }
        }

        private void Frame_OnTap(object sender, EventArgs e)
        {
            var frame = (Frame) sender;
            var weekday = frame.AutomationId;

            if (_weekdayList.Contains(weekday))
            {
                _weekdayList.Remove(weekday);
                ChangeWeekSelect(frame, false);
            }
            else
            {
                _weekdayList.Add(weekday);
                ChangeWeekSelect(frame, true);
            }
            
        }

        private static void ChangeWeekSelect(Frame frame, bool isSelected)
        {
            frame.BackgroundColor = isSelected ? Color.White : Color.DeepSkyBlue;

            var frameChildren = frame.Children;
            foreach (var element in frameChildren)
            {
                var child = (StackLayout) element;
                foreach (var view in child.Children)
                {
                    var label = (Label) view;
                    label.BackgroundColor = isSelected ? Color.White : Color.DeepSkyBlue;
                }
            }
        }

        private void ButtonUpdateBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void ButtonUpdateSchedule_OnClicked(object sender, EventArgs e)
        {
            
        }
    }
}