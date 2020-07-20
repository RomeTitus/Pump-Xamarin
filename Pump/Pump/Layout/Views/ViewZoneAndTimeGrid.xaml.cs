using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.MaskedEntry;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewZoneAndTimeGrid : ContentView
    {
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