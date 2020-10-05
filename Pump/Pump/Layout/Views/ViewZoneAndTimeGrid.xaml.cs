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
        private string Id = "";
        public ViewZoneAndTimeGrid(ScheduleDetail scheduleDetail, Equipment equipment, bool isTimeSet)
        {
            InitializeComponent();
            Id = equipment.ID;
            LabelZoneTime.AutomationId = equipment.ID;
            LabelZoneName.Text = equipment.NAME;
            if (isTimeSet)
                LabelZoneTime.Text = scheduleDetail.DURATION;
        }
        public ViewZoneAndTimeGrid(IReadOnlyList<string> zoneAndTimeList, bool isTimeSet)
        {
            InitializeComponent();
            Id = zoneAndTimeList[0];
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

        public void SetBackGroundColour(Color colour)
        {
            ZoneAndTimeGrid.BackgroundColor = colour;
        }

        public TapGestureRecognizer GetTapGesture()
        {
            ZoneAndTimeGrid.AutomationId = Id;
            return GridViewScheduleTapGesture;
        }
    }
}