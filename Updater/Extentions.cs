using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.FtpClient;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Diagnostics;

namespace Updater
{
    public static class Extentions
    {
        public static string GetReadableSize(double sizeinbytes)
        {
            if (sizeinbytes == 0) return "0 KB";

            string measure;
            double finalsize;
            if (sizeinbytes >= Math.Pow(1024 , 3))
            {
                finalsize = sizeinbytes / Math.Pow(1024 , 3);
                measure = "GB";
            }
            else
            {
                if (sizeinbytes >= Math.Pow(1024 , 2))
                {
                    finalsize = sizeinbytes / Math.Pow(1024 , 2);
                    measure = "MB";
                }
                else
                {
                    finalsize = sizeinbytes / 1024;
                    measure = "KB";
                }
            }
            return $"{finalsize.ToString("0.00")} {measure}";
        }
        public static string ReadtxtFileStream(Stream s)
        {
            using (StreamReader sr = new StreamReader(s))
            {
                return sr.ReadToEnd();
            }
        }

        public static FilesList GenerateFileListFromLocalFolder(string BasePath)
        {
            var final = new FilesList();
            try
            {
                var rawfileslist = Directory.GetFiles(BasePath , "*.*" , SearchOption.AllDirectories);

                Stopwatch streamperfcalc = new Stopwatch();
                streamperfcalc.Start();
                foreach (string rawfile in rawfileslist)
                {

                    File toadd = new File();

                    var fileinf = new FileInfo(rawfile);
                    toadd.BaseDirectory = BasePath;
                    toadd.LocationFromBase = rawfile.Replace(BasePath , string.Empty);
                    toadd.Name = Path.GetFileName(rawfile);
                    toadd.Size = fileinf.Length;

                    final.Add(toadd);
                }
                streamperfcalc.Stop();
                Console.WriteLine($"it took {streamperfcalc.Elapsed.ToString()} to finish generating local files list");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return final;
        }
        public static FilesList GenerateFileListFromFTPServerFolder(string BasePath , IEnumerable<FtpListItem> files , FtpClient client)
        {
            var final = new FilesList();
            try
            {
                Stopwatch streamperfcalc = new Stopwatch();
                streamperfcalc.Start();
                foreach (var item in files)
                {
                    var toadd = new File();
                    toadd.BaseDirectory = BasePath;
                    toadd.LocationFromBase = item.FullName.Replace(BasePath , string.Empty);
                    toadd.Name = item.Name;
                    toadd.Size = item.Size;

                    final.Add(toadd);
                }
                streamperfcalc.Stop();
                Console.WriteLine($"it took {streamperfcalc.Elapsed.ToString()} to finish generating server files list");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return final;
        }
        public static void GetAllFiles(string Directory , FtpClient client , List<FtpListItem> final)
        {
            try
            {
                var listofcurdirectory = client.GetListing(Directory);
                foreach (var item in listofcurdirectory)
                {
                    if (item.Type == FtpFileSystemObjectType.File)
                    {
                        final.Add(item);
                    }
                    else if (item.Type == FtpFileSystemObjectType.Directory)
                    {
                        GetAllFiles(item.FullName , client , final);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static FilesList GetNewFiles(FilesList ServersList , FilesList LocalList)
        {
            FilesList final = new FilesList();

            foreach (File serverfile in ServersList)
            {
                File? EquiLocalFile = LocalList.Find(x => x.LocationFromBase.TrimStart('\\' , '/') == serverfile.LocationFromBase.TrimStart('\\' , '/'));
                Debug.Assert(EquiLocalFile.HasValue , $"Found a server that has no equi value : {serverfile.LocationFromBase}");
                if (EquiLocalFile.HasValue)
                {
                    if (EquiLocalFile.Value.Size != serverfile.Size)
                    {
                        final.Add(serverfile);
                    }
                }
                else
                {
                    //no equi local file found
                    final.Add(serverfile);
                }
            }
            return final;
        }

        public static void SaveStreamToFile(string fileFullPath , Stream stream)
        {
            try
            {
                if (stream.Length == 0) return;

                Directory.CreateDirectory(Path.GetDirectoryName(fileFullPath));


                // Create a FileStream object to write a stream to a file
                using (FileStream fs = System.IO.File.Create(fileFullPath))
                {
                    if (fs.CanWrite)
                    {
                        stream.Seek(0 , SeekOrigin.Begin);
                        stream.CopyTo(fs);
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }       
    }
}
