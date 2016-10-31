using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static GMP.Extentions.Extentions;

namespace GMP
{
    /// <summary>
    /// Interaction logic for MusicSlider.xaml
    /// </summary>
    public partial class MusicSlider : UserControl
    {

        #region Properties
        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty , value); }
        }
        // Using a DependencyProperty as the backing store for Minimum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum" , typeof(double) , typeof(MusicSlider) , new PropertyMetadata(0d));


        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty , value); }
        }
        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum" , typeof(double) , typeof(MusicSlider) , new PropertyMetadata(100d));


        public double StartValue
        {
            get { return (double)GetValue(StartValueProperty); }
            set { SetValue(StartValueProperty , value); }
        }
        // Using a DependencyProperty as the backing store for StartValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartValueProperty =
            DependencyProperty.Register("StartValue" , typeof(double) , typeof(MusicSlider) , new PropertyMetadata(0d));




        public double EndValue
        {
            get { return (double)GetValue(EndValueProperty); }
            set { SetValue(EndValueProperty , value); }
        }
        // Using a DependencyProperty as the backing store for EndValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndValueProperty =
            DependencyProperty.Register("EndValue" , typeof(double) , typeof(MusicSlider) , new PropertyMetadata(100d));



        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty , value); }
        }
        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value" , typeof(double) , typeof(MusicSlider) , new PropertyMetadata(0d , ValueChanged));

        private static void ValueChanged(DependencyObject d , DependencyPropertyChangedEventArgs e)
        {
            var newval = (double)e.NewValue;
            var oldval = (double)e.OldValue;
            if (oldval == newval) return;
            var slider = (MusicSlider)d;
            if (slider != null)
            {
             

                Ellipse PART_Ellipse = slider.FindChild<Ellipse>("PART_ELLIPSE");               

                var oldthickness = new Thickness(slider.RelativeToSlider(oldval) - (PART_Ellipse.ActualWidth / 2) , 0 , 0 , 0);                
                var newthickness = new Thickness(slider.RelativeToSlider(newval) - (PART_Ellipse.ActualWidth / 2) , 0 , 0 , 0);

                var updatesotryboard = (Storyboard)slider.TryFindResource("EllipseUpdateStoryBoard");                
                var ThicknessAnimation = (ThicknessAnimationUsingKeyFrames)updatesotryboard.Children[0];
                var FromTimeLine = (SplineThicknessKeyFrame)ThicknessAnimation.KeyFrames[0];
                var ToTimeLine = (SplineThicknessKeyFrame)ThicknessAnimation.KeyFrames[1];
                

                FromTimeLine.Value = oldthickness;
                ToTimeLine.Value = newthickness;
              
                Storyboard.SetTarget(updatesotryboard , PART_Ellipse);
                updatesotryboard.Begin();
            }

        }

        /// <summary>
        /// returns a value relative to the slider
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public double RelativeToSlider(double value)
        {
            if (Maximum == 0) return ActualWidth;
            if (value >= Maximum) return ActualWidth;
            if (value <= Minimum) return 0;
            return ((value - Minimum )* ActualWidth) / (Maximum- Minimum);
        }

        #endregion


        #region Styles
        public Style StartRectangleStyle
        {
            get { return (Style)GetValue(StartRectangleStyleProperty); }
            set { SetValue(StartRectangleStyleProperty , value); }
        }
        // Using a DependencyProperty as the backing store for StartRectangleStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartRectangleStyleProperty =
            DependencyProperty.Register("StartRectangleStyle" , typeof(Style) , typeof(MusicSlider) , new PropertyMetadata(default(Style)));




        public Style EndRectangleStyle
        {
            get { return (Style)GetValue(EndRectangleStyleProperty); }
            set { SetValue(EndRectangleStyleProperty , value); }
        }
        // Using a DependencyProperty as the backing store for EndRectangleStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndRectangleStyleProperty =
            DependencyProperty.Register("EndRectangleStyle" , typeof(Style) , typeof(MusicSlider) , new PropertyMetadata(default(Style)));





        public Style CurrentEllipseStyle
        {
            get { return (Style)GetValue(CurrentEllipseStyleProperty); }
            set { SetValue(CurrentEllipseStyleProperty , value); }
        }

        // Using a DependencyProperty as the backing store for CurrentEllipseStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentEllipseStyleProperty =
            DependencyProperty.Register("CurrentEllipseStyle" , typeof(Style) , typeof(MusicSlider) , new PropertyMetadata(default(Style)));





        public Style PlayableTrackRectangleStyle
        {
            get { return (Style)GetValue(PlayableTrackRectangleStyleProperty); }
            set { SetValue(PlayableTrackRectangleStyleProperty , value); }
        }
        // Using a DependencyProperty as the backing store for PlayableTrackRectangleStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlayableTrackRectangleStyleProperty =
            DependencyProperty.Register("PlayableTrackRectangleStyle" , typeof(Style) , typeof(MusicSlider) , new PropertyMetadata(default(Style)));




        public Style CurrentTrackRectangleStyle
        {
            get { return (Style)GetValue(CurrentTrackRectangleStyleProperty); }
            set { SetValue(CurrentTrackRectangleStyleProperty , value); }
        }
        // Using a DependencyProperty as the backing store for CurrentTrackRectangleStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentTrackRectangleStyleProperty =
            DependencyProperty.Register("CurrentTrackRectangleStyle" , typeof(Style) , typeof(MusicSlider) , new PropertyMetadata(default(Style)));




        public Style BaseBorderStyle
        {
            get { return (Style)GetValue(BaseBorderStyleProperty); }
            set { SetValue(BaseBorderStyleProperty , value); }
        }
        // Using a DependencyProperty as the backing store for BaseRectangleStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BaseBorderStyleProperty =
            DependencyProperty.Register("BaseBorderStyle" , typeof(Style) , typeof(MusicSlider) , new PropertyMetadata(default(Style)));




        #endregion




        public MusicSlider()
        {
            InitializeComponent();

            DataContext = this;

            var def = "Default";
            BaseBorderStyle = (Style)TryFindResource(def + nameof(BaseBorderStyle));
            StartRectangleStyle = (Style)TryFindResource(def + nameof(StartRectangleStyle));
            EndRectangleStyle = (Style)TryFindResource(def + nameof(EndRectangleStyle));
            CurrentEllipseStyle = (Style)TryFindResource(def + nameof(CurrentEllipseStyle));
            PlayableTrackRectangleStyle = (Style)TryFindResource(def + nameof(PlayableTrackRectangleStyle));
            CurrentTrackRectangleStyle = (Style)TryFindResource(def + nameof(CurrentTrackRectangleStyle));
        }
    }
}
