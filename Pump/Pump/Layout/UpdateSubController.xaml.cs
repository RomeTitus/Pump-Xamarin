using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdateSubController : ContentPage
    {
        private SubController _subController;
        public UpdateSubController(SubController subController = null)
        {
            InitializeComponent();
            if (subController == null)
            {
                subController = new SubController();
            }

            _subController = subController;
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void ButtonUpdateSubController_OnClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SubControllerIp.Text) && string.IsNullOrEmpty(SubControllerPort.Text))
            {
                
                DisplayAlert("Select LoRa Mode",
                    "Confirm to find and pair Sub Controller ", "Search",
                    "Cancel");
            }
        }
    }
}