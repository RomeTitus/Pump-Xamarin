using System.Collections.Generic;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.MaskedEntry;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewZoneAndTimeGrid : ContentView
    {
        public ViewZoneAndTimeGrid(ScheduleDetail scheduleDetail, Equipment equipment, bool isTimeSet)
        {
            InitializeComponent();

            LabelZoneTime.AutomationId = equipment.ID;
            LabelZoneName.Text = equipment.NAME;
            if (isTimeSet)
                LabelZoneTime.Text = scheduleDetail.DURATION;
        }
        public ViewZoneAndTimeGrid(IReadOnlyList<string> zoneAndTimeList, bool isTimeSet)
        {
            InitializeComponent();

            LabelZoneTime.AutomationId = zoneAndTimeList[0];
            LabelZoneName.Text = zoneAndTimeList[1];
            if (isTimeSet)
                LabelZoneTime.Text = zoneAndTimeList[2];
        }

        public MaskedEntry getMaskText()
        {
            return LabelZoneTime;
        }

        public Label getZoneNameText()
        {
            return LabelZoneName;
        }
    }
}