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
        private readonly Equipment _equipment;
        private readonly ScheduleDetail _scheduleDetail;
        public ViewZoneAndTimeGrid(ScheduleDetail scheduleDetail, Equipment equipment, bool isTimeSet = false)
        {
            InitializeComponent();
            _equipment = equipment;
            _scheduleDetail = scheduleDetail;
            LabelZoneTime.AutomationId = _equipment.ID;
            LabelZoneName.Text = _equipment.NAME;
            if (isTimeSet && scheduleDetail != null)
                LabelZoneTime.Text = scheduleDetail.DURATION;
        }

        public MaskedEntry GetMaskText()
        {
            return LabelZoneTime;
        }

        public Label GetZoneNameText()
        {
            return LabelZoneName;
        }

        public void SetBackGroundColor(Color color)
        {
            ZoneAndTimeGrid.BackgroundColor = color;
        }

        public TapGestureRecognizer GetTapGesture()
        {
            ZoneAndTimeGrid.AutomationId = _scheduleDetail.ID;
            return GridViewScheduleTapGesture;
        }
    }
}