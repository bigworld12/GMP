using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using GMP.Classes;
using GMP.Enums;
using System.Windows.Threading;
using GMP.Extentions;
using NHotkey;
using NHotkey.Wpf;
using GMP.FlyOuts;
using Winforms = System.Windows.Forms;
using Newtonsoft.Json.Linq;
using static Updater.UpdateManager;
using System.Net;
using GMP.ViewModel;
using System.Drawing;
using System.Drawing.Imaging;
using GMP.Converters;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GMP.Controlling_Classes;
using System.Diagnostics;
using GMP.Views;

namespace GMP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>    
    public partial class MainWindow
    {
        public MainViewModel ViewModel { get { return (MainViewModel)TryFindResource("ViewModel"); } }





        public MusicListDropHandler MusicDropHandler
        {
            get { return (MusicListDropHandler)GetValue(MusicDropHandlerProperty); }
            set { SetValue(MusicDropHandlerProperty , value); }
        }

        // Using a DependencyProperty as the backing store for MusicDropHandler.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MusicDropHandlerProperty =
            DependencyProperty.Register("MusicDropHandler" , typeof(MusicListDropHandler) , typeof(MainWindow) , new PropertyMetadata(default(MusicListDropHandler)));




        public MainWindow()
        {
            InitializeComponent();
            App.SettingsSaved += App_SettingsSaved;

            ViewModel.SetMainWindInstance(this);
            Application.Current.MainWindow = this;
            ((CommunicationObject)TryFindResource("comobj")).MainWindowInstance = this;
            ((CommunicationObject)TryFindResource("comobj")).MusicInstance = Music;
            App.CommunicationObj = ((CommunicationObject)TryFindResource("comobj"));
            InitCommands();
            InitUpdateGUITimer();
            InitHotkeys();

            DataContext = this;
            MusicDropHandler = new MusicListDropHandler();
            MusicDropHandler.ViewModel = ViewModel;
            if (File.Exists(App.SavePath))
            {
                Settings.Instance.LoadSettings(App.SavePath);
                SendLog($"Loading Settings From : {App.SavePath}");
            }
            else
            {
                Settings.Instance.ToJson = Settings.Instance.DefaultSettings;
                Settings.Instance.SaveSettings(App.SavePath);
                LoadSettingsIntoGUI();
                SendLog($"Settings file not found, created a new one at : {App.SavePath}");
            }
            DownloaderWindow.ViewModel.Music = ViewModel.Music;
            //var paras = Environment.GetCommandLineArgs();
            //HandleArguments(paras);


            //Updater.LogListener
            LogListener += MainWindow_LogListener;

            string GMPFolderPath = @"/GMP";

            Task.Run(() =>
            {
                try
                {
                    var checkresult = CheckForUpdates(@"ftp.bigworld12.tk" , ProtectionManager.TTS.GetFTPCreds() , 21 , App.LocalVersion , $@"{GMPFolderPath}/version.txt" , $@"{GMPFolderPath}/Files" , App.EXEFolder);
                    SendLog($"Server Version : {checkresult.ServerVersion.ToString()}" , LogFlyOut.LogTypes.Warning);
                    Winforms.MessageBox.Show(checkresult.NewFilesResult.Count.ToString());
                    if (checkresult.ShouldUpdate)
                    {
                        SendLog("Found a new Update , Opening the update tool and closing the application");
                        StartUpdating(
                                "ftp.bigworld12.tk" ,
                                Path.Combine(App.EXEFolder , "bwUpdaterTool.exe") ,
                                App.EXEFolder ,
                                $@"{GMPFolderPath}/Files" ,
                                "ftps" ,
                                checkresult.NewFilesResult ,
                                @"EAAAAImcyr4KPWFBHWLQ2h9iCEBtc9hRvKabmRVGwM63uVba" ,
                                @"EAAAAJygOLyYRMIa3J6zrm5IC6UENfoGVniQ3h8B58JTqBYV");
                        Application.Current.Dispatcher.Invoke(() => { Application.Current.Shutdown(0); });
                    }                 
                }
                catch(Exception ex) { SendError(ex); }
            });
        }

        private void MainWindow_Exited(object sender , EventArgs e)
        {
            Settings.Instance.SaveSettings(App.SavePath);
            Music.Player.Close();
        }

        private void App_SettingsSaved(object sender , JObject e)
        {
            SendLog("Saved Settings.");
        }

        #region Properties

        public MusicObject Music
        {
            get { return ViewModel.Music; }
        }



        public string ReadableTitle
        {
            get { return (string)GetValue(ReadableTitleProperty.DependencyProperty); }
        }

        // Using a DependencyProperty as the backing store for ReadableTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyPropertyKey ReadableTitleProperty =
            DependencyProperty.RegisterReadOnly("ReadableTitle" , typeof(string) , typeof(MainWindow) , new PropertyMetadata($"Gaming Music Player - Ver : { App.LocalVersion.ToString()}"));







        #endregion

        #region Commands
        public void InitCommands()
        {
            PlaySongCommand = new RelayCommand(ExecutePlaySong , CanExecutePlaySong);
            PauseSongCommand = new RelayCommand(ExecutePauseSong , CanExecutePauseSong);
        }


        public ICommand PlaySongCommand
        {
            get;
            internal set;
        }
        private void ExecutePlaySong(object obj)
        {
            var songtoplay = (Song)obj;
            if (ReferenceEquals(songtoplay , Music.Player.CurrentSong))
            {
                switch (Music.PlayBackStatus)
                {
                    case PlayBackStatuses.Paused:
                        Music.Player.Play();
                        break;
                    case PlayBackStatuses.Stopped:
                        Music.Player.Play();
                        break;
                    default:
                        break;
                }

            }
            else
            {
                if (songtoplay == null)
                    ViewModel.UserSelectedPlayList.MoveNext();
                Music.Player.Open(songtoplay , ViewModel.UserSelectedPlayList);
            }
        }
        private bool CanExecutePlaySong(object arg)
        {
            return true;
        }

        public ICommand PauseSongCommand
        {
            get;
            internal set;
        }
        private void ExecutePauseSong(object obj)
        {
            Music.Player.Pause();
        }
        private bool CanExecutePauseSong(object arg)
        {
            return Music.Player.CanPause;
        }

        #endregion





        private void UpdateGUITimer_Tick(object sender , EventArgs e)
        {
            var timer = (DispatcherTimer)sender;
            var musicobj = (MusicObject)timer.Tag;
            if (!ViewModel.IsUserEditing)
            {
                ReadableProgressValue = musicobj.Player.SPosition;
            }
            if (Music.Player.CurrentSong != null)
            {
                if (!ViewModel.IsUserEditing && Music.Player.CurrentSong.IsUseStartEndRange && Music.Player.CurrentSong.EndPos != Music.Player.CurrentSong.MaxDuration && ReadableProgressValue > Music.Player.CurrentSong.EndPos)
                {
                    Music.Player.InvokeCustomMediaEnded();
                }
                else if (!ViewModel.IsUserEditing && Music.Player.CurrentSong.IsUseStartEndRange && Music.Player.CurrentSong.StartPos != 0 && ReadableProgressValue < Music.Player.CurrentSong.StartPos)
                {
                    Music.Player.SPosition = Music.Player.CurrentSong.StartPos;
                }
                Music.Player.CurrentSong.Position = Music.Player.SPosition;
            }
        }
        public void LoadSettingsIntoGUI()
        {
            /*  js["isuserange"] = s.IsUseStartEndRange;
                    js["startpos"] = s.StartPos;
                    js["endpos"] = s.EndPos;*/


            //load stuff from the Json file
            if (App.SettingsInstance != null && App.SettingsInstance.ToJson != null)
            {

                var j = App.SettingsInstance.ToJson;

                try
                {
                    var jpl = j.GV<JArray>("playlists");
                    if (jpl != null)
                    {
                        foreach (JObject item in jpl)
                        {
                            var newpl = new PlayList(Music);
                            newpl.Name = item.GV<string>("name");
                            var jsongs = item.GV<JArray>("songs");
                            foreach (JObject jsong in jsongs)
                            {
                                if (File.Exists(jsong.GV<string>("path")))
                                {
                                    var newsong = new Song(jsong.GV<string>("path") , Music);
                                    newsong.IsFav = jsong.GV<bool>("isfav");

                                    if (jsong["isuserange"] != null) newsong.IsUseStartEndRange = jsong.GV<bool>("isuserange");
                                    if (jsong["startpos"] != null) newsong.StartPos = jsong.GV<double>("startpos");
                                    if (jsong["endpos"] != null) newsong.EndPos = jsong.GV<double>("endpos");
                                    newpl.Add(newsong);
                                }
                            }
                            Music.PlayLists.Add(newpl);
                        }
                    }
                }
                catch (Exception e)
                {
                    SendError(e);
                }
                try
                {
                    Music.Player.Volume = j.GV<double>("Volume");
                }
                catch (Exception e) { SendError(e); }



                try
                {
                    int cursi = j.GV<int>("CurrentSongIndex");
                    int curpli = j.GV<int>("CurrentPlayListIndex");
                    //after loading play list
                    if (curpli == -1)
                    {
                        Music.Player.CurrentPlayList = null;
                        Music.Player.CurrentSong = null;
                    }
                    else
                    {
                        if (cursi == -1)
                        {
                            Music.Player.CurrentPlayList = null;
                            Music.Player.CurrentSong = null;
                        }
                        else
                        {
                            if (curpli >= Music.PlayLists.Count)
                            {
                                Music.Player.CurrentPlayList = null;
                                Music.Player.CurrentSong = null;
                            }
                            else
                            {
                                //check for song 
                                var curpl = Music.PlayLists[curpli];
                                if (cursi >= curpl.Count)
                                {
                                    Music.Player.CurrentPlayList = null;
                                    Music.Player.CurrentSong = null;
                                }
                                else
                                {
                                    Music.Player.CurrentPlayList = curpl;
                                    Music.Player.CurrentSong = Music.Player.CurrentPlayList[cursi];
                                    Music.Player.Open(Music.Player.CurrentSong , Music.Player.CurrentPlayList);

                                    PlaylistsRepresenter.SelectedItem = Music.Player.CurrentPlayList;
                                }
                            }
                        }
                    }
                }
                catch (Exception e) { SendError(e); }              
                try
                {
                    Music.RepeatMode = (RepeatModes)Enum.Parse(typeof(RepeatModes) , j.GV<string>("Repeat Mode"));
                }
                catch (Exception e) { SendError(e); }
              
                try
                {
                    //hotkeys
                    var jhks = j.GV<JArray>("HotKeys");
                    foreach (JObject jhk in jhks)
                    {
                        var keyenum = (Key)Enum.Parse(typeof(Key) , jhk.GV<string>("key"));

                        var fnalmod = jhk.GV<string[]>("mod")?.Select(xz => { ModifierKeys outenum; Enum.TryParse(xz , out outenum); return outenum; }).Aggregate((prev , next) => prev | next);
                        if (fnalmod.HasValue)
                        {
                            var x = new HotKey(keyenum , fnalmod.Value);
                            SetValueFromSimpleName(jhk.GV<string>("name") , x);
                        }

                    }
                }
                catch (Exception e)
                {
                    SendError(e);
                }

                try
                {
                    var savedjson = App.SettingsInstance.ToJson.GV<string>("Latest Save Path");
                    if (Directory.Exists(savedjson))
                    {
                        DownloaderWindow.ViewModel.WantedSaveFolder = savedjson;
                    }
                }
                catch (Exception e)
                {
                    SendError(e);
                }

            }

            var newdt = new DispatcherTimer();
            newdt.Interval = TimeSpan.FromSeconds(2);
            newdt.Tick += Newdt_Tick;
            newdt.Start();
        }


        private void Newdt_Tick(object sender , EventArgs e)
        {
            var dt = sender as DispatcherTimer;
            dt.Stop();
            if (Music.Player.CurrentSong != null)
            {
                SongsViewer.ScrollIntoView(Music.Player.CurrentSong);
            }
        }

        public DispatcherTimer UpdateGUITimer;



        public void HandleArguments(string[] args)
        {
            try
            {
                var apppath = args[0];
                if (args.Length > 1)
                {
                    PlayList toaddpl = new PlayList(Music);
                    for (int i = 1; i < args.Length; i++)
                    {
                        var rawpath = args[i];
                        if (File.Exists(rawpath))
                        {
                            var newsong = new Song(rawpath , Music);
                            toaddpl.Add(newsong);
                        }
                    }
                    Music.PlayLists.Add(toaddpl);

                    ViewModel.UserSelectedPlayList = toaddpl;

                    Music.Player.CurrentSong = toaddpl.First();
                    Music.Player.CurrentPlayList = toaddpl;

                    Music.Player.Open(Music.Player.CurrentSong , Music.Player.CurrentPlayList);
                }
            }
            catch (Exception ex)
            {
                SendLog($"Parsing startup arguments Gave this Exception :\n{ex.ToString()}" , LogFlyOut.LogTypes.Error);
            }
        }



        private void MainWindow_LogListener(object sender , string e)
        {
            SendLog(e , LogFlyOut.LogTypes.Warning);
        }

        private sealed class FtpsWebRequestCreator : IWebRequestCreate
        {
            public WebRequest Create(Uri uri)
            {
                FtpWebRequest webRequest = (FtpWebRequest)WebRequest.Create(uri.AbsoluteUri.Remove(3 , 1)); // Removes the "s" in "ftps://".
                webRequest.EnableSsl = true;
                return webRequest;
            }
        }


        public void InitUpdateGUITimer()
        {
            UpdateGUITimer = new DispatcherTimer();
            UpdateGUITimer.Interval = TimeSpan.FromMilliseconds(100);
            UpdateGUITimer.IsEnabled = false;
            UpdateGUITimer.Tick += UpdateGUITimer_Tick;
            UpdateGUITimer.Tag = Music;
            UpdateGUITimer.Start();
        }
        private void OpenMusicMenuItem_Click(object sender , RoutedEventArgs e)
        {
            var muzicfile = (Song)SongsViewer.SelectedItem;
            var playlst = (PlayList)PlaylistsRepresenter.SelectedItem;
            Music.Player.Open(muzicfile , playlst);
        }

        private void AddSongButton_Click(object sender , RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".mp3";
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;
            dlg.AddExtension = true;
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
            dlg.Multiselect = true;
            dlg.Filter = "Audio files (*.mp3;*.m4a;*.wma;*.wav;*.w64;*.ogg;*.flac)|*.mp3;*.m4a;*.wma;*.wav;*.w64;*.ogg;*.flac|All Files|*.*";
            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value)
            {
                ViewModel.AddSongs(dlg.FileNames , ViewModel.UserSelectedPlayList);
            }
        }

        private void RemoveMusicMenuItem_Click(object sender , RoutedEventArgs e)
        {
            ViewModel.RemoveSongs(ViewModel.UserSelectedSongs , ViewModel.UserSelectedPlayList);
        }


        private void CopyToMenuiItem_Click(object sender , RoutedEventArgs e)
        {
            var menuitm = (MenuItem)sender;
            var playlist = (PlayList)menuitm.DataContext;

            ViewModel.AddSongs(ViewModel.UserSelectedSongs , playlist , true);
        }
        private void MoveToMenuiItem_Click(object sender , RoutedEventArgs e)
        {
            var menuitm = (MenuItem)sender;
            var playlist = (PlayList)menuitm.DataContext;
            //move UserSelectedSongs from UserSelectedPlayList to  playlist

            if (ViewModel.UserSelectedSongs.Any(x => ReferenceEquals(x , Music.Player.CurrentSong)))
            {
                //change current playlist to the new playlist
                Music.Player.CurrentPlayList = playlist;
            }
            var list = ViewModel.UserSelectedSongs.ToList();
            foreach (var item in list)
            {
                ViewModel.RemoveSong(item , ViewModel.UserSelectedPlayList , true);
                ViewModel.AddSong(item , playlist , true);
            }
            ViewModel.UserSelectedPlayList = playlist;
            SendLog($"Moved {list.Count} Songs");
        }


        private void AddPlayListButton_Click(object sender , RoutedEventArgs e)
        {
            ViewModel.AddPlayList(null);
        }

        private void MoveNextSong_MouseDown(object sender , MouseButtonEventArgs e)
        {

            switch (Music.RepeatMode)
            {
                case RepeatModes.None:
                case RepeatModes.SingleSong:
                case RepeatModes.AllLists:
                    Music.PlayLists.GeneralMoveNext();
                    break;
                case RepeatModes.SingleList:
                    Music.PlayLists.CurrentPlayList.MoveNext();
                    break;
                default:
                    break;
            }

            Music.Player.Open(Music.PlayLists.CurrentPlayList.CurrentSong , Music.PlayLists.CurrentPlayList);
        }

        private void MovePreviousSong_MouseDown(object sender , MouseButtonEventArgs e)
        {
            switch (Music.RepeatMode)
            {
                case RepeatModes.None:
                case RepeatModes.SingleSong:
                case RepeatModes.AllLists:
                    Music.PlayLists.GeneralMovePrevious();
                    break;
                case RepeatModes.SingleList:
                    Music.PlayLists.CurrentPlayList.MovePrevious();
                    break;
                default:
                    break;
            }

            Music.Player.Open(Music.PlayLists.CurrentPlayList.CurrentSong , Music.PlayLists.CurrentPlayList);
        }


        private void PauseSong_MouseDown(object sender , MouseButtonEventArgs e)
        {
            Music.Player.Pause();
        }

        private void OpenHotKeySettingsFlyOut_Click(object sender , RoutedEventArgs e)
        {
            HotKSetFly.IsOpen = !HotKSetFly.IsOpen;
        }

        private void PlayingMusicProgressSlider_MouseDown(object sender , MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right) return;
            if (Music.Player.CurrentSong == null) return;
            ViewModel.IsUserEditing = true;
        }
        public double ReadableProgressValue
        {
            get
            {
                return (double)GetValue(ReadableProgressValueProperty);
            }
            set
            {
                SetValue(ReadableProgressValueProperty , value);

            }
        }
        // Using a DependencyProperty as the backing store for ReadableProgressValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReadableProgressValueProperty =
            DependencyProperty.Register("ReadableProgressValue" , typeof(double) , typeof(MainWindow) , new PropertyMetadata(default(double)));

        private void PlayingMusicProgressSlider_MouseUp(object sender , MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Right) return;
            if (Music.Player.CurrentSong == null) return;
            if (ViewModel.IsUserEditing)
            {
                //mousedown was called                
                Music.Player.SPosition = ReadableProgressValue;
                ViewModel.IsUserEditing = false;
            }
            else
            {
                //one click only
                if (e.LeftButton == MouseButtonState.Released)
                {
                    UpdateGUITimer.Stop();
                    ViewModel.IsUserEditing = true;
                    var newpos = (e.GetPosition(PlayingMusicProgressSlider).X / PlayingMusicProgressSlider.ActualWidth) * Music.Player.CurrentSong.MaxDuration;
                    if (Music.Player.CurrentSong.IsUseStartEndRange)
                    {
                        if (newpos < Music.Player.CurrentSong.EndPos)
                        {
                            Music.Player.SPosition = newpos;
                        }
                    }
                    else
                    {
                        Music.Player.SPosition = newpos;
                    }

                    if (Music.PlayBackStatus != PlayBackStatuses.Paused && Music.PlayBackStatus != PlayBackStatuses.Playing)
                        PlaySongCommand.Execute(Music.Player.CurrentSong);

                    ViewModel.IsUserEditing = false;
                    UpdateGUITimer.Start();

                }
            }



        }

        private void PlayButton_MouseDown(object sender , MouseButtonEventArgs e)
        {
            PlaySongCommand.Execute(Music.Player.CurrentSong);
        }

        private void AddFolderButton_Click(object sender , RoutedEventArgs e)
        {
            var dlg = new Winforms.FolderBrowserDialog();

            dlg.ShowNewFolderButton = true;
            dlg.RootFolder = Environment.SpecialFolder.MyComputer;
            dlg.Description = "Please choose the folder of the songs you want to add";
            var result = dlg.ShowDialog();
            if (result == Winforms.DialogResult.OK)
            {
                IninstCounter count = new IninstCounter();
                count.Count = 0;
                ViewModel.AddDirectory(dlg.SelectedPath , count);
                SendLog($"Added {count} Songs.");
                Settings.Instance.SaveSettings(App.SavePath);
            }
        }


        private void SaveButton_Click(object sender , RoutedEventArgs e)
        {
            Settings.Instance.SaveSettings(App.SavePath);
        }



        private void RemoveSongsButton_Click(object sender , RoutedEventArgs e)
        {
            ViewModel.RemoveSongs(ViewModel.UserSelectedSongs , ViewModel.UserSelectedPlayList);
        }

        private void RemoveDupilicatesButton_Click(object sender , RoutedEventArgs e)
        {

            var distinct = ViewModel.UserSelectedPlayList.Distinct(new SongEqualityComparer());

            var removeditems = new List<Song>();
            foreach (var item in ViewModel.UserSelectedPlayList.ToList())
            {
                if (!distinct.Contains(item))
                {
                    removeditems.Add(item);
                    ViewModel.RemoveSong(item , ViewModel.UserSelectedPlayList , true);
                }
            }
            SendLog($"Removed {removeditems.Count} Duplicates.");
            Settings.Instance.SaveSettings(App.SavePath);
        }


        private void RemovePlayListButton_Click(object sender , RoutedEventArgs e)
        {
            if (ViewModel.UserSelectedPlayList != null)
            {
                ViewModel.RemovePlayList(ViewModel.UserSelectedPlayList);
                Settings.Instance.SaveSettings(App.SavePath);
            }
        }

        private void ShowLogButton_Click(object sender , RoutedEventArgs e)
        {
            LogFly.IsOpen = !LogFly.IsOpen;
        }

        private void MainWin_Loaded(object sender , RoutedEventArgs e)
        {

        }

        private void SongsViewer_SelectionChanged(object sender , SelectionChangedEventArgs e)
        {
            SongsViewer.SetValue(ListBoxHelper.IsMultiSelectedProperty , IsHasMultiSongs());
            ViewModel.OPC("MultiSongsVisiblity");
        }
        public bool IsHasMultiSongs()
        {
            if (SongsViewer != null && SongsViewer.SelectedItems != null)
            {
                if (SongsViewer.SelectedItems.Count > 1)
                {
                    return true;

                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        private void PlaylistsRepresenter_SelectionChanged(object sender , SelectionChangedEventArgs e)
        {
            ViewModel.OPC("GetMoveToPlaylists");
            if (Music.Player.CurrentSong != null && Music.Player.CurrentPlayList != null && PlaylistsRepresenter.SelectedItem != null && ((PlayList)PlaylistsRepresenter.SelectedItem).Contains(Music.Player.CurrentSong))
            {
                SongsViewer.ScrollIntoView(Music.Player.CurrentSong);
            }
        }
        private void MoveFirstMenuItem_Click(object sender , RoutedEventArgs e)
        {
            var mi = (MenuItem)sender;
            PlayList pltomove = (PlayList)mi.DataContext;
            Music.PlayLists.Move(Music.PlayLists.IndexOf(pltomove) , 0);
            ViewModel.UserSelectedPlayList = pltomove;
        }

        private void MoveLastMenuItem_Click(object sender , RoutedEventArgs e)
        {
            var mi = (MenuItem)sender;
            PlayList pltomove = (PlayList)mi.DataContext;
            Music.PlayLists.Move(Music.PlayLists.IndexOf(pltomove) , Music.PlayLists.Count - 1);
            ViewModel.UserSelectedPlayList = pltomove;
        }

        private void MoveUpMenuItem_Click(object sender , RoutedEventArgs e)
        {
            var mi = (MenuItem)sender;
            PlayList pltomove = (PlayList)mi.DataContext;
            var oldindx = Music.PlayLists.IndexOf(pltomove);
            if (oldindx != 0)
            {
                Music.PlayLists.Move(oldindx , oldindx - 1);
            }
            ViewModel.UserSelectedPlayList = pltomove;

        }
        private void MoveDownMenuItem_Click(object sender , RoutedEventArgs e)
        {
            var mi = (MenuItem)sender;
            PlayList pltomove = (PlayList)mi.DataContext;
            var oldindx = Music.PlayLists.IndexOf(pltomove);
            if (oldindx != Music.PlayLists.Count - 1)
            {
                Music.PlayLists.Move(oldindx , oldindx + 1);
            }
            ViewModel.UserSelectedPlayList = pltomove;
        }
        private void RemovePlayListMenuItem_Click(object sender , RoutedEventArgs e)
        {
            var minitem = (MenuItem)sender;
            var pl = (PlayList)minitem.DataContext;
            ViewModel.RemovePlayList(pl);
        }

        private void RemoveInvalidSongsButton_Click(object sender , RoutedEventArgs e)
        {
            int count = 0;

            foreach (var item in Music.PlayLists)
            {
                var todelete = item.Where(x => !File.Exists(x.FullPath));
                foreach (var music in todelete)
                {
                    ViewModel.RemoveSong(music , item , true);
                    count += 1;
                }
            }
            SendLog($"Removed {count} Invalid Songs");
            Settings.Instance.SaveSettings(App.SavePath);
        }

        private void OfficialPageButton_Click(object sender , RoutedEventArgs e)
        {
            Process.Start(@"https://bigworld12.github.io/GMP/");
        }
        public MusicDownloader DownloaderWindow
        {
            get { return (MusicDownloader)TryFindResource("DownloaderWindow"); }
        }

        private void MusicDownloaderButton_Click(object sender , RoutedEventArgs e)
        {
            DownloaderWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            DownloaderWindow.Visibility = Visibility.Visible;
        }
    }

    /// <summary>
    /// Hotkeys
    /// </summary>
    public partial class MainWindow
    {
        public HotKey GetValueFromSimpleName(string simplename)
        {
            string propname;
            var dictlist = HKDict.ToList();
            propname = dictlist.Find(x => x.Value == simplename).Key;
            return GetValueFromPropName(propname);
        }
        public HotKey GetValueFromPropName(string propname)
        {
            switch (propname)
            {
                case nameof(PlayPauseHotKey):
                    return PlayPauseHotKey;
                case nameof(NextSongHotKey):
                    return NextSongHotKey;
                case nameof(PreviousSongHotKey):
                    return PreviousSongHotKey;
                case nameof(NextPlayListHotKey):
                    return NextPlayListHotKey;
                case nameof(PreviousPlayListHotKey):
                    return PreviousPlayListHotKey;
                case nameof(IncreaseVolumeHotKey):
                    return IncreaseVolumeHotKey;
                case nameof(DecreaseVolumeHotKey):
                    return DecreaseVolumeHotKey;
                default:
                    return null;
            }
        }


        public void SetValueFromPropName(string propname , HotKey value)
        {
            switch (propname)
            {
                case nameof(PlayPauseHotKey):
                    PlayPauseHotKey = value;
                    break;
                case nameof(NextSongHotKey):
                    NextSongHotKey = value;
                    break;
                case nameof(PreviousSongHotKey):
                    PreviousSongHotKey = value;
                    break;
                case nameof(NextPlayListHotKey):
                    NextPlayListHotKey = value;
                    break;
                case nameof(PreviousPlayListHotKey):
                    PreviousPlayListHotKey = value;
                    break;
                case nameof(IncreaseVolumeHotKey):
                    IncreaseVolumeHotKey = value;
                    break;
                case nameof(DecreaseVolumeHotKey):
                    DecreaseVolumeHotKey = value;
                    break;
                default:
                    break;
            }
        }
        public void SetValueFromSimpleName(string simplename , HotKey value)
        {
            string propname;
            var dictlist = HKDict.ToList();
            propname = dictlist.Find(x => x.Value == simplename).Key;
            SetValueFromPropName(propname , value);
        }








        public void InitHotkeys()
        {

            QuickAR(HKDict[nameof(PlayPauseHotKey)] , PlayPauseHotKey);
            QuickAR(HKDict[nameof(NextSongHotKey)] , NextSongHotKey);
            QuickAR(HKDict[nameof(PreviousSongHotKey)] , PreviousSongHotKey);
            QuickAR(HKDict[nameof(NextPlayListHotKey)] , NextPlayListHotKey);
            QuickAR(HKDict[nameof(PreviousPlayListHotKey)] , PreviousPlayListHotKey);
            QuickAR(HKDict[nameof(IncreaseVolumeHotKey)] , IncreaseVolumeHotKey);
            QuickAR(HKDict[nameof(DecreaseVolumeHotKey)] , DecreaseVolumeHotKey);

        }
        string nosongsfoundmessage = "There are no songs,please consider adding some";
        /// <summary>
        /// handles the logic of the pressed key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnHotkeyPress(object sender , HotkeyEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                e.Handled = true;
                switch (e.Name)
                {
                    case "pphk":
                        if (Music.PlayLists.IsGlobalNoSongs)
                        {
                            return;
                        }
                        if (Music.PlayBackStatus == PlayBackStatuses.Playing)
                        {
                            PauseSongCommand.Execute(Music.Player.CurrentSong);
                        }
                        else
                        {
                            PlaySongCommand.Execute(Music.Player.CurrentSong);
                        }
                        break;
                    case "nshk":
                        MoveNextSong_MouseDown(null , null);
                        break;
                    case "pshk":
                        MovePreviousSong_MouseDown(null , null);
                        break;
                    case "nplhk":
                        Music.PlayLists.MoveNext();
                        Music.Player.Open(Music.PlayLists.CurrentPlayList.CurrentSong , Music.PlayLists.CurrentPlayList);
                        break;
                    case "pplhk":
                        Music.PlayLists.MovePrevious();
                        Music.Player.Open(Music.PlayLists.CurrentPlayList.CurrentSong , Music.PlayLists.CurrentPlayList);
                        break;
                    case "ivhk":
                        if (Music.Player.Volume != 1)
                            Music.Player.Volume += 0.1;
                        break;
                    case "dvhk":

                        if (Music.Player.Volume != 0)
                            Music.Player.Volume -= 0.1;
                        break;
                    default:
                        SendLog(nosongsfoundmessage , LogFlyOut.LogTypes.Warning);
                        break;
                }
            });

        }


        private void QuickAR(string hkname , HotKey hkinstance)
        {
            try
            {
                HotkeyManager.Current.AddOrReplace(hkname , hkinstance.Key , hkinstance.ModifierKeys , OnHotkeyPress);
            }
            catch (Exception e)
            {
                SendError(e);
            }

        }

        public void SendLog(string log , LogFlyOut.LogTypes type = LogFlyOut.LogTypes.Normal)
        {
            ViewModel.SendLog(log , type);
        }
        public void SendError(Exception ex)
        {
            ViewModel.SendError(ex);
        }

        /// <summary>
        /// pphk
        /// </summary>
        public HotKey PlayPauseHotKey
        {
            get
            { return (HotKey)GetValue(PlayPauseHotKeyProperty); }
            set
            { SetValue(PlayPauseHotKeyProperty , value); }
        }
        public static readonly DependencyProperty PlayPauseHotKeyProperty =
            DependencyProperty.Register(nameof(PlayPauseHotKey) , typeof(HotKey) , typeof(MainWindow) , new PropertyMetadata(new HotKey(Key.Q , ModifierKeys.Alt) , HKChanged));



        /// <summary>
        /// nshk
        /// </summary>
        public HotKey NextSongHotKey
        {
            get { return (HotKey)GetValue(NextSongHotKeyProperty); }
            set { SetValue(NextSongHotKeyProperty , value); }
        }

        // Using a DependencyProperty as the backing store for NextSongHotKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NextSongHotKeyProperty =
            DependencyProperty.Register(nameof(NextSongHotKey) , typeof(HotKey) , typeof(MainWindow) , new PropertyMetadata(new HotKey(Key.C , ModifierKeys.Alt) , HKChanged));

        /// <summary>
        /// pshk
        /// </summary>
        public HotKey PreviousSongHotKey
        {
            get { return (HotKey)GetValue(PreviousSongHotKeyProperty); }
            set { SetValue(PreviousSongHotKeyProperty , value); }
        }

        // Using a DependencyProperty as the backing store for PreviousSongHotKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreviousSongHotKeyProperty =
            DependencyProperty.Register(nameof(PreviousSongHotKey) , typeof(HotKey) , typeof(MainWindow) , new PropertyMetadata(new HotKey(Key.X , ModifierKeys.Alt) , HKChanged));




        /// <summary>
        /// nplhk
        /// </summary>
        public HotKey NextPlayListHotKey
        {
            get { return (HotKey)GetValue(NextPlayListHotKeyProperty); }
            set { SetValue(NextPlayListHotKeyProperty , value); }
        }

        // Using a DependencyProperty as the backing store for NextPlayListHotKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NextPlayListHotKeyProperty =
            DependencyProperty.Register(nameof(NextPlayListHotKey) , typeof(HotKey) , typeof(MainWindow) , new PropertyMetadata(new HotKey(Key.V , ModifierKeys.Alt) , HKChanged));


        /// <summary>
        /// pplhk
        /// </summary>
        public HotKey PreviousPlayListHotKey
        {
            get { return (HotKey)GetValue(PreviousPlayListHotKeyProperty); }
            set { SetValue(PreviousPlayListHotKeyProperty , value); }
        }

        // Using a DependencyProperty as the backing store for PreviousPlayListHotKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreviousPlayListHotKeyProperty =
            DependencyProperty.Register(nameof(PreviousPlayListHotKey) , typeof(HotKey) , typeof(MainWindow) , new PropertyMetadata(new HotKey(Key.Z , ModifierKeys.Alt) , HKChanged));




        /// <summary>
        /// ivhk
        /// </summary>
        public HotKey IncreaseVolumeHotKey
        {
            get { return (HotKey)GetValue(IncreaseVolumeHotKeyProperty); }
            set { SetValue(IncreaseVolumeHotKeyProperty , value); }
        }

        // Using a DependencyProperty as the backing store for IncreaseVolumeHotKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IncreaseVolumeHotKeyProperty =
            DependencyProperty.Register(nameof(IncreaseVolumeHotKey) , typeof(HotKey) , typeof(MainWindow) , new PropertyMetadata(new HotKey(Key.G , ModifierKeys.Alt) , HKChanged));



        /// <summary>
        /// dvhk
        /// </summary>
        public HotKey DecreaseVolumeHotKey
        {
            get { return (HotKey)GetValue(DecreaseVolumeHotKeyProperty); }
            set { SetValue(DecreaseVolumeHotKeyProperty , value); }
        }

        // Using a DependencyProperty as the backing store for DecreaseVolumeHotKey.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DecreaseVolumeHotKeyProperty =
            DependencyProperty.Register(nameof(DecreaseVolumeHotKey) , typeof(HotKey) , typeof(MainWindow) , new PropertyMetadata(new HotKey(Key.F , ModifierKeys.Alt) , HKChanged));


        private static void HKChanged(DependencyObject d , DependencyPropertyChangedEventArgs e)
        {
            var val = e.NewValue;
            string equihkname = HKDict[e.Property.Name];
            ((MainWindow)Application.Current.MainWindow)?.QuickHKV(equihkname , (HotKey)val);
        }
        /// <summary>
        /// key = nameof , value = simple name
        /// </summary>
        public static Dictionary<string , string> HKDict = new Dictionary<string , string>()
        {
            { nameof(PlayPauseHotKey),"pphk" } ,
            { nameof(NextSongHotKey),"nshk" },
            { nameof(PreviousSongHotKey),"pshk"},
            { nameof(NextPlayListHotKey),"nplhk"},
            { nameof(PreviousPlayListHotKey),"pplhk" },
            { nameof(IncreaseVolumeHotKey),"ivhk"},
            { nameof(DecreaseVolumeHotKey),"dvhk"}

        };


        //todo stopped here :
        private void QuickHKV(string name , HotKey newval)
        {
            try
            {
                if (newval == null || newval.Key == Key.None)
                {
                    HotkeyManager.Current.Remove(name);
                }
                else
                {
                    QuickAR(name , newval);
                }
            }
            catch (Exception e)
            {
                SendError(e);
            }
        }

    }

    /// <summary>
    /// overlay
    /// </summary>
    public partial class MainWindow
    {
        private Bitmap m_OverlayImage;
        /// <summary>
        /// contains the object to be rendered
        /// Note : Rendering happens here
        /// </summary>
        public Bitmap OverlayImage
        {
            get { return m_OverlayImage; }
            set
            {
                if (m_OverlayImage != null)
                {
                    m_OverlayImage.Dispose();
                }
                m_OverlayImage = value;
                if (value != null)
                    SetOverlay(value);
            }
        }
        public void UpdateImage(System.Drawing.Size RenderSize)
        {
            //first, create a dummy bitmap just to get a graphics object
            Bitmap img = new Bitmap(RenderSize.Width , RenderSize.Height , System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics drawing = Graphics.FromImage(img);
            drawing.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //paint the background
            drawing.Clear(System.Drawing.Color.Transparent);

            string mplstr = string.Empty;
            string progressstr = string.Empty;
            string repeatmodestr = string.Empty;


            if (Music.Player.CurrentSong != null)
            {
                mplstr = $"Currently Playing : {Music.Player.CurrentPlayList.ReadableRepresentation} / {Music.Player.CurrentSong.SongName}";
            }
            else
            {
                mplstr = $"Currently Playing : Nothing";
            }
            if (Music.Player.CurrentSong != null && Music.PlayBackStatus != PlayBackStatuses.Failed)
            {
                progressstr = Music.Player.CurrentSong.ReadableProgress;
            }
            else
            {
                progressstr = "(00:00/00:00)";
            }
            repeatmodestr = $"Repeat Mode : {new RepeatModeStringConverter().Convert(Music.RepeatMode , null , null , null)}";


            Font goodmplstrfont = new Font("Arial" , 16 , System.Drawing.FontStyle.Regular);
            FindGoodFont(drawing , mplstr , RenderSize , ref goodmplstrfont , GraphicsUnit.Point);

            Font goodprogressstrfont = new Font("Arial" , 16 , System.Drawing.FontStyle.Regular);
            FindGoodFont(drawing , progressstr , RenderSize , ref goodprogressstrfont , GraphicsUnit.Point);

            Font goodrepeatmodestrfont = new Font("Arial" , 16 , System.Drawing.FontStyle.Regular);
            FindGoodFont(drawing , repeatmodestr , RenderSize , ref goodrepeatmodestrfont , GraphicsUnit.Point);

            SizeF goodmplstrfontsize = drawing.MeasureString(mplstr , goodmplstrfont);
            SizeF goodprogressstrfontsize = drawing.MeasureString(progressstr , goodprogressstrfont);
            SizeF goodrepeatmodestrfontsize = drawing.MeasureString(repeatmodestr , goodrepeatmodestrfont);


            drawing.DrawString(mplstr , goodmplstrfont , System.Drawing.Brushes.Black , new PointF(0 , 0));
            drawing.DrawString(progressstr , goodprogressstrfont , System.Drawing.Brushes.Black , new PointF(0 , goodmplstrfontsize.Height + 10));
            drawing.DrawString(repeatmodestr , goodrepeatmodestrfont , System.Drawing.Brushes.Black , new PointF(0 , goodprogressstrfontsize.Height + 50));

            /*
            the final Result should be :
             
            Currently Playing : <Whatever info here>
            (<currentprogress> / <duration>)
            Repeat Mode : <repeat mode here>
            */


            goodmplstrfont.Dispose();
            goodprogressstrfont.Dispose();
            goodrepeatmodestrfont.Dispose();

            drawing.Save();
            drawing.Dispose();

            //set the property to render the overlay
            OverlayImage = img;
        }

        // You hand this the text that you need to fit inside some
        // available room, and the font you'd like to use.
        // If the text fits nothing changes
        // If the text does not fit then it is reduced in size to
        // make it fit.
        // PreferedFont is the Font that you wish to apply
        // FontUnit is there because the default font unit is not
        // always the one you use, and it is info required in the
        // constructor for the new Font.
        public static void FindGoodFont(Graphics Graf , string sStringToFit ,
                                        System.Drawing.Size TextRoomAvail ,
                                        ref Font FontToUse ,
                                        GraphicsUnit FontUnit)
        {
            // Find out what the current size of the string in this font is
            SizeF RealSize = Graf.MeasureString(sStringToFit , FontToUse);
            if ((RealSize.Width <= TextRoomAvail.Width) && (RealSize.Height <= TextRoomAvail.Height))
            {
                // The current font is fine...
                return;
            }

            // Either width or height is too big...
            // Usually either the height ratio or the width ratio
            // will be less than 1. Work them out...
            float HeightScaleRatio = TextRoomAvail.Height / RealSize.Height;
            float WidthScaleRatio = TextRoomAvail.Width / RealSize.Width;

            // We'll scale the font by the one which is furthest out of range...
            float ScaleRatio = (HeightScaleRatio < WidthScaleRatio) ? ScaleRatio = HeightScaleRatio : ScaleRatio = WidthScaleRatio;
            float ScaleFontSize = FontToUse.Size * ScaleRatio;



            System.Drawing.FontStyle OldFontStyle = FontToUse.Style;

            // Get rid of the old non working font...
            FontToUse.Dispose();

            // Tell the caller to use this newer smaller font.
            FontToUse = new Font(FontToUse.FontFamily ,
                                    ScaleFontSize ,
                                    OldFontStyle ,
                                    FontUnit);
        }

        public static void SetOverlay(Bitmap tooverlay)
        {
            if (tooverlay == null) return;
            //to do
            //currently shows the image in a control
            Window tempwin = new Window();
            var imgcontrol = new System.Windows.Controls.Image();
            tempwin.Content = imgcontrol;
            imgcontrol.Source = CreateBitmapSourceFromGdiBitmap(tooverlay);
            tempwin.Height = 500;
            tempwin.Width = 500;

            tempwin.Show();
        }

        public static BitmapSource CreateBitmapSourceFromGdiBitmap(Bitmap bitmap)
        {
            if (bitmap == null) return null;

            var rect = new Rectangle(0 , 0 , bitmap.Width , bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect ,
                ImageLockMode.ReadWrite ,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width ,
                    bitmap.Height ,
                    bitmap.HorizontalResolution ,
                    bitmap.VerticalResolution ,
                    PixelFormats.Bgra32 ,
                    null ,
                    bitmapData.Scan0 ,
                    size ,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        private void TestOverlayImageButton_Click(object sender , RoutedEventArgs e)
        {
            UpdateImage(new System.Drawing.Size(500 , 300));
        }
    }


}
