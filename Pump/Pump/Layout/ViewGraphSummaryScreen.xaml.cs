using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pump.Layout.Views;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewGraphSummaryScreen : ContentPage
    {
        private readonly IrrigationCommands _command = new IrrigationCommands();
        private readonly SocketMessage _socket = new SocketMessage();
        public ViewGraphSummaryScreen()
        {
            InitializeComponent();
            new Thread(PopulateEquipmentLastUsed).Start();
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }


        private void PopulateEquipmentLastUsed()
        {
            try
            {
                var equipmentLastUsed = _socket.Message(_command.getEquipmentLastUsed());
                Device.BeginInvokeOnMainThread(() =>
                {

                    ScrollViewLastOnDetail.Children.Clear();


                    var scheduleList = GetEquipmentLastUsed(equipmentLastUsed);
                    foreach (View view in scheduleList)
                    {
                        ScrollViewLastOnDetail.Children.Add(view);
                    }

                });
            }
            catch
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ScrollViewLastOnDetail.Children.Clear();
                    ScrollViewLastOnDetail.Children.Add(new ViewNoConnection());
                });
            }

        }


        private List<object> GetEquipmentLastUsed(string equipmentLastUsed)
        {
            List<object> equipmentLastUsedDetailList = new List<object>();
            try
            {
                if (equipmentLastUsed == "No Data" || equipmentLastUsed == "")
                {
                    equipmentLastUsedDetailList.Add(new ViewEmptySchedule("No Equipment History Found"));
                    return equipmentLastUsedDetailList;
                }


                var equipmentLastUsedList = equipmentLastUsed.Split('$').Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
                
                equipmentLastUsedDetailList.AddRange(equipmentLastUsedList.Select(lastUsed => new ViewEquipmentLastOnHistory(lastUsed.Split('#').ToList())));
                   
                return equipmentLastUsedDetailList;
            }
            catch
            {
                equipmentLastUsedDetailList = new List<object> { new ViewNoConnection() };
                return equipmentLastUsedDetailList;
            }
        }

    }
}