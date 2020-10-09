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

        private void SetScheduleSummary()
        {
            var scheduleTime = new ScheduleTime();
            var runningCustomSchedule = new RunningCustomSchedule();
            var runningScheduleDetail = runningCustomSchedule.getCustomScheduleDetailRunning(schedule);
            var endTime = new RunningCustomSchedule().getCustomScheduleEndTime(schedule);
            if (endTime != null)
            {
                var timeLeft = (TimeSpan)(endTime - scheduleTime.FromUnixTimeStamp(schedule.StartTime));
                LabelCustomSchedule.Text = "Total Duration: " + scheduleTime.convertDateTimeToString(timeLeft);
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