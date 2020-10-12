using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FloatingScreenScroll : PopupPage
    {
        public bool IsStackLayout = true;
        public FloatingScreenScroll()
        {
            InitializeComponent();
        }

        public void SetFloatingScreen(IEnumerable<object> screens)
        {
            if(IsStackLayout)
                foreach (View screen in screens) ScrollViewFloatingPage.Children.Add(screen);
            else
            {
                var flexLayout = new FlexLayout
                {
                    Wrap = FlexWrap.Wrap,
                    JustifyContent = FlexJustify.SpaceBetween
                };
                foreach (View screen in screens) flexLayout.Children.Add(screen);
                ScrollViewFloatingPage.Children.Add(flexLayout);
            }
        }
    }
}