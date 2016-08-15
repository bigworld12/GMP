using GMP.Classes;
using MahApps.Metro.Controls;
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
using NHotkey.Wpf;
using NHotkey;
using System.ComponentModel;

namespace GMP.FlyOuts
{
    public class CommunicationObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OPC(string propname)
        {
            PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propname));
        }

        private MusicObject m_MusicInstance;

        public MusicObject MusicInstance
        {
            get { return m_MusicInstance; }
            set { m_MusicInstance = value; OPC(nameof(MusicInstance)); }
        }


        private MainWindow m_MainWindowInstance;

        public MainWindow MainWindowInstance
        {
            get { return m_MainWindowInstance; }
            set { m_MainWindowInstance = value; OPC(nameof(MainWindowInstance)); }
        }

    }

    /// <summary>
    /// Interaction logic for HKSettings.xaml
    /// </summary>
    public partial class HKSettings : UserControl
    {


        public CommunicationObject CommunicationObj
        {
            get { return (CommunicationObject)GetValue(CommunicationObjProperty); }
            set { SetValue(CommunicationObjProperty , value);
                App.CommunicationObj = value;
            }
        }

        // Using a DependencyProperty as the backing store for CommunicationObj.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommunicationObjProperty =
            DependencyProperty.Register("CommunicationObj" , typeof(CommunicationObject) , typeof(HKSettings) , new PropertyMetadata(null));



        public HKSettings()
        {
            InitializeComponent();
        }
    }
}
