using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UpdateCustomSchedule : ContentPage
    {
        public UpdateCustomSchedule()
        {
            InitializeComponent();
        }

        private void ButtonCreateCustomSchedule_OnClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }
    }
}