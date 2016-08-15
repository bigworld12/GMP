using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GMP.Extentions
{
    public static class ListBoxHelper
    {

        [Category("Helpers")]
        [AttachedPropertyBrowsableForType(typeof(ListBox))]
        public static bool GetIsMultiSelected(UIElement obj)
        {
            return (bool)obj.GetValue(IsMultiSelectedProperty);
        }

        public static void SetIsMultiSelected(UIElement obj , bool value)
        {
            obj.SetValue(IsMultiSelectedProperty , value);
        }

        // Using a DependencyProperty as the backing store for IsMultiSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMultiSelectedProperty =
            DependencyProperty.RegisterAttached("IsMultiSelected" , typeof(bool) , typeof(ListBoxHelper) , new UIPropertyMetadata(false, IsMultiSelectedChanged));

        private static void IsMultiSelectedChanged(DependencyObject d , DependencyPropertyChangedEventArgs e)
        {                        
            
        }

      
    }
}
