using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EmbeddedImages;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewEquipmentSummary : ContentView
    {
        public readonly Equipment _equipment;
        public ViewEquipmentSummary(Equipment equipment)
        {
            InitializeComponent();
            _equipment = equipment;
            stackLayoutEquipmentSummary.AutomationId = _equipment.ID;
            Populate();
        }

        public void Populate()
        {
            LabelEquipmentName.Text = _equipment.NAME;
            LabelPin.Text = "Pin: " + _equipment.GPIO;
            if(_equipment.DirectOnlineGPIO != null)
                LabelPin.Text += "-" + _equipment.DirectOnlineGPIO;
            if(_equipment.isPump)
                EquipmentImage.Source = ImageSource.FromResource(
                    "Pump.Icons.activePump.png",
                    typeof(ImageResourceExtention).GetTypeInfo().Assembly);
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return StackLayoutViewEquipmentTapGesture;
        }


    }
}