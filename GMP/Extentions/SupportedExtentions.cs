using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMP.Extentions
{
    public static class SupportedExtentions
    {
        public static readonly Dictionary<string , string> MIMETypesDictionary = new Dictionary<string , string>
        {
            { "aif", "audio/x-aiff"},
            { "aifc", "audio/x-aiff"},
            { "aiff", "audio/x-aiff"},
            { "au", "audio/basic"},
            { "kar", "audio/midi"},
            { "m3u", "audio/x-mpegurl"},
            { "m4a", "audio/mp4a-latm"},
            { "m4b", "audio/mp4a-latm"},
            { "m4p", "audio/mp4a-latm"},
            { "mid", "audio/midi"},
            { "midi", "audio/midi"},
            { "mp2", "audio/mpeg"},
            { "mp3", "audio/mpeg"},
            { "mpga", "audio/mpeg"},
            { "ra", "audio/x-pn-realaudio"},
            { "ram", "audio/x-pn-realaudio"},
            { "snd", "audio/basic"},
            { "wav", "audio/x-wav"},
            { "avi", "video/x-msvideo"},
            { "dif", "video/x-dv"},
            { "dv", "video/x-dv"},
            { "m4u", "video/vnd.mpegurl"},
            { "m4v", "video/x-m4v"},
            { "mov", "video/quicktime"},
            { "movie", "video/x-sgi-movie"},
            { "mp4", "video/mp4"},
            { "mpe", "video/mpeg"},
            { "mpeg", "video/mpeg"},
            { "mpg", "video/mpeg"},
            { "mxu", "video/vnd.mpegurl"},
            { "qt", "video/quicktime"}};
    }
}
