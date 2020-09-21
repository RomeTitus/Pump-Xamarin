using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pump.Database;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewCustomScheduleHomeScreen : ContentPage
    {
        public ViewCustomScheduleHomeScreen()
        {
            InitializeComponent();
        }

        private void ButtonCreateCustomSchedule_OnClicked(object sender, EventArgs e)
        {
            /*
            if (new DatabaseController().IsRealtimeFirebaseSelected())
            {
                if (_equipmentList.Count > 0)
                    Navigation.PushModalAsync(new UpdateSchedule(_equipmentList));
                else
                    DisplayAlert("Cannot Create a Schedule", "You are missing the equipment that is needed to create a schedule", "Understood");

            }
            else
            */
                Navigation.PushModalAsync(new UpdateCustomSchedule());
        }
    }
}