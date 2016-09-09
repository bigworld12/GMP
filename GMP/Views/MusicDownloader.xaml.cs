using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using GMP.ViewModel;
using Microsoft.Win32;
using GMP.Classes;
using static GMP.Extentions.Extentions;
using System.IO;

namespace GMP.Views
{
    /// <summary>
    /// Interaction logic for MusicDownloader.xaml
    /// </summary>
    public partial class MusicDownloader
    {
        public MusicDownloaderViewModel ViewModel
        {
            get { return (MusicDownloaderViewModel)TryFindResource("ViewModel"); }
        }



        public MusicDownloader()
        {
            InitializeComponent();
            /*
                    if (j["Latest Save Path"] != null)
                    {
                        var savepath = j.GV<string>("Folder Save Path");
                        DownloaderWindow.ViewModel.SaveDiagInstance.SelectedPath = savepath;
                    }
            */
            if (Settings.Instance.ToJson != null)
            {
                var savedjson = Settings.Instance.ToJson.GV<string>("Latest Save Path");
                if (Directory.Exists(savedjson))
                {
                    ViewModel.WantedSaveFolder = savedjson;
                }
            }
        }

        private void DownloadMusicButton_Click(object sender , RoutedEventArgs e)
        {
            ViewModel.DownloadCurrentVideo(ViewModel.SavePath);
        }

        private void SaveAsButton_Click(object sender , RoutedEventArgs e)
        {
            var x = ViewModel.SaveDiagInstance.ShowDialog();
            if (x == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.OPC(nameof(ViewModel.WantedSaveFolder));
                Settings.Instance.ToJson["Latest Save Path"] = ViewModel.WantedSaveFolder;
                Settings.Instance.SaveSettings(App.SavePath);
            }
        }

        private void ParseButton_Click(object sender , RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ViewModel.URL))
            {
                ViewModel.ParseVideo(ViewModel.URL);
            }
        }

        private void DownloadMusicToPlayListButton_Click(object sender , RoutedEventArgs e)
        {

            ViewModel.DownloadCurrentVideo(ViewModel.SavePath , (PlayList)PlayListsCombobox.SelectedItem);


        }

        private void DownloaderWindow_Closing(object sender , System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Visibility = Visibility.Collapsed;
        }
    }
}
