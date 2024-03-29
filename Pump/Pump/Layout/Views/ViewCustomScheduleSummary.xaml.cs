﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public ViewCustomScheduleSummary(CustomSchedule customSchedule, List<Equipment> equipmentList)
        {
            InitializeComponent();
            CustomSchedule = customSchedule;
            _equipmentList = equipmentList;
            SetScheduleSummary();
        }

        public void UpdateScheduleSummary()
        {
            var runningScheduleDetail = RunningCustomSchedule.GetCustomScheduleDetailRunning(CustomSchedule);
            var endTime = new RunningCustomSchedule().getCustomScheduleEndTime(CustomSchedule);
            if (endTime != null)
            {
                var timeLeft = (TimeSpan) (endTime - ScheduleTime.FromUnixTimeStampLocal(CustomSchedule.StartTime));
                LabelCustomSchedule.Text = "Total Duration: " + ScheduleTime.ConvertTimeSpanToString(timeLeft);
            }

            foreach (var equipment in _equipmentList.Where(equipment => equipment.ID == CustomSchedule.id_Pump))
            {
                LabelPumpName.Text = equipment.NAME;
            }

            labelScheduleName.Text = CustomSchedule.NAME;

            foreach (var view in ScrollViewZoneDetail.Children)
            {
                var scheduleGrid = (ViewZoneAndTimeGrid) view;

                scheduleGrid.SetBackGroundColor(Color.Yellow);
                if (runningScheduleDetail != null)
                {
                    if (scheduleGrid.AutomationId == runningScheduleDetail.ID)
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
                var timeLeft = (TimeSpan) (endTime - ScheduleTime.FromUnixTimeStampLocal(CustomSchedule.StartTime));
                LabelCustomSchedule.Text = "Total Duration: " + ScheduleTime.ConvertTimeSpanToString(timeLeft);
            }

            foreach (var equipment in _equipmentList.Where(equipment => equipment.ID == CustomSchedule.id_Pump))
            {
                LabelPumpName.Text = equipment.NAME;
            }

            labelScheduleName.Text = CustomSchedule.NAME;

            var index = 0;
            for (var i = 0; i < CustomSchedule.Repeat + 1; i++)
            {
                foreach (var scheduleEquipment in CustomSchedule.ScheduleDetails)
                {
                    scheduleEquipment.ID = index.ToString();
                    var scheduleGrid = new ViewZoneAndTimeGrid(scheduleEquipment,
                        _equipmentList.FirstOrDefault(x => x.ID == scheduleEquipment.id_Equipment),
                        true);
                    _zoneAndTimeTapGesture.Add(scheduleGrid.GetTapGesture());
                    scheduleGrid.SetBackGroundColor(Color.Yellow);
                    scheduleGrid.AutomationId = scheduleEquipment.ID;
                    if (runningScheduleDetail != null)
                    {
                        if (scheduleEquipment.ID == runningScheduleDetail.ID) //Why does the ID randomly Change?
                            scheduleGrid.SetBackGroundColor(Color.YellowGreen);
                    }

                    ScrollViewZoneDetail.Children.Add(scheduleGrid);
                    index++;
                }
            }
        }

        public List<TapGestureRecognizer> GetZoneAndTimeGestureRecognizers()
        {
            return _zoneAndTimeTapGesture;
        }

        public Button GetButtonEdit()
        {
            if (ButtonEdit.AutomationId == null)
                ButtonEdit.AutomationId = CustomSchedule.ID;
            return ButtonEdit;
        }

        public Button GetButtonDelete()
        {
            if (ButtonDelete.AutomationId == null)
                ButtonDelete.AutomationId = CustomSchedule.ID;
            return ButtonDelete;
        }
    }
}