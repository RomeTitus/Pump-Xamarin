using System;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EmbeddedImages
{
    [ContentProperty(nameof(source))]
    internal class ImageResourceExtension : IMarkupExtension
    {
        public string source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (source == null)
                return null;
            var imageSource = ImageSource.FromResource(source, typeof(ImageResourceExtension).GetTypeInfo().Assembly);
            return imageSource;
        }
    }
}