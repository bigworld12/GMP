using GMP.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GMP.Extentions.Extentions;

namespace GMP.Classes
{
    public class PlayList : ObservableCollection<Song>, INotifyPropertyChanged
    {

        private MusicObject m_MusicInstance;

        public MusicObject MusicInstance
        {
            get { return m_MusicInstance; }
            set
            {
                m_MusicInstance = value;
                OPC(nameof(MusicInstance));
            }
        }

        public PlayList(MusicObject instance)
        {
            MusicInstance = instance;
            CollectionChanged += PlayList_CollectionChanged;
        }

        private void PlayList_CollectionChanged(object sender , System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OPC(nameof(ReadableRepresentation));
        }

        public Song CurrentSong
        {
            get { return MusicInstance.Player.CurrentSong; }
            set
            {
                MusicInstance.Player.CurrentSong = value;
                OPC(nameof(CurrentSong));
            }
        }


        private string m_Name;
        public string Name
        {
            get { return m_Name; }
            set
            {
                m_Name = value;
                OPC(nameof(Name));
                OPC(nameof(ReadableRepresentation));
            }
        }

        public string ReadableRepresentation
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    return $"{TrimExtraThan(Name , 40)} ({Count})";
                }
                else
                {
                    return $"(Un-named) ({Count})";
                }

            }
        }

        private bool m_IsCurrent;
        public bool IsCurrent
        {
            get { return m_IsCurrent; }
            set { m_IsCurrent = value; OPC(nameof(IsCurrent)); }
        }


        private bool m_isfav;
        public bool IsFav
        {
            get { return m_isfav; }
            set { m_isfav = value; OPC(nameof(IsFav)); }
        }
        /// <summary>
        /// moves to the next song within this playlist
        /// </summary>
        public void MoveNext()
        {
            if (Count > 0)
            {
                if (CurrentSong != null)
                {
                    if (IndexOf(CurrentSong) == Count - 1)
                    {
                        CurrentSong = this[0];
                    }
                    else
                    {
                        CurrentSong = this[IndexOf(CurrentSong) + 1];
                    }
                }
                else
                {
                    //set song to first in palylist
                    CurrentSong = this[0];
                }
            }
        }

        public void MovePrevious()
        {
            if (Count > 0)
            {
                if (CurrentSong != null)
                {
                    if (IndexOf(CurrentSong) == 0)
                    {
                        CurrentSong = this.Last();
                    }
                    else
                    {
                        CurrentSong = this[IndexOf(CurrentSong) - 1];
                    }
                }
                else
                {
                    //set song to first in palylist
                    CurrentSong = this.Last();
                }
            }
        }

        public void OPC(string propname)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propname));
        }
    }
}
