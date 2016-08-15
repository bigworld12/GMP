using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace GMP.Extentions
{
    /// <summary>
	/// A Slider which provides a way to modify the 
	/// auto tooltip text by using a format string.
	/// </summary>
	public class FormattedSlider : Slider
    {
        private ToolTip _autoToolTip;
        

        /// <summary>
        /// Gets/sets a converter used to modify the auto tooltip's content.       
        /// </summary>
        public IValueConverter AutoToolTipTextConverter
        {
            get { return (IValueConverter)GetValue(AutoToolTipTextConverterProperty); }
            set { SetValue(AutoToolTipTextConverterProperty , value); }
        }

        // Using a DependencyProperty as the backing store for AutoToolTipTextConverter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoToolTipTextConverterProperty =
            DependencyProperty.Register("AutoToolTipTextConverter" , typeof(IValueConverter) , typeof(FormattedSlider) , new PropertyMetadata(null));



        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
            this.FormatAutoToolTipContent();
        }

        protected override void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            base.OnThumbDragDelta(e);
            this.FormatAutoToolTipContent();
        }

        private void FormatAutoToolTipContent()
        {
            if (AutoToolTipTextConverter != null)
            {
                AutoToolTip.Content = AutoToolTipTextConverter.Convert(AutoToolTip.Content,null,null,null);
            }
        }

        private ToolTip AutoToolTip
        {
            get
            {
                if (_autoToolTip == null)
                {
                    
                    FieldInfo field = typeof(Slider).GetField(
                        "_autoToolTip" ,
                        BindingFlags.NonPublic | BindingFlags.Instance);

                    _autoToolTip = field.GetValue(this) as ToolTip;
                }

                return _autoToolTip;
            }
        }
    }
}
