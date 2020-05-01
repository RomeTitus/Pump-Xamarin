using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace EmbeddedImages
{
    [ContentProperty (nameof(source))]
    class ImageResourceExtention : IMarkupExtension
    {
        public string source { get; set; }
        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (source == null)
                return null;
            var imageSource = ImageSource.FromResource(source, typeof(ImageResourceExtention).GetTypeInfo().Assembly);
            return imageSource;
        }
    }
}
