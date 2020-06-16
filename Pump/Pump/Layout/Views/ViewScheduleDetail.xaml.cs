using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewScheduleDetail : ContentView
    {

        public ViewScheduleDetail(List<string> schedule)
        {
            InitializeComponent();

            PopulateSchedule(schedule);
        }

        private void PopulateSchedule(IReadOnlyList<string> schedule)
        {
            LabelScheduleName.Text = schedule[1];
            LablePump.Text = schedule[2];
            LableZone.Text = schedule[3];
            LableStartTime.Text = schedule[4];
            LableEndTime.Text = schedule[5];
        }
    }
}