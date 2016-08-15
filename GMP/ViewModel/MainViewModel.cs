using GMP.Classes;
using GMP.Extentions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace GMP.ViewModel
{

    public class MainViewModel : INotifyPropertyChanged
    {
        private MainWindow _MainWindowInstance;
        private MusicObject m_Music;
        public MusicObject Music
        {
            get { return m_Music; }
            set
            {
                m_Music = value;
                OPC(nameof(Music));
            }
        }








        public IEnumerable<Song> UserSelectedSongs
        {
            get
            {
                return _MainWindowInstance.SongsViewer.SelectedItems.Cast<Song>();
            }
        }
        public PlayList UserSelectedPlayList
        {
            get
            {
                if (_MainWindowInstance.PlaylistsRepresenter.SelectedItem == null)
                {
                    return (PlayList)_MainWindowInstance.PlaylistsRepresenter.Items[0];
                }
                return (PlayList)_MainWindowInstance.PlaylistsRepresenter.SelectedItem;
            }
            set
            {
                _MainWindowInstance.PlaylistsRepresenter.SelectedItem = value;
            }
        }

        private bool m_IsUserEditing;
        public bool IsUserEditing
        {
            get { return m_IsUserEditing; }
            set
            {
                m_IsUserEditing = value;
                OPC(nameof(IsUserEditing));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;




        public Visibility MultiSongsVisiblity
        {
            get
            {
                if ((bool)_MainWindowInstance.SongsViewer.GetValue(ListBoxHelper.IsMultiSelectedProperty))
                {
                    //has multiple items
                    return Visibility.Collapsed;
                }
                else
                {
                    //has only one item
                    return Visibility.Visible;
                }

            }

        }
        public IEnumerable<PlayList> GetMoveToPlaylists
        {
            get
            {
                return Music.PlayLists.Where(x => !ReferenceEquals(x , UserSelectedPlayList));
            }

        }


        public void OPC(string propname)
        {
            PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propname));
        }


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            Music = new MusicObject();

        }
        public void SetMainWindInstance(MainWindow inst)
        {
            _MainWindowInstance = inst;
        }
    }
}