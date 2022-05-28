using System.Collections.Generic;
using System.Linq;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewScheduleSummary : ContentView
    {
        private readonly List<Equipment> _equipmentList;
        private readonly IrrigationController.Schedule _schedule;

        public ViewScheduleSummary(IrrigationController.Schedule schedule, List<Equipment> equipmentList)
        {
            InitializeComponent();
            _schedule = schedule;
            _equipmentList = equipmentList;
            SetScheduleSummary();
        }

        private void SetScheduleSummary()
        {
            labelScheduleTime.Text = _schedule.TIME;
            LabelPumpName.Text = _equipmentList.FirstOrDefault(x => x.ID == _schedule.id_Pump)?.NAME;
            labelScheduleName.Text = _schedule.NAME;
            SetWeek();
            foreach (var scheduleDetail in _schedule.ScheduleDetails)
                ScrollViewZoneDetail.Children.Add(new ViewZoneAndTimeGrid(scheduleDetail,
                    _equipmentList.FirstOrDefault(x => x?.ID == scheduleDetail.id_Equipment), true));
        }

        private void SetWeek()
        {
            if (_schedule.WEEK.Contains("SUNDAY"))
            {
                LabelSunday.TextColor = Color.Black;
                LabelSunday.FontAttributes = FontAttributes.Bold;
                LabelSunday.FontSize = 24;
            }

            if (_schedule.WEEK.Contains("MONDAY"))
            {
                LabelMonday.TextColor = Color.Black;
                LabelMonday.FontAttributes = FontAttributes.Bold;
                LabelMonday.FontSize = 24;
            }

            if (_schedule.WEEK.Contains("TUESDAY"))
            {
                LabelTuesday.TextColor = Color.Black;
                LabelTuesday.FontAttributes = FontAttributes.Bold;
                LabelTuesday.FontSize = 24;
            }

            if (_schedule.WEEK.Contains("WEDNESDAY"))
            {
                LabelWednesday.TextColor = Color.Black;
                LabelWednesday.FontAttributes = FontAttributes.Bold;
                LabelWednesday.FontSize = 24;
            }

            if (_schedule.WEEK.Contains("THURSDAY"))
            {
                LabelThursday.TextColor = Color.Black;
                LabelThursday.FontAttributes = FontAttributes.Bold;
                LabelThursday.FontSize = 24;
            }

            if (_schedule.WEEK.Contains("FRIDAY"))
            {
                LabelFriday.TextColor = Color.Black;
                LabelFriday.FontAttributes = FontAttributes.Bold;
                LabelFriday.FontSize = 24;
            }

            if (_schedule.WEEK.Contains("SATURDAY"))
            {
                LabelSaturday.TextColor = Color.Black;
                LabelSaturday.FontAttributes = FontAttributes.Bold;
                LabelSaturday.FontSize = 24;
            }
        }


        public Button GetButtonEdit()
        {
            if (ButtonEdit.AutomationId == null)
                ButtonEdit.AutomationId = _schedule.ID;
            return ButtonEdit;
        }

        public Button GetButtonDelete()
        {
            if (ButtonDelete.AutomationId == null)
                ButtonDelete.AutomationId = _schedule.ID;
            return ButtonDelete;
        }
    }
}