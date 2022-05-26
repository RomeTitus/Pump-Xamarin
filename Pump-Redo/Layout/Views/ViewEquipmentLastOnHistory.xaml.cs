using System;
using System.Collections.Generic;
using System.Reflection;
using EmbeddedImages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewEquipmentLastOnHistory : ContentView
    {
        public ViewEquipmentLastOnHistory(IReadOnlyList<string> equipmentLastOnList)
        {
            InitializeComponent();

            LabelEquipmentName.Text = equipmentLastOnList[0];
            LabelLastOn.Text = equipmentLastOnList[3];
            LabelDurationTime.Text = SetTime(Convert.ToInt32(equipmentLastOnList[4]));
            if (equipmentLastOnList[2] != "1") return;
            EquipmentImage.Source = ImageSource.FromResource(
                "Pump-Redo.Icons.activePump.png",
                typeof(ImageResourceExtention).GetTypeInfo().Assembly);
            StackLayoutViewEquipmentLastOn.BackgroundColor = Color.DarkTurquoise;
        }

        private static string SetTime(int time)
        {
            if (time < 60)
                return time + "min";

            var hour = 0;
            var stillDivide = true;
            while (stillDivide)
                if (time - 60 >= 0)
                {
                    time -= 60;
                    hour++;
                }
                else
                {
                    stillDivide = false;
                }

            if (time == 0)
                return hour + "h";

            return hour + "h" + time + "min";
        }
    }
}