using GMP.Classes;
using GMP.Extentions;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System;
using System.Collections;
using GMP.Enums;
using GMP.FlyOuts;

namespace GMP.ViewModel
{

    public class MainViewModel : ViewModel
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

        public void AddPlayList(string n , bool IsNotifyAdd = true)
        {
            Music.PlayLists.Add(new PlayList(Music) { Name = n });
            if (IsNotifyAdd)
            {
                SendLog($"Added 1 Playlist");
                Settings.Instance.SaveSettings(App.SavePath);
            }
        }
        public void AddPlayList(string n , IEnumerable<Song> From , bool IsNotifyAdd = true)
        {
            var pl = new PlayList(Music);
            pl.Name = n;
            
            AddSongs(From , pl , false , IsNotifyAdd);
            Music.PlayLists.Add(pl);
            if (IsNotifyAdd)
            {
                SendLog($"Added 1 Playlist");
                Settings.Instance.SaveSettings(App.SavePath);
            }
        }
        public void RemovePlayList(PlayList pl)
        {
            int curplloc = Music.PlayLists.IndexOf(pl);
            if (Music.Player.CurrentPlayList == pl)
            {
                //user wants to remove a playlist that has a song that is playing

                //move to the next playlist
                Music.PlayLists.MoveNext();

                if (Music.Player.CurrentPlayList == pl)
                {
                    //it's the only playlist with songs
                    Music.Player.Stop();
                    Music.Player.CurrentSong = null;
                    Music.Player.CurrentPlayList = null;
                    Music.PlayLists.Remove(pl);
                }
                else
                {
                    //moved to the next playlist successfully                        
                    Music.PlayLists.Remove(pl);
                    //open only if Current song previously existed in removed playlist
                    Music.Player.Open(Music.Player.CurrentSong , Music.Player.CurrentPlayList);
                }
            }
            else
            {
                //remove playlist normally
                Music.PlayLists.Remove(pl);
            }
            if (curplloc < Music.PlayLists.Count)
                UserSelectedPlayList = Music.PlayLists[curplloc];
        }


        public void AddSongs(IEnumerable<string> ToAdd , PlayList pl , bool IsNotifyAdd = true)
        {
            foreach (var item in ToAdd)
            {
                AddSong(item , pl , true);
            }
            if (IsNotifyAdd)
            {
                SendLog($"Added {ToAdd.Count()} Songs");
                Settings.Instance.SaveSettings(App.SavePath);
            }

        }
        public void AddSongs(IEnumerable<FileInfo> ToAdd , PlayList pl , bool IsNotifyAdd = false)
        {
            foreach (var item in ToAdd)
            {
                AddSong(item.FullName , pl , true);
            }

            if (IsNotifyAdd)
            {
                SendLog($"Added {ToAdd.Count()} Songs");
                Settings.Instance.SaveSettings(App.SavePath);
            }
        }
        public void AddSongs(IEnumerable<Song> ToAdd , PlayList pl , bool IsNewAdd = false , bool IsNotifyAdd = true)
        {
            foreach (var item in ToAdd.ToList())
            {
                if (IsNewAdd)
                {

                    AddSong(item.FullPath , pl , true);
                }
                else
                {
                    AddSong(item , pl , true);
                }

            }
            if (IsNotifyAdd)
            {
                SendLog($"Added {ToAdd.Count()} Songs");
                Settings.Instance.SaveSettings(App.SavePath);
            }

        }

        public void RemoveSongs(IEnumerable<Song> ToDelete , PlayList pl)
        {
            foreach (var item in ToDelete.ToList())
            {
                RemoveSong(item , pl , true);
            }
            SendLog($"Removed {ToDelete.Count()} Songs");
            Settings.Instance.SaveSettings(App.SavePath);
        }



        public void AddSong(string path , PlayList pl , bool IsMultiAdd = false)
        {
            var toadd = new Song(path , Music);
            pl.Add(toadd);
            if (!IsMultiAdd)
            {
                SendLog($"Added 1 Song");
                Settings.Instance.SaveSettings(App.SavePath);
            }
        }
        public void AddSong(Song ToAdd , PlayList pl , bool IsMultiAdd = false)
        {
            if (pl.Contains(ToAdd))
                return;
            pl.Add(ToAdd);
            if (!IsMultiAdd)
            {
                SendLog($"Added 1 Song");
                Settings.Instance.SaveSettings(App.SavePath);
            }
        }

        public void RemoveSong(Song ToDelete , PlayList pl , bool IsMultiRemove = false)
        {
            if (!pl.Contains(ToDelete)) return;
            bool wasplay = Music.PlayBackStatus == PlayBackStatuses.Playing;
            if (ReferenceEquals(Music.Player.CurrentSong , ToDelete))
            {
                Music.Player.Stop();
                Music.Player.Close();
                var indx = pl.IndexOf(ToDelete);
                pl.Remove(ToDelete);
                if (pl.Count > 0)
                {
                    pl.MoveNext(indx);
                    Music.Player.Open(pl.CurrentSong , pl);
                }
                else
                {
                    Music.Player.CurrentSong = null;
                }
            }
            else
            {
                pl.Remove(ToDelete);
            }
            if (!IsMultiRemove)
            {
                SendLog($"Removed 1 Song");
                Settings.Instance.SaveSettings(App.SavePath);
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


                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var init_pl = new PlayList(Music);
                    init_pl.Name = dirct.Name;
                    AddSongs(filepaths , init_pl);
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

                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var pl = new PlayList(Music);
                    pl.Name = wantedname;
                    AddSongs(validsongs , pl);
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

        private RelayCommand m_DeleteSongsCommand;

        public RelayCommand DeleteSongsCommand
        {
            get
            {
                if (m_DeleteSongsCommand == null)
                {
                    m_DeleteSongsCommand = new RelayCommand(ExecuteDeleteSongsCommand , CanExecuteDeleteSongsCommand);
                }
                return m_DeleteSongsCommand;
            }
            set
            {

                m_DeleteSongsCommand = value;
                OPC(nameof(DeleteSongsCommand));
            }
        }

        private bool CanExecuteDeleteSongsCommand(object arg)
        {
            return true;
        }

        private void ExecuteDeleteSongsCommand(object obj)
        {
            var rawlist = (IList)obj;
            var todelete = rawlist.Cast<Song>();
            int count = 0;
            foreach (var item in todelete.ToList())
            {
                RemoveSong(item , UserSelectedPlayList);
                count += 1;
            }
            LogFlyOut.SendLog(null , $"Removed {count} Songs" , FlyOuts.LogFlyOut.LogTypes.Normal);
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



        public void SendLog(string Message , LogFlyOut.LogTypes type = LogFlyOut.LogTypes.Normal)
        {

            LogFlyOut.SendLog(null , Message , type);
        }
        public void SendError(Exception ex)
        {
            LogFlyOut.SendLog(null , ex.ToString() , LogFlyOut.LogTypes.Error);
        }
    }
}