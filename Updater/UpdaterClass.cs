using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.FtpClient;
using System.ComponentModel;
using static Updater.Extentions;
using System.Diagnostics;
using System.Net;
using static Updater.Encryptor;

namespace Updater
{
    public struct Version
    {
        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Build { get; private set; }
        public int Revision { get; private set; }
        public VersionType Type { get; private set; }

        public static Version Null
        {
            get
            {
                return new Version(-1 , -1 , -1 , -1 , VersionType.None);
            }
        }

        public bool IsNull
        {
            get
            {
                return Major == -1
                    && Minor == -1
                    && Build == -1
                    && Revision == -1
                    && Type == VersionType.None;
            }
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Build}.{Revision}-{Type.ToString()}";
        }
        public Version(int maj , int min , int bui , int rev , VersionType typ)
        {
            Major = maj;
            Minor = min;
            Build = bui;
            Revision = rev;

            Type = typ;
        }
        public static bool operator ==(Version obj1 , Version obj2)
        {
            return (obj1.Major == obj2.Major
                && obj1.Minor == obj2.Minor
                && obj1.Build == obj2.Build
                && obj1.Revision == obj2.Revision
                && obj1.Type == obj2.Type);
        }
        public static bool operator !=(Version obj1 , Version obj2)
        {
            return !(obj1 == obj2);
        }

        public static bool operator >(Version obj1 , Version obj2)
        {
            if (obj1 == obj2) return false;

            if (obj1.Type > obj2.Type) return true;
            else if (obj1.Type == obj2.Type)
            {
                if (obj1.Major > obj2.Major)
                {
                    return true;
                }
                else if (obj1.Major == obj2.Major)
                {
                    if (obj1.Minor > obj2.Minor)
                    {
                        return true;
                    }
                    else if (obj1.Minor == obj2.Minor)
                    {
                        if (obj1.Build > obj2.Build)
                        {
                            return true;
                        }
                        else if (obj1.Build == obj2.Build)
                        {
                            if (obj1.Revision > obj2.Revision)
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
            else
            {
                return false;
            }
        }
        public static bool operator <(Version obj1 , Version obj2)
        {
            if (obj1 == obj2) return false;

            return !(obj1 > obj2);
        }

        public static bool operator >=(Version obj1 , Version obj2)
        {
            if (obj1 == obj2) return true;
            return (obj1 > obj2);
        }
        public static bool operator <=(Version obj1 , Version obj2)
        {
            if (obj1 == obj2) return true;
            return (obj1 < obj2);
        }

        public static Version Parse(string strversion)
        {
            var rawlist = strversion.Split(new[] { '-' } , StringSplitOptions.RemoveEmptyEntries);
            var type = rawlist[1];

            VersionType parsedtype;
            Enum.TryParse(type , true , out parsedtype);
            int[] rawvers = rawlist[0].Split(new[] { '.' } , StringSplitOptions.RemoveEmptyEntries).Select(x => { int result; int.TryParse(x , out result); return result; }).ToArray();

            return new Version(rawvers[0] , rawvers[1] , rawvers[2] , rawvers[3] , parsedtype); ;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Version))
            {
                return this == (Version)obj;
            }
            else
            {
                return base.Equals(obj);
            }
        }
        public override int GetHashCode()
        {
            return Major + Minor + Build + Revision;
        }
    }
    public class CheckForUpdateResult
    {
        public CheckForUpdateResult(bool su , FilesList res , Version ver)
        {
            ShouldUpdate = su;
            NewFilesResult = res;
            ServerVersion = ver;
        }
        public bool ShouldUpdate { get; private set; }

        public FilesList NewFilesResult { get; private set; }

        public Version ServerVersion { get; private set; }
    }
    public enum VersionType
    {
        None = 0,
        Alpha = 1,
        Beta = 2,
        Release = 3
    }
    public static class UpdateManager
    {

        /// <summary>
        /// Event to indicate Log
        /// </summary>
        public static event EventHandler<string> LogListener;

        //sometext here

        /// <summary>
        /// Called to signal to subscribers that Log
        /// </summary>
        /// <param name="e"></param>
        static void SendLog(string e)
        {
            LogListener?.Invoke(null , e);
        }

        public static async Task<CheckForUpdateResult> CheckForUpdates(string FTPServer , NetworkCredential creds , int Port , Version LocalVersion , string ServerVersiontxtPath , string ServerProgramFolderSubPath , string LocalProgramFolderSubPath , bool IgnoreVersionChecking = false)
        {
            FilesList resulist = null;
            Version serverversion = Version.Null;
            CheckForUpdateResult finalret;
            finalret = await Task.Run(() =>
            {
                try
                {
                    using (FtpClient client = new FtpClient())
                    {
                        PrepareSSLFTPClient(client);
                        client.Port = Port;
                        client.Host = FTPServer;
                        client.Credentials = creds;
                        client.Connect();


                        bool shouldcheckfiles = false;

                        if (!IgnoreVersionChecking)
                        {
                            if (client.FileExists(ServerVersiontxtPath))
                            {
                                //check for version
                                var filestream = client.OpenRead(ServerVersiontxtPath);
                                string rawversion = ReadtxtFileStream(filestream);
                                serverversion = Version.Parse(rawversion);
                                if (serverversion > LocalVersion == true)
                                {
                                    shouldcheckfiles = true;
                                }
                                else
                                {
                                    shouldcheckfiles = false;
                                    //don't do anything
                                    SendLog($"local version is equal to or less than server version, ignoring update");
                                }
                            }
                            else
                            {
                                //don't check for version,check for files only
                                shouldcheckfiles = true;
                                SendLog($"server version file doesn't exist, checking via files only");
                            }
                        }
                        else
                        {
                            shouldcheckfiles = true;
                        }

                        if (shouldcheckfiles)
                        {
                            //update to server version
                            List<FtpListItem> serverfileslist = new List<FtpListItem>();
                            GetAllFiles(ServerProgramFolderSubPath , client , serverfileslist);
                            var serversfiles = GenerateFileListFromFTPServerFolder(ServerProgramFolderSubPath , serverfileslist , client);
                            var localfiles = GenerateFileListFromLocalFolder(LocalProgramFolderSubPath);
                            resulist = GetNewFiles(serversfiles , localfiles);
                            SendLog($"Received new files to download : {resulist?.ToString()}\nServer List : {serversfiles.ToString()}\nLocal List : {localfiles.ToString()}");
                        }
                    }
                    return new CheckForUpdateResult(resulist != null && resulist.Count > 0 , resulist , serverversion);
                }
                catch (Exception ex)
                {
                    SendLog(ex.ToString());
                    return new CheckForUpdateResult(false , null , Version.Null);
                }
            });
            return finalret;
        }
        private static void PrepareSSLFTPClient(FtpClient client)
        {
            client.EncryptionMode = FtpEncryptionMode.Explicit;
            client.DataConnectionEncryption = true;
            client.ValidateCertificate += Client_ValidateCertificate;
        }
        private static void Client_ValidateCertificate(FtpClient control , FtpSslValidationEventArgs e)
        {
            SendLog($"validating certificate PolicyErrors : {e.PolicyErrors.ToString()}, For Certificate :\nSubject : {e.Certificate.Subject}\nIssur : {e.Certificate.Issuer}\nSerial Number : {e.Certificate.GetSerialNumberString()}");
            e.Accept = true;
        }

        public static void StartUpdating(string FTPServer , string updaterlocation , string localbase , string serverbase , string mode , FilesList listtoupdate , string hashedname , string hashedpw)
        {
            if (!System.IO.File.Exists(updaterlocation))
            {
                SendLog($@"""bwUpdaterTool.exe"" wasn't found at the path : ""{updaterlocation}""");
                return;
            }
            ProcessStartInfo info = new ProcessStartInfo(updaterlocation);
            info.WorkingDirectory = localbase;
            string finalpath = Path.Combine(Path.GetDirectoryName(updaterlocation) , "tempargs.txt");

            System.IO.File.WriteAllLines(finalpath , new[]
            {
                $@"-host_name(=)""{FTPServer}""",
                $@"-local_base(=)""{localbase}""",
                $@"-server_base(=)""{serverbase}""",
                $@"-mode(=)""{mode}""",
                $@"-list(=)""{listtoupdate.ToString()}""",
                $@"-hashed_username(=)""{hashedname}""",
                $@"-hashed_password(=)""{hashedpw}"""
            });

            info.Arguments = finalpath;
            try
            {
                Process.Start(info);
                SendLog($@"""bwUpdaterTool.exe"" Sucessfully opened with arguments :\n{info.Arguments},\nand working directory : {info.WorkingDirectory}");
            }
            catch (Exception ex)
            {
                SendLog($@"This error was thrown while starting bwUpdaterTool\n{ex.ToString()}");
                return;
            }
        }
    }
}
