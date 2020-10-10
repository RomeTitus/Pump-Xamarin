using System;
using System.Collections.Generic;
using System.Linq;
using Pump.Class;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewCustomScheduleSummary : ContentView
    {
        private readonly List<Equipment> _equipmentList = null;
        private readonly FloatingScreen _floatingScreen;
        public readonly CustomSchedule schedule;
        readonly List<TapGestureRecognizer> _zoneAndTimeTapGesture = new List<TapGestureRecognizer>();
        public ViewCustomScheduleSummary()
        {
            InitializeComponent();
        }

        public ViewCustomScheduleSummary(CustomSchedule schedule, FloatingScreen floatingScreen, List<Equipment> equipmentList)
        {
            InitializeComponent();
            _floatingScreen = floatingScreen;
            this.schedule = schedule;
            _equipmentList = equipmentList;
            SetScheduleSummary();
        }

        public void UpdateScheduleSummary()
        {
            var scheduleTime = new ScheduleTime();
            var runningCustomSchedule = new RunningCustomSchedule();
            var runningScheduleDetail = RunningCustomSchedule.GetCustomScheduleDetailRunning(schedule);
            var endTime = new RunningCustomSchedule().getCustomScheduleEndTime(schedule);
            if (endTime != null)
            {
                var timeLeft = (TimeSpan)(endTime - ScheduleTime.FromUnixTimeStampLocal(schedule.StartTime));
                LabelCustomSchedule.Text = "Total Duration: " + ScheduleTime.ConvertTimeSpanToString(timeLeft);
            }

            foreach (var equipment in _equipmentList.Where(equipment => equipment.ID == schedule.id_Pump))
            {
                LabelPumpName.Text = equipment.NAME;
            }

            labelScheduleName.Text = schedule.NAME;


            foreach (ViewZoneAndTimeGrid scheduleGrid in ScrollViewZoneDetail.Children)
            {
                
                scheduleGrid.SetBackGroundColour(Color.Yellow);
                if (runningScheduleDetail != null)
                {
                    if (scheduleGrid.Id == runningScheduleDetail.id_Equipment)
                        scheduleGrid.SetBackGroundColour(Color.YellowGreen);
                }
            }
        }
        public void SetScheduleSummary()
        {
            var scheduleTime = new ScheduleTime();
            var runningCustomSchedule = new RunningCustomSchedule();
            var runningScheduleDetail = RunningCustomSchedule.GetCustomScheduleDetailRunning(schedule);
            var endTime = new RunningCustomSchedule().getCustomScheduleEndTime(schedule);
            if (endTime != null)
            {
                var timeLeft = (TimeSpan)(endTime - ScheduleTime.FromUnixTimeStampLocal(schedule.StartTime));
                LabelCustomSchedule.Text = "Total Duration: " + ScheduleTime.ConvertTimeSpanToString(timeLeft);
            }



            foreach (var equipment in _equipmentList.Where(equipment => equipment.ID == schedule.id_Pump))
            {
                LabelPumpName.Text = equipment.NAME;
            }

            labelScheduleName.Text = schedule.NAME;

            
            foreach (var scheduleEquipment in schedule.ScheduleDetails)
            {
                var scheduleGrid = new ViewZoneAndTimeGrid(scheduleEquipment,
                    _equipmentList.First(x => x.ID == scheduleEquipment.id_Equipment),
                    true);
                _zoneAndTimeTapGesture.Add(scheduleGrid.GetTapGesture());
                scheduleGrid.SetBackGroundColour(Color.Yellow);
                if (runningScheduleDetail != null)
                {
                    if(scheduleEquipment == runningScheduleDetail)
                        scheduleGrid.SetBackGroundColour(Color.YellowGreen);
                }
                
                ScrollViewZoneDetail.Children.Add(scheduleGrid);
            }
        }

        public List<TapGestureRecognizer> GetZoneAndTimeGestureRecognizers()
        {
            return _zoneAndTimeTapGesture;
        }

        public Button GetButtonEdit()
        {
            if(ButtonEdit.AutomationId == null)
                ButtonEdit.AutomationId = schedule.ID;
            return ButtonEdit;
        }

        public Button GetButtonDelete()
        {
            if(ButtonDelete.AutomationId == null)
                ButtonDelete.AutomationId = schedule.ID;
            return ButtonDelete;
        }
    }
}