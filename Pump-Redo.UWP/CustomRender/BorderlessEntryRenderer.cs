using Pump.CustomRender;
using Pump_Redo.UWP.CustomRender;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(BorderlessEntry), typeof(BorderlessEntryRenderer))]

namespace Pump_Redo.UWP.CustomRender
{
    public class BorderlessEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            //Configure native control (TextBox)
            if (Control != null) Control.Background = null;

            // Configure Entry properties
            if (e.NewElement != null)
            {
            }
        }
    }
}