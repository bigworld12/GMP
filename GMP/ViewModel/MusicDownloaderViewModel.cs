using GMP.Classes;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using VideoLibrary;


namespace GMP.ViewModel
{
    public class MusicDownloaderViewModel : ViewModel
    {
        private string m_WantedFileName;
        /// <summary>
        /// without extension
        /// </summary>
        public string WantedFileName
        {
            get { return m_WantedFileName; }
            set
            {
                m_WantedFileName = RemoveFileNameSpecialCharacters(value);
                OPC(nameof(WantedFileName));
            }
        }
        public string WantedSaveFolder
        {
            get { return SaveDiagInstance.SelectedPath; }
            set { SaveDiagInstance.SelectedPath = value;
                OPC(nameof(WantedSaveFolder));
            }
        }



        private string m_CurrentState = "Nothing";
        public string CurrentState
        {
            get { return m_CurrentState; }
            set
            {
                m_CurrentState = value;
                OPC(nameof(CurrentState));
            }
        }

        private bool m_IsParsing;
        public bool IsParsing
        {
            get { return m_IsParsing; }
            set
            {
                IsCanDownload = !value;
                IsCanParse = !value;
                if (value == true)
                {
                    CurrentState = "Parsing Video ...";
                }
                else if (m_IsParsing == true && value == false)
                {
                    //Finished Parsing (from true to false)
                    CurrentState = "Finished Parsing the video, you can start downloading";
                }
                m_IsParsing = value;
                OPC(nameof(IsParsing));
                OPC(nameof(IsDoingProgress));
            }
        }

        private bool m_IsDownloading;
        public bool IsDownloading
        {
            get { return m_IsDownloading; }
            set
            {

                IsCanDownload = !value;
                if (value == true)
                {
                    IsCanDownload = false;
                    IsCanParse = false;
                    CurrentState = "Downloading ...";
                    //started downloading
                }
                else if (m_IsDownloading == true && value == false)
                {
                    //Finished Downloading 
                    IsCanParse = true;
                    CurrentState = $"Finished Downloading , Saved to :\n{SavePath}";
                }
                m_IsDownloading = value;
                OPC(nameof(IsDownloading));
                OPC(nameof(IsDoingProgress));
            }
        }

        public bool IsDoingProgress
        {
            get
            {
                return IsDownloading || IsParsing;
            }
        }

        private bool m_IsCanDownload;
        public bool IsCanDownload
        {
            get
            {
                return m_IsCanDownload;
            }
            set
            {
                m_IsCanDownload = value;
                OPC(nameof(IsCanDownload));
            }
        }


        private bool m_IsCanParse = true;
        public bool IsCanParse
        {
            get { return m_IsCanParse; }
            set
            {
                m_IsCanParse = value;
                OPC(nameof(IsCanParse));
            }
        }



        public string WantedExtension
        {
            get
            {
                if (CurrentVideo == null) return ".mp4";
                else return CurrentVideo.FileExtension;
            }
        }

        private YouTubeVideo m_CurrentVideo;

        public YouTubeVideo CurrentVideo
        {
            get { return m_CurrentVideo; }
            set
            {
                m_CurrentVideo = value;
                OPC(nameof(CurrentVideo));
                OPC(nameof(WantedExtension));
                if (value == null)
                {
                    WantedFileName = null;
                }
                else
                {
                    WantedFileName = value.Title.Substring(0 , value.Title.LastIndexOf('-') - 1);
                }
            }
        }
        YouTube _def;

        private string m_URLPath;
        public string URL
        {
            get { return m_URLPath; }
            set
            {
                m_URLPath = value;
                OPC(nameof(URL));
                if (value == null)
                {
                    IsCanDownload = false;
                    IsCanParse = false;
                }
                else
                {
                    IsCanParse = true;
                }
                //check if the url is valid and parse it
            }
        }

        public string SavePath
        {
            get
            {
                if (CurrentVideo == null)
                    return null;
                else
                    return Path.Combine(WantedSaveFolder , Path.ChangeExtension(WantedFileName , CurrentVideo.FileExtension));
            }
        }





        public System.Windows.Forms.FolderBrowserDialog SaveDiagInstance;

        public MusicDownloaderViewModel()
        {
            _def = YouTube.Default;
            SaveDiagInstance = new System.Windows.Forms.FolderBrowserDialog();
            SaveDiagInstance.Description = "Please select where to save";
            SaveDiagInstance.RootFolder = Environment.SpecialFolder.MyComputer;
            SaveDiagInstance.ShowNewFolderButton = true;
        }


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

        public void ParseVideo(string From)
        {
            if (IsParsing) return;
            IsParsing = true;
            Task.Run(() =>
            {
                try
                {
                    var yt = _def;
                    var vid = yt.GetAllVideos(From);
                    CurrentVideo = vid.Where(x => (x.AudioFormat == AudioFormat.Aac || x.AudioFormat == AudioFormat.Mp3) && x.AdaptiveKind == AdaptiveKind.Audio).OrderByDescending(x => x.AudioBitrate).First();
                    IsParsing = false;
                }
                catch (Exception ex)
                {
                    IsParsing = false;
                    CurrentState = "Failed to Parsed the Video,See the Log";
                    FlyOuts.LogFlyOut.SendLog(null , $"Error happened when parsing video : {From}\nException :\n{ex.ToString()}" , FlyOuts.LogFlyOut.LogTypes.Error);
                }
            });
        }
        public static string RemoveFileNameSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')
                    || c == '.' || c == '_' || c == ' ' || c == '-')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public void DownloadCurrentVideo(string To , PlayList toadd = null)
        {
            if (IsDownloading) return;
            if (CurrentVideo == null) return;
            Task.Run(() =>
            {
                CurrentState = "Downloading ...";

                IsDownloading = true;
                try
                {
                    using (VideoClient vc = new VideoClient())
                    using (var stream = vc.Stream(CurrentVideo))
                    using (var fs = new FileStream(To , FileMode.Create))
                    {
                        stream.CopyTo(fs);
                    }
                    if (toadd != null)
                    {
                        Console.WriteLine($"Adding Song : {To}");
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var song = new Song(To , Music);
                            toadd.Add(song);
                            FlyOuts.LogFlyOut.SendLog(null , $"Added 1 Song" , FlyOuts.LogFlyOut.LogTypes.Normal);
                        });

                    }
                    CurrentState = $"Finished Downloading , Saved File To :\n{To}";
                }
                catch (Exception ex)
                {
                    CurrentState = "Error Occured, See the Log For more Info";
                    FlyOuts.LogFlyOut.SendLog(null , $"An Error Occured When Downloading File To : {To}\nExpetion :\n{ex.ToString()}" , FlyOuts.LogFlyOut.LogTypes.Error);
                }
                IsDownloading = false;

            });
        }
    }
}
