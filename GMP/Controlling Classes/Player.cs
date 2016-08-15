using GMP.Classes;
using GMP.Enums;
using GMP.FlyOuts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GMP.Player
{
    public class CustomPlayer : MediaPlayer, INotifyPropertyChanged
    {

        public CustomPlayer(MusicObject AssignedDataObjParam)
        {

            MusicInstance = AssignedDataObjParam;
            MediaOpened += OnMediaOpened;
            MediaFailed += OnMediaFailed;
            MediaEnded += OnMediaEnded;

        }

        private void OnMediaEnded(object sender , EventArgs e)
        {

            switch (MusicInstance.RepeatMode)
            {
                case RepeatModes.None:
                    Stop();
                    break;
                case RepeatModes.SingleSong:
                    Stop();
                    Play();
                    break;
                case RepeatModes.SingleList:
                    Stop();
                    MusicInstance.Player.CurrentPlayList.MoveNext();
                    Open(MusicInstance.Player.CurrentPlayList.CurrentSong , MusicInstance.Player.CurrentPlayList);
                    break;
                case RepeatModes.AllLists:
                    Stop();
                    MusicInstance.PlayLists.GeneralMoveNext();
                    Open(MusicInstance.Player.CurrentPlayList.CurrentSong , MusicInstance.Player.CurrentPlayList);
                    break;
                default:
                    break;
            }
        }

        private void OnMediaOpened(object sender , EventArgs e)
        {
            if (CurrentSong.MaxDuration == 0) CurrentSong.MaxDuration = NaturalDuration.TimeSpan.TotalMilliseconds;

            Volume = oldvol;
            if (CurrentSong != null && CurrentPlayList != null)
            {
                base.Play();
                MusicInstance.PlayBackStatus = PlayBackStatuses.Playing;
            }
        }

        private void OnMediaFailed(object sender , ExceptionEventArgs e)
        {
            LogFlyOut.SendLog(null , $"Player failed to open : {CurrentSong.FullPath} , maybe it isn't a supported type, removing it from the list\nException :\n{e.ErrorException.ToString()}" , LogFlyOut.LogTypes.Error);
            CurrentPlayList.Remove(CurrentSong);
            CurrentSong = null;
            CurrentPlayList = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OPC(string propname)
        {
            PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propname));
        }
        public double SPosition
        {
            get
            {
                return Position.TotalMilliseconds;
            }
            set
            {
                Position = TimeSpan.FromMilliseconds(value);
                OPC(nameof(SPosition));
            }
        }

        private MusicObject m_AssignedDataObj;

        public MusicObject MusicInstance
        {
            get { return m_AssignedDataObj; }
            set { m_AssignedDataObj = value; OPC(nameof(MusicInstance)); }
        }

        private Song m_currentSong;
        public Song CurrentSong
        {
            get { return m_currentSong; }
            set
            {
                if (m_currentSong == value) return;
                if (m_currentSong != null) m_currentSong.IsCurrent = false;
                m_currentSong = value;
                if (value != null) value.IsCurrent = true;
                OPC(nameof(CurrentSong));
            }
        }


        private PlayList m_currentplaylist;
        public PlayList CurrentPlayList
        {
            get { return m_currentplaylist; }
            set
            {
                if (m_currentplaylist == value) return;
                if (m_currentplaylist != null) m_currentplaylist.IsCurrent = false;
                m_currentplaylist = value;
                if (value != null) value.IsCurrent = true;
                OPC(nameof(CurrentPlayList));
            }
        }


        public new double Volume
        {
            get
            {

                return base.Volume;
            }
            set
            {
                base.Volume = value;
                OPC(nameof(Volume));
            }
        }
        double oldvol;
        public void Open(Song msc , PlayList pl)
        {
            oldvol = Volume;
            if (CurrentSong != null)
            {
                Stop();
                CurrentSong.IsCurrent = false;
                CurrentPlayList.IsCurrent = false;
            }
            Close();

            CurrentPlayList = pl;
            CurrentSong = msc;
            if (msc != null)
            {
                msc.IsCurrent = true;
                pl.IsCurrent = true;
                Open(new Uri(msc.FullPath));
            }
        }
        public new void Play()
        {
            if (CurrentSong == null)
            {
                if (CurrentPlayList == null)
                {
                    if (MusicInstance.PlayLists.Count > 0)
                    {
                        var firstplist = MusicInstance.PlayLists[0];
                        if (firstplist.Count > 0)
                        {
                            Open(firstplist[0] , firstplist);
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }

                }
                else
                {
                    if (CurrentPlayList.Count > 0)
                    {
                        Open(CurrentPlayList[0] , CurrentPlayList);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else
            {
                base.Play();
                MusicInstance.PlayBackStatus = PlayBackStatuses.Playing;
            }
        }
        public new void Pause()
        {
            if (CanPause) base.Pause();
            MusicInstance.PlayBackStatus = PlayBackStatuses.Paused;
        }

        public new void Stop()
        {
            base.Stop();
            MusicInstance.PlayBackStatus = PlayBackStatuses.Stopped;
        }


    }
}
