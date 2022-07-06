using Rg.Plugins.Popup.Pages;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PopupLoading : PopupPage
    {
        public PopupLoading(string label = null, bool closeWhenBackgroundIsClicked = false)
        {
            InitializeComponent();
            if (label != null)
                LoadingLabel.Text = label;
            CloseWhenBackgroundIsClicked = closeWhenBackgroundIsClicked;
        }


    }
}