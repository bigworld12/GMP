using GMP.Enums;
using GMP.FlyOuts;
using GMP.Player;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GMP.Classes
{
    public class MusicObject : INotifyPropertyChanged
    {

        public Visibility PlayButtonVisibility
        {
            get
            {
                if (PlayBackStatus == PlayBackStatuses.Paused || PlayBackStatus == PlayBackStatuses.Stopped)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }
        }
        public Visibility PauseButtonVisibility
        {
            get
            {
                if (PlayBackStatus == PlayBackStatuses.Playing)
                    return Visibility.Visible;
                else
                    return Visibility.Hidden;
            }
        }

        public MusicObject()
        {
            Player = new CustomPlayer(this);
            PlayLists = new PlayListsObj(this);
            PlayBackStatus = PlayBackStatuses.Stopped;
            RepeatMode = RepeatModes.None;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OPC(string propname)
        {
            PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propname));
        }

        private CustomPlayer m_player;
        public CustomPlayer Player
        {
            get { return m_player; }
            set
            {
                m_player = value;
                OPC(nameof(Player));
            }
        }


        private RepeatModes m_repeatMode;
        public RepeatModes RepeatMode
        {
            get { return m_repeatMode; }
            set
            {
                m_repeatMode = value;
                OPC(nameof(RepeatMode));
            }
        }


        private PlayBackStatuses m_PlaybackStatus;
        public PlayBackStatuses PlayBackStatus
        {
            get { return m_PlaybackStatus; }
            set
            {
                m_PlaybackStatus = value;
                OPC(nameof(PlayBackStatus));
                OPC(nameof(PlayButtonVisibility));
                OPC(nameof(PauseButtonVisibility));
            }
        }

        public string ReadableRepeatMode
        {
            get
            {
                switch (RepeatMode)
                {
                    case RepeatModes.None:
                        return "No Repeat";
                    case RepeatModes.SingleSong:
                        return "Single Song";
                    case RepeatModes.SingleList:
                        return "Single List";
                    case RepeatModes.AllLists:
                        return "All Lists";
                    default:
                        return "No Repeat";
                }
            }
        }

        private PlayListsObj m_PlayLists;
        public PlayListsObj PlayLists
        {
            get { return m_PlayLists; }
            set { m_PlayLists = value; }
        }
    }
}
