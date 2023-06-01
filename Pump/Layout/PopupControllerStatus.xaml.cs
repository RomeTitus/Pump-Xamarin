using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Pump.Class;
using Pump.CustomRender;
using Pump.IrrigationController;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PopupControllerStatus : PopupPage
    {

        public PopupControllerStatus(ControllerStatus status)
        {
            InitializeComponent();
        }

      
        public void Populate(ControllerStatus status)
        {
            if (status.LastUpdated != null)
                LabelExecutionTime.Text = ScheduleTime.FromUnixTimeStampUtc(status.LastUpdated.Value).ToLocalTime()
                    .ToString("dd/MM/yyyy HH:mm")
                    .ToString(CultureInfo.InvariantCulture);
            CheckBoxCompleted.IsChecked = status.Complete;
            StatusLabel.Text = status.StatusType?.ToString();
            var steps = "";
            status.Steps.ForEach(x => steps += x);
            LabelSteps.Text = steps;
        }

        private void ButtonCancel_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }
    }
}