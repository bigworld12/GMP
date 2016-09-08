using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using GMP.Classes;
using static GMP.Extentions.Extentions;
using System.Windows.Input;

namespace GMP
{
    public class Settings : INotifyPropertyChanged
    {
        public Settings()
        {
            Instance = this;

            //read default settings
            var assembly = Assembly.GetExecutingAssembly();
            var recname = string.Empty;
            var names = assembly.GetManifestResourceNames().ToList();
            recname = names.Find(x => x.ToLower().Contains("settingsbase"));

            if (!string.IsNullOrWhiteSpace(recname))
            {
                using (Stream stream = assembly.GetManifestResourceStream(recname))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    m_DefaultSettings = JObject.Parse(result);
                }
            }
            else
            {
                m_DefaultSettings = null;
            }
            OPC(nameof(DefaultSettings));
            OPC(nameof(Instance));
        }
        public static Settings Instance { get; private set; }

        private JObject m_ToJson;

        public JObject ToJson
        {
            get { return m_ToJson; }
            set
            {
                m_ToJson = value;
                OPC(nameof(ToJson));
            }
        }

        public void SaveSettings(string savepath)
        {
            UpdateSettings();

            File.WriteAllText(savepath , ToJson.ToString());
            App.RaiseSettingsSaved(ToJson);
        }
        public void LoadSettings(string savepath)
        {
            try
            {
                if (File.Exists(savepath))
                {
                    ToJson = JObject.Parse(File.ReadAllText(savepath));
                }
                else
                {
                    ToJson = DefaultSettings;
                }
                App.CommunicationObj.MainWindowInstance.LoadSettingsIntoGUI();
            }
            catch (Exception ex)
            {
                FlyOuts.LogFlyOut.SendLog(null , $"This error happened when loading the settings\nException :\n{ex.ToString()}" , FlyOuts.LogFlyOut.LogTypes.Error);
                ToJson = DefaultSettings;
            }


        }
        private JObject m_DefaultSettings;
        public JObject DefaultSettings
        {
            get
            {
                return m_DefaultSettings;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void UpdateSettings()
        {
            var mu = App.CommunicationObj.MusicInstance;
            var mw = App.CommunicationObj.MainWindowInstance;

            if (mu.Player.CurrentSong == null)
            {
                ToJson["CurrentPlayListIndex"] = -1;
                ToJson["CurrentSongIndex"] = -1;
            }
            else
            {
                ToJson["CurrentPlayListIndex"] = mu.PlayLists.IndexOf(mu.Player.CurrentPlayList);
                ToJson["CurrentSongIndex"] = mu.Player.CurrentPlayList.IndexOf(mu.Player.CurrentPlayList.CurrentSong);
            }


            ToJson["Volume"] = mu.Player.Volume;
            ToJson["Repeat Mode"] = mu.RepeatMode.ToString();


            var HKArray = new JArray();
            for (int i = 0; i < MainWindow.HKDict.Count; i++)
            {
                var dictitem = MainWindow.HKDict.ElementAt(i);
                var x = new JObject();
                x["name"] = dictitem.Value;
                var hotk = mw.GetValueFromPropName(dictitem.Key);
                x["key"] = hotk.Key.ToString();

                var modenum = hotk.ModifierKeys;

                var modstrlist = hotk.ModifierKeys.ToString().Split(new[] { "," } , StringSplitOptions.RemoveEmptyEntries).Select(strenum => { ModifierKeys outenum; Enum.TryParse(strenum , out outenum); return outenum.ToString(); });
                x["mod"] = new JArray(modstrlist);
                HKArray.Add(x);
            }
            ToJson["HotKeys"] = HKArray;



            var PLArray = new JArray();
            foreach (PlayList pl in mu.PlayLists)
            {
                var jpl = new JObject();
                jpl["name"] = pl.Name;

                var jsongs = new JArray();
                foreach (Song s in pl)
                {
                    var js = new JObject();
                    js["path"] = s.FullPath;
                    js["isfav"] = s.IsFav;
                    js["isuserange"] = s.IsUseStartEndRange;
                    js["startpos"] = s.GetActualStartPos();
                    var actend = s.GetActualEndPos();
                    if (actend <= -1)
                    {
                        js["endpos"] = s.MaxDuration;
                    }
                    else
                    {
                        js["endpos"] = actend;
                    }

                    jsongs.Add(js);
                }
                jpl["songs"] = jsongs;
                PLArray.Add(jpl);
            }
            ToJson["playlists"] = PLArray;
        }
        public void OPC(string propname)
        { PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propname)); }
    }
}
