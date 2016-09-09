using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GMP.FlyOuts
{
    /// <summary>
    /// Interaction logic for LogFlyOut.xaml
    /// </summary>
    public partial class LogFlyOut : UserControl
    {
        public static LogFlyOut Instance { get; set; }

        public LogFlyOut()
        {
            InitializeComponent();
            Blocks = LogDocument.Blocks;
        }
        public enum LogTypes { Error, Normal, Warning }

        private static BlockCollection Blocks;

        public static void SendLog(DateTime? time , string message , LogTypes level)
        {
            Action action = () =>
            {
                if (!time.HasValue)
                    time = DateTime.Now;
                Run DateText = new Run($"{time?.ToShortTimeString()} : ");
                DateText.Foreground = new SolidColorBrush(Colors.LightGreen);

                SolidColorBrush finalcolor;

                switch (level)
                {
                    case LogTypes.Error:
                        finalcolor = new SolidColorBrush(Colors.Red);
                        App.CommunicationObj.MainWindowInstance.LogFly.IsOpen = true;
                        break;
                    case LogTypes.Normal:
                        finalcolor = new SolidColorBrush(Colors.White);
                        break;
                    case LogTypes.Warning:
                        finalcolor = new SolidColorBrush(Colors.Yellow);
                        break;
                    default:
                        finalcolor = new SolidColorBrush(Colors.White);
                        break;
                }
                //normal color



                Run MessageText = new Run(message);
                MessageText.Foreground = finalcolor;


                Paragraph NewParagraph = new Paragraph();
                NewParagraph.Inlines.Add(DateText);
                NewParagraph.Inlines.Add(MessageText);
                Blocks.Add(NewParagraph);
            };

            try
            {
                if (Application.Current.Dispatcher.CheckAccess() == false) { Application.Current.Dispatcher.Invoke(action); }
                else { action(); }
            }
            catch { }
        }

        private void ClearButton_Click(object sender , RoutedEventArgs e)
        {
            Blocks?.Clear();
        }
    }
}
