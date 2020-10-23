using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EmbeddedImages;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSubControllerSummary : ContentView
    {
        public readonly SubController SubController;
        public ViewSubControllerSummary(SubController subController)
        {
            InitializeComponent();
            SubController = subController;
            stackLayoutSubControllerSummary.AutomationId = SubController.ID;
            Populate();
        }

        public void Populate()
        {
            LabelSubControllerName.Text = SubController.NAME;
            if(SubController.IpAdress == null)
                LabelType.Text = "LoRa";
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return StackLayoutViewSubControllerTapGesture;
        }
    }
}