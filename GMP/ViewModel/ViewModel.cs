using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMP.ViewModel
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OPC(string propname)
        {
            var dispatcher = System.Windows.Application.Current.Dispatcher;
            dispatcher.Invoke(() => {
                PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propname));
            });
            
        }
    }
}
