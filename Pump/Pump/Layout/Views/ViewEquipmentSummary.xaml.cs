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
        public Equipment Equipment;
        public ViewEquipmentSummary(Equipment equipment)
        {
            InitializeComponent();
            Equipment = equipment;
            stackLayoutEquipmentSummary.AutomationId = Equipment.ID;
            Populate();
        }

        public void Populate()
        {
            LabelEquipmentName.Text = Equipment.NAME;
            LabelPin.Text = "Pin: " + Equipment.GPIO;
            if(Equipment.DirectOnlineGPIO != null)
                LabelPin.Text += "-" + Equipment.DirectOnlineGPIO;
            if(Equipment.isPump)
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