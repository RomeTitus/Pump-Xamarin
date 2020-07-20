using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FloatingScreenScroll : PopupPage
    {
        public FloatingScreenScroll()
        {
            InitializeComponent();
        }

        public void setFloatingScreen(List<object> screens)
        {
            foreach (View screen in screens) ScrollViewFloatingPage.Children.Add(screen);
        }
    }
}