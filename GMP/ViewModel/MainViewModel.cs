using GMP.Classes;
using GMP.Extentions;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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



        /// <summary>
        /// adds a folder to playlists in where each folder represents a playlist
        /// </summary>
        /// <param name="GrandBaseDirectory">the directory user selected</param>
        public void AddDirectory(string GrandBaseDirectory , IninstCounter count)
        {
            Task.Run(() =>
            {
                //F:\Music > Music
                /*
                 F:\Music\1.5K Surprise > Music - 1.5K Surprise
                 F:\Music\1.5K Surprise\SONGS > Music - 1.5K Surprise - SONGS
                 */
                DirectoryInfo dirct = Directory.CreateDirectory(GrandBaseDirectory);

                //add to a new playlist
                IEnumerable<FileInfo> filepaths = GetValidSongs(dirct);



                App.Current.Dispatcher.InvokeAsync(() =>
                {
                    var init_pl = new PlayList(Music);
                    init_pl.Name = dirct.Name;
                    foreach (var item in filepaths)
                    {
                        init_pl.Add(new Song(item.FullName , Music));
                    }
                    Music.PlayLists.Add(init_pl);
                    count.Count += init_pl.Count;
                });
                var subfolders = dirct.GetDirectories();
                foreach (DirectoryInfo item in subfolders)
                {
                    ProcessAddSubDirectory(dirct , item , count);
                }
            });

        }
        public IEnumerable<FileInfo> GetValidSongs(DirectoryInfo folder)
        {
            try
            {

                return folder.GetFiles().Where(fx => IsSongValid(fx));
            }
            catch
            {
                return new List<FileInfo>();
            }

        }
        private void ProcessAddSubDirectory(DirectoryInfo GrandBaseDirectory , DirectoryInfo SubFolder , IninstCounter count)
        {
            var validsongs = GetValidSongs(SubFolder);
            if (validsongs.Count() > 0)
            {

                var parent = GrandBaseDirectory.Parent;

                string wantedname = string.Empty;
                if (parent != null)
                {
                    wantedname = SubFolder.FullName.Replace(parent.FullName.Replace(@"\\" , @"\") , string.Empty).TrimStart('\\').Replace(@"\" , @" - ");
                }
                else
                {
                    wantedname = SubFolder.FullName.Replace(GrandBaseDirectory.FullName.Replace(@"\\" , @"\") , string.Empty).TrimStart('\\').Replace(@"\" , @" - ");
                }
                App.Current.Dispatcher.InvokeAsync(() =>
                {
                    var pl = new PlayList(Music);
                    pl.Name = wantedname;
                    foreach (var item in validsongs)
                    {
                        pl.Add(new Song(item.FullName , Music));
                    }
                    Music.PlayLists.Add(pl);
                    count.Count += pl.Count;
                });

            }
            //get other sub directories
            try
            {
                var subs = SubFolder.GetDirectories();
                foreach (var item in subs)
                {
                    ProcessAddSubDirectory(GrandBaseDirectory , item , count);
                }
            }
            catch { }
        }

        public static bool IsSongValid(string path)
        {
            try
            {
                var file = new FileInfo(path);
                if (file.Exists)
                {
                    var ext = file.Extension.Replace("." , string.Empty).ToLower();
                    return Audioextensions.Contains(ext);
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }

        }
        public static string[] Audioextensions = { "mp3" , "m4a" , "wma" , "wav" , "w64" , "ogg" , "flac" };
      
        public static bool IsSongValid(FileInfo fi)
        {
            try
            {
                if (fi.Exists)
                {
                    var ext = fi.Extension.Replace("." , string.Empty).ToLower();
                    return Audioextensions.Contains(ext);
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
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
                if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                {
                    if (Music != null && Music.PlayLists != null)
                    {
                        var wh = Music.PlayLists.Where(x => !ReferenceEquals(x , UserSelectedPlayList));
                        if (wh != null)
                        {
                            return wh;
                        }
                        else
                        {
                            return new List<PlayList>();
                        }
                    }
                    else
                    {
                        return new List<PlayList>();
                    }
                }
                else
                {
                    return new List<PlayList>();
                }

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