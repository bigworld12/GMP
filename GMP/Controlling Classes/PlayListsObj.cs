using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using GMP.FlyOuts;

namespace GMP.Classes
{
    public class PlayListsObj : ObservableCollection<PlayList>
    {
        public bool IsGlobalNoSongs
        {
            get
            {
                return this.Count == 0 && this.All(x => x.Count == 0);
            }
        }

        public PlayListsObj(MusicObject instance)
        {
            MusicInstance = instance;
            var x = new PlayList(instance);
            Add(x);
        }

        public new void Add(PlayList item)
        {
            base.Add(item);
            if (Count == 2 && (this[0].Count == 0 && string.IsNullOrWhiteSpace(this[0].Name)))
            {
                RemoveAt(0);
                ((MainWindow)App.Current.MainWindow).PlaylistsRepresenter.SelectedItem = this[0];
            }

        }
        public new void Remove(PlayList item)
        {
            base.Remove(item);
            if (Count == 0)
            {
                var x = new PlayList(MusicInstance);
                Add(x);
            }
        }
        public new void RemoveAt(int indx)
        {
            base.RemoveAt(indx);
            if (Count == 0)
            {
                var x = new PlayList(MusicInstance);
                Add(x);
            }
        }
        private MusicObject m_MusicInstance;

        public MusicObject MusicInstance
        {
            get { return m_MusicInstance; }
            set { m_MusicInstance = value; }
        }

        public PlayList CurrentPlayList
        {
            get { return MusicInstance.Player.CurrentPlayList; }
            set
            {
                MusicInstance.Player.CurrentPlayList = value;
                OPC(nameof(CurrentPlayList));
            }
        }

        public void OPC(string propname)
        {
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(propname));
        }
        string nosongsfoundmessage = "There are no songs,please consider adding some";


        /// <summary>
        /// changes only current playlist to the next one that has songs
        /// </summary>
        public void MoveNext()
        {
            if (IsGlobalNoSongs)
            {
                LogFlyOut.SendLog(null , nosongsfoundmessage , LogFlyOut.LogTypes.Warning);
                return;
            }

            if (CurrentPlayList != null)
            {
                if (IndexOf(CurrentPlayList) == Count - 1)
                {
                    CurrentPlayList = this[0];

                }
                else
                {
                    CurrentPlayList = this[IndexOf(CurrentPlayList) + 1];
                }
            }
            else
            {
                CurrentPlayList = this[0];
            }

            if (CurrentPlayList.Count == 0)
            {
                LogFlyOut.SendLog(null , "Reached an empty playlist , moving on" , LogFlyOut.LogTypes.Warning);
                MoveNext();
            }
            else
            {
                CurrentPlayList.CurrentSong = CurrentPlayList[0];
            }

        }

        /// <summary>
        /// changes only current playlist to the previous one that has songs
        /// </summary>
        public void MovePrevious()
        {
            if (IsGlobalNoSongs)
            {
                LogFlyOut.SendLog(null , nosongsfoundmessage , LogFlyOut.LogTypes.Warning);
                return;
            }

            if (CurrentPlayList != null)
            {
                if (IndexOf(CurrentPlayList) == 0)
                {
                    CurrentPlayList = this.Last();

                }
                else
                {
                    CurrentPlayList = this[IndexOf(CurrentPlayList) - 1];
                }
            }
            else
            {
                CurrentPlayList = this.Last();
            }

            if (CurrentPlayList.Count == 0)
            {
                LogFlyOut.SendLog(null , "Reached an empty playlist , moving on" , LogFlyOut.LogTypes.Warning);
                MovePrevious();
            }
            else
            {
                CurrentPlayList.CurrentSong = CurrentPlayList.Last();
            }
        }

        /// <summary>
        /// changes current song to the next one across all playlists
        /// </summary>
        public void GeneralMoveNext()
        {
            if (IsGlobalNoSongs)
            {
                LogFlyOut.SendLog(null , nosongsfoundmessage , LogFlyOut.LogTypes.Warning);
                return;
            }
            if (CurrentPlayList != null)
            {
                if (CurrentPlayList.CurrentSong != null)
                {
                    //check if it will move to next play list or next song
                    if (CurrentPlayList.IndexOf(CurrentPlayList.CurrentSong) == CurrentPlayList.Count - 1)
                    {
                        //move to next playlist
                        MoveNext();
                    }
                    else
                    {
                        //move to next song
                        CurrentPlayList.MoveNext();
                    }
                }
                else
                {
                    //move to next song within this playlist
                    CurrentPlayList.MoveNext();
                }
            }
            else
            {
                //move to the first playlist
                MoveNext();
            }

        }

        /// <summary>
        /// changes current song to the previous one across all playlists
        /// </summary>
        public void GeneralMovePrevious()
        {
            if (IsGlobalNoSongs)
            {
                LogFlyOut.SendLog(null , nosongsfoundmessage , LogFlyOut.LogTypes.Warning);
                return;
            }
            if (CurrentPlayList != null)
            {
                if (CurrentPlayList.CurrentSong != null)
                {
                    //check if it will move to previous play list or previous song
                    if (CurrentPlayList.IndexOf(CurrentPlayList.CurrentSong) == 0)
                    {
                        //move to previous playlist
                        MovePrevious();
                    }
                    else
                    {
                        //move to previous song
                        CurrentPlayList.MovePrevious();
                    }
                }
                else
                {
                    //move to previous song within this playlist
                    CurrentPlayList.MovePrevious();
                }
            }
            else
            {
                //move to the first playlist
                MovePrevious();
            }
        }


    }
}
