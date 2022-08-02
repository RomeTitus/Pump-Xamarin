using System;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSchedule
    {
        private readonly Equipment _equipment;

        public ViewSchedule(IrrigationController.Schedule schedule, Equipment equipment)
        {
            InitializeComponent();
            _equipment = equipment ?? new Equipment();
            AutomationId = schedule.Id;
            Populate(schedule);
        }

        public void Populate(IrrigationController.Schedule schedule)
        {
            LabelScheduleName.Text = schedule.NAME;
            LabelScheduleTime.Text = schedule.TIME;
            LabelPumpName.Text = _equipment.NAME;
            SwitchScheduleIsActive.IsToggled = !schedule.isActive.Contains("0");
            SetWeek(schedule.WEEK);
            StackLayoutStatus.AddUpdateRemoveStatus(schedule.ControllerStatus);
        }

        private void SetWeek(string week)
        {
            if (week.Contains("SUNDAY"))
            {
                LabelSunday.TextColor = Color.Black;
                LabelSunday.FontAttributes = FontAttributes.Bold;
                LabelSunday.FontSize = 12;
            }

            if (week.Contains("MONDAY"))
            {
                LabelMonday.TextColor = Color.Black;
                LabelMonday.FontAttributes = FontAttributes.Bold;
                LabelMonday.FontSize = 12;
            }

            if (week.Contains("TUESDAY"))
            {
                LabelTuesday.TextColor = Color.Black;
                LabelTuesday.FontAttributes = FontAttributes.Bold;
                LabelTuesday.FontSize = 12;
            }

            if (week.Contains("WEDNESDAY"))
            {
                LabelWednesday.TextColor = Color.Black;
                LabelWednesday.FontAttributes = FontAttributes.Bold;
                LabelWednesday.FontSize = 12;
            }

            if (week.Contains("THURSDAY"))
            {
                LabelThursday.TextColor = Color.Black;
                LabelThursday.FontAttributes = FontAttributes.Bold;
                LabelThursday.FontSize = 12;
            }

            if (week.Contains("FRIDAY"))
            {
                LabelFriday.TextColor = Color.Black;
                LabelFriday.FontAttributes = FontAttributes.Bold;
                LabelFriday.FontSize = 12;
            }

            if (week.Contains("SATURDAY"))
            {
                LabelSaturday.TextColor = Color.Black;
                LabelSaturday.FontAttributes = FontAttributes.Bold;
                LabelSaturday.FontSize = 12;
            }
        }

        public Switch GetSwitch()
        {
            return SwitchScheduleIsActive;
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return StackLayoutViewScheduleTapGesture;
        }

        private void SwitchTapGestureRecognizer(object sender, EventArgs e)
        {
            SwitchScheduleIsActive.IsToggled = !SwitchScheduleIsActive.IsToggled;
        }
        
        public void AddStatusActivityIndicator()
        {
            StackLayoutStatus.AddStatusActivityIndicator();
        }
    }
}