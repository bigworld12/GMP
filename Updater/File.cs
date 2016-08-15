using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.FtpClient;
using System.Text;
using System.Threading.Tasks;
using static Updater.Extentions;

namespace Updater
{
    public struct File
    {
        public string Name { get; set; }
        public string BaseDirectory { get; set; }

        /// <summary>
        /// relative location on server from base directory
        /// </summary>
        public string LocationFromBase { get; set; }

        /// <summary>
        /// In Bytes
        /// </summary>
        public long Size { get; set; }

        public string ReadableSize
        {
            get
            {
                return GetReadableSize(Size);
            }
        }

        public override string ToString()
        {
            return $@"{LocationFromBase}(,){Size}";
        }        
    }


    public class FilesList : List<File>
    {
        public static FilesList FromIEnumerable(IEnumerable<File> source)
        {
            FilesList final = new FilesList();
            final.AddRange(source);
            return final;
        }

        /// <summary>
        /// gets a string for all the items in the list
        /// </summary>
        /// <returns>the items in the list separated by '(;)'</returns>
        public override string ToString()
        {
            return string.Join(@"(;)" , this);
        }
    }

}
