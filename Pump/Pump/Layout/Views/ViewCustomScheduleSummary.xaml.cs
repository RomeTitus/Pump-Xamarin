using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly CustomSchedule schedule;

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

        private void SetScheduleSummary()
        {
            var scheduleTime = new ScheduleTime();
            if (schedule.StartTime != 0)
            {
                var timeSpent = DateTime.Now - scheduleTime.FromUnixTimeStamp(schedule.StartTime);
                labelScheduleTime.Text = "Time Spent: " + scheduleTime.convertDateTimeToString(timeSpent);
            }


            foreach (var equipment in _equipmentList.Where(equipment => equipment.ID == schedule.id_Pump))
            {
                LabelPumpName.Text = equipment.NAME;
            }

            labelScheduleName.Text = schedule.NAME;

            foreach (var scheduleEquipment in schedule.ScheduleDetails)
            {
                ScrollViewZoneDetail.Children.Add(
                    new ViewZoneAndTimeGrid(scheduleEquipment, _equipmentList.First(x => x.ID == scheduleEquipment.id_Equipment),
                        true));
            }
        }

        public Button GetButtonEdit()
        {
            ButtonEdit.AutomationId = schedule.ID;
            return ButtonEdit;
        }

        public Button GetButtonDelete()
        {
            ButtonDelete.AutomationId = schedule.ID;
            return ButtonDelete;
        }
    }
}