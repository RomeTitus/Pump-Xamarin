using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FloatingScreen : PopupPage
    {
        public FloatingScreen()
        {
            InitializeComponent();
        }

        public void SetFloatingScreen(IEnumerable<object> screens)
        {
            ViewFloatingPage.Children.Clear();
            foreach (View screen in screens) ViewFloatingPage.Children.Add(screen);
        }
    }
}