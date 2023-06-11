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
        private readonly List<TapGestureRecognizer> _zoneAndTimeTapGesture = new List<TapGestureRecognizer>();
        public readonly CustomSchedule CustomSchedule;

        public ViewCustomScheduleSummary(CustomSchedule customSchedule, List<Equipment> equipmentList)
        {
            InitializeComponent();
            CustomSchedule = customSchedule;
            _equipmentList = equipmentList;
            SetScheduleSummary();
        }

        public void UpdateScheduleSummary()
        {
            var runningScheduleDetail = CustomSchedule.GetScheduleDetailRunning();
            var endTime = CustomSchedule.GetScheduleEndTime();
            if (endTime != null)
            {
                var timeLeft = (TimeSpan)(endTime - ScheduleTime.FromUnixTimeStampLocal(CustomSchedule.StartTime));
                LabelCustomSchedule.Text = "Total Duration: " + ScheduleTime.ConvertTimeSpanToString(timeLeft);
            }

            foreach (var equipment in _equipmentList.Where(equipment => equipment.Id == CustomSchedule.id_Pump))
                LabelPumpName.Text = equipment.NAME;

            labelScheduleName.Text = CustomSchedule.NAME;

            foreach (var view in ScrollViewZoneDetail.Children)
            {
                var scheduleGrid = (ViewZoneAndTimeGrid)view;

                scheduleGrid.SetBackGroundColor(Color.Yellow);
                if (runningScheduleDetail != null)
                    if (scheduleGrid.AutomationId == runningScheduleDetail.ID)
                        scheduleGrid.SetBackGroundColor(Color.YellowGreen);
            }
        }

        private void SetScheduleSummary()
        {
            var runningScheduleDetail = CustomSchedule.GetScheduleDetailRunning();
            var endTime = CustomSchedule.GetScheduleEndTime();
            if (endTime != null)
            {
                var timeLeft = (TimeSpan)(endTime - ScheduleTime.FromUnixTimeStampLocal(CustomSchedule.StartTime));
                LabelCustomSchedule.Text = "Total Duration: " + ScheduleTime.ConvertTimeSpanToString(timeLeft);
            }

            foreach (var equipment in _equipmentList.Where(equipment => equipment.Id == CustomSchedule.id_Pump))
                LabelPumpName.Text = equipment.NAME;

            labelScheduleName.Text = CustomSchedule.NAME;

            var index = 0;
            for (var i = 0; i < CustomSchedule.Repeat + 1; i++)
                foreach (var scheduleEquipment in CustomSchedule.ScheduleDetails)
                {
                    scheduleEquipment.ID = index.ToString();
                    var scheduleGrid = new ViewZoneAndTimeGrid(scheduleEquipment,
                        _equipmentList.FirstOrDefault(x => x.Id == scheduleEquipment.id_Equipment),
                        true);
                    _zoneAndTimeTapGesture.Add(scheduleGrid.GetTapGesture());
                    scheduleGrid.SetBackGroundColor(Color.Yellow);
                    scheduleGrid.AutomationId = scheduleEquipment.ID;
                    if (runningScheduleDetail != null)
                        if (scheduleEquipment.ID == runningScheduleDetail.ID) //Why does the ID randomly Change?
                            scheduleGrid.SetBackGroundColor(Color.YellowGreen);

                    ScrollViewZoneDetail.Children.Add(scheduleGrid);
                    index++;
                }
        }

        public List<TapGestureRecognizer> GetZoneAndTimeGestureRecognizers()
        {
            return _zoneAndTimeTapGesture;
        }

        public Button GetButtonEdit()
        {
            if (ButtonEdit.AutomationId == null)
                ButtonEdit.AutomationId = CustomSchedule.Id;
            return ButtonEdit;
        }

        public Button GetButtonDelete()
        {
            if (ButtonDelete.AutomationId == null)
                ButtonDelete.AutomationId = CustomSchedule.Id;
            return ButtonDelete;
        }
    }
}