using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMP.Enums
{
   
    public enum RepeatModes
    {
        None,
        SingleSong,
        SingleList,
        AllLists
    }
    
    public enum PlayBackStatuses
    {        
        Playing = 2,
        Paused = 1,
        Stopped = 0,
        Failed = -1
    }
}
