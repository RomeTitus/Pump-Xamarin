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
        private readonly List<Equipment> _equipmentList;
        public readonly CustomSchedule CustomSchedule;
        private readonly List<TapGestureRecognizer> _zoneAndTimeTapGesture = new List<TapGestureRecognizer>();
        public ViewCustomScheduleSummary()
        {
            InitializeComponent();
        }

        public ViewCustomScheduleSummary(CustomSchedule customSchedule, List<Equipment> equipmentList)
        {
            InitializeComponent();
            this.CustomSchedule = customSchedule;
            _equipmentList = equipmentList;
            SetScheduleSummary();
        }

        public void UpdateScheduleSummary()
        {
            var runningScheduleDetail = RunningCustomSchedule.GetCustomScheduleDetailRunning(CustomSchedule);
            var endTime = new RunningCustomSchedule().getCustomScheduleEndTime(CustomSchedule);
            if (endTime != null)
            {
                var timeLeft = (TimeSpan)(endTime - ScheduleTime.FromUnixTimeStampLocal(CustomSchedule.StartTime));
                LabelCustomSchedule.Text = "Total Duration: " + ScheduleTime.ConvertTimeSpanToString(timeLeft);
            }

            foreach (var equipment in _equipmentList.Where(equipment => equipment.ID == CustomSchedule.id_Pump))
            {
                LabelPumpName.Text = equipment.NAME;
            }
            labelScheduleName.Text = CustomSchedule.NAME;
            foreach (ViewZoneAndTimeGrid scheduleGrid in ScrollViewZoneDetail.Children)
            {
                
                scheduleGrid.SetBackGroundColor(Color.Yellow);
                if (runningScheduleDetail != null)
                {
                    if (scheduleGrid.AutomationId == runningScheduleDetail.id_Equipment)
                        scheduleGrid.SetBackGroundColor(Color.YellowGreen);
                }
            }
        }

        private void SetScheduleSummary()
        {
            var runningScheduleDetail = RunningCustomSchedule.GetCustomScheduleDetailRunning(CustomSchedule);
            var endTime = new RunningCustomSchedule().getCustomScheduleEndTime(CustomSchedule);
            if (endTime != null)
            {
                var timeLeft = (TimeSpan)(endTime - ScheduleTime.FromUnixTimeStampLocal(CustomSchedule.StartTime));
                LabelCustomSchedule.Text = "Total Duration: " + ScheduleTime.ConvertTimeSpanToString(timeLeft);
            }

            foreach (var equipment in _equipmentList.Where(equipment => equipment.ID == CustomSchedule.id_Pump))
            {
                LabelPumpName.Text = equipment.NAME;
            }

            labelScheduleName.Text = CustomSchedule.NAME;

            
            foreach (var scheduleEquipment in CustomSchedule.ScheduleDetails)
            {
                var scheduleGrid = new ViewZoneAndTimeGrid(scheduleEquipment,
                    _equipmentList.First(x => x.ID == scheduleEquipment.id_Equipment),
                    true);
                _zoneAndTimeTapGesture.Add(scheduleGrid.GetTapGesture());
                scheduleGrid.SetBackGroundColor(Color.Yellow);
                if (runningScheduleDetail != null)
                {
                    if(scheduleEquipment == runningScheduleDetail)
                        scheduleGrid.SetBackGroundColor(Color.YellowGreen);
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
                ButtonEdit.AutomationId = CustomSchedule.ID;
            return ButtonEdit;
        }

        public Button GetButtonDelete()
        {
            if(ButtonDelete.AutomationId == null)
                ButtonDelete.AutomationId = CustomSchedule.ID;
            return ButtonDelete;
        }
    }
}