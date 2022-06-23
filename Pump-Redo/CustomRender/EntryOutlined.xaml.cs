using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.CustomRender
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EntryOutlined : ContentView
    {
        public EntryOutlined()
        {
            InitializeComponent();
        }

        public static readonly BindableProperty TextProperty =
            BindableProperty.Create(nameof(Text), typeof(string), typeof(EntryOutlined));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static readonly BindableProperty PlaceholderProperty =
            BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(EntryOutlined));

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value);}
        }

        public static readonly BindableProperty PlaceholderColorProperty =
            BindableProperty.Create(nameof(PlaceholderColor), typeof(Color), typeof(EntryOutlined), Color.Blue);

        public Color PlaceholderColor
        {
            get { return (Color)GetValue(PlaceholderColorProperty); }
            set { SetValue(PlaceholderColorProperty, value); }
        }

        public static readonly BindableProperty BorderColorProperty =
            BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(EntryOutlined), Color.Blue);

        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }
        
        public async void TextBox_Focused(object sender, FocusEventArgs e)
        {
            await TranslateLabelToTitle();
        }

        public async void TextBox_Unfocused(object sender, FocusEventArgs e)
        {
            await TranslateLabelToPlaceHolder();
        }

        public async Task TranslateLabelToTitle(bool noAnimation = false)
        {
            if (string.IsNullOrEmpty(this.Text))
            {
                var distance = GetPlaceholderDistance(PlaceHolderLabel);
                if(noAnimation)
                    await PlaceHolderLabel.TranslateTo(0, -distance, 0);
                else
                    await PlaceHolderLabel.TranslateTo(0, -distance);
                
            }
            
        }

        async Task TranslateLabelToPlaceHolder()
        {
            if(string.IsNullOrEmpty(Text))
            {
                await PlaceHolderLabel.TranslateTo(0, 0);                
            }
        }

        double GetPlaceholderDistance(Label control)
        {
            double distance;
            if(Device.RuntimePlatform == Device.iOS) distance = 0;
            else distance = 5;
            
            distance = control.Height + distance;
            return distance;
        }
        
        public event EventHandler<TextChangedEventArgs> TextChanged;

        protected virtual void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextChanged?.Invoke(this, e);
        }

    }
}