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
    public partial class ViewManualSchedule : ContentView
    {
        public ViewManualSchedule(List<string> manual, bool isManualWithSchedule)
        {
            InitializeComponent();
            setManualText(manual, isManualWithSchedule);
        }

        private void setManualText(List<string> manual, bool isManualWithSchedule)
        {
            if (isManualWithSchedule)
                LableManual.Text = "Manual Running with Schedule";
            else
                LableManual.Text = "Manual Running without Schedule";
            
            LableManualTime.Text = "Ends at: " + manual[0];
        }
        
    }
}