using Android.Content;
using Pump.CustomRender;
using Pump.Droid.CustomRender;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(BorderlessEntry), typeof(BorderlessEntryRenderer))]
namespace Pump.Droid.CustomRender
{
    public class BorderlessEntryRenderer : EntryRenderer
    {
        public BorderlessEntryRenderer(Context context) : base(context)
        {
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            //Configure native control (TextBox)
            if(Control != null)
            {
                Control.Background = null;
            }

            // Configure Entry properties
            if(e.NewElement != null)
            {

            }
        }
    }
}