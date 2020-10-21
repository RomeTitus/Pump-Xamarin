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
        public readonly PiController _piController;
        public ViewSubControllerSummary(PiController piController)
        {
            InitializeComponent();
            _piController = piController;
            stackLayoutSubControllerSummary.AutomationId = _piController.ID;
            Populate();
        }

        public void Populate()
        {
            LabelSubControllerName.Text = _piController.NAME;
            if(_piController.IpAdress == null)
                LabelType.Text = "LoRa";
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return StackLayoutViewSubControllerTapGesture;
        }
    }
}