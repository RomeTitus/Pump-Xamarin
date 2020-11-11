using Pump.IrrigationController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ViewSiteSummary : ContentView
    {
        public readonly Site _site;
        public ViewSiteSummary(Site site)
        {
            InitializeComponent();
            _site = site;
            AutomationId = _site.ID;
            stackLayoutSiteSummary.AutomationId = _site.ID;
            Populate();
        }
        public void Populate()
        {
            LabelSiteName.Text = _site.NAME;
            LabelSiteDescription.Text = _site.Description;
        }

        public void setBackgroundColor(Color color)
        {
            stackLayoutSiteSummary.BackgroundColor = color;
        }

        public TapGestureRecognizer GetTapGestureRecognizer()
        {
            return StackLayoutViewSiteTapGesture;
        }
    }
}