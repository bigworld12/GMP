using GMP.FlyOuts;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Windows;

namespace GMP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Settings SettingsInstance;


        public static readonly Updater.Version LocalVersion = new Updater.Version(1 , 1 , 0 , 1, Updater.VersionType.Beta);


        App()
        {
            try
            {
                SettingsInstance = new Settings();
            }
            catch (Exception ex)
            {
                LogFlyOut.SendLog(null , ex.ToString() , LogFlyOut.LogTypes.Error);
            }

        }
        public static readonly string EXEFolder = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);

        /// <summary>
        /// Where to save/load settings at/from
        /// </summary>
        public static string SavePath = Path.Combine(EXEFolder , "Settings.GMP");
        public static CommunicationObject CommunicationObj { get; set; }
     
        /// <summary>
        /// Event to indicate When settings are saved
        /// </summary>
        public static event EventHandler<JObject> SettingsSaved;
      
        /// <summary>
        /// Called to signal to subscribers When settings are saved
        /// </summary>
        /// <param name="e"></param>
        public static void RaiseSettingsSaved(JObject e)
        {
            SettingsSaved?.Invoke(Current , e);
        }


        private void Application_Exit(object sender , ExitEventArgs e)
        {
            Settings.Instance.SaveSettings(SavePath);
            CommunicationObj.MusicInstance.Player.Close();
        }

        private void Application_DispatcherUnhandledException(object sender , System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string msg = $"This Error wasn't handeled in dispatcher :\n{e.Exception.ToString()}";
            LogFlyOut.SendLog(null , msg , LogFlyOut.LogTypes.Error);            
            MessageBox.Show(msg);
            e.Handled = true;            
        }
    }
}
