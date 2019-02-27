using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace IGrill.App.Areas
{
    public static class TextIndexer
    {
        public static bool GetUseUpperCase(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseUpperCaseProperty);
        }
        public static void SetUseUpperCase(DependencyObject obj, bool value)
        {
            obj.SetValue(UseUpperCaseProperty, value);
        }
        public static readonly DependencyProperty UseUpperCaseProperty =
            DependencyProperty.RegisterAttached("UseUpperCase", typeof(bool), typeof(TextBlock), new PropertyMetadata(false, (sender, args) =>
            {
                var textBlock = (TextBlock)sender;
                textBlock.RegisterPropertyChangedCallback(TextBlock.TextProperty, (s, e) =>
                {
                    textBlock.Text = textBlock.Text.ToUpper();
                });
            }));
    }
}
