using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GMP.Extentions.Extentions;
using static GMP.Extentions.SupportedExtentions;
namespace GMP.Classes
{
    public class Song : INotifyPropertyChanged
    {
        private MusicObject m_MusicInstance;

        public MusicObject MusicInstance
        {
            get { return m_MusicInstance; }
            set
            {
                m_MusicInstance = value;
                OPC(nameof(MusicInstance));
            }
        }

        public bool SupressPEdit { get; set; }
        public Song(string SPath , MusicObject Instance)
        {
            if (System.IO.File.Exists(SPath))
            {
                MusicInstance = Instance;
                Extension = System.IO.Path.GetExtension(SPath).Replace("." , string.Empty);
                FullPath = SPath;
                DirectoryPath = System.IO.Path.GetDirectoryName(SPath);
                if (IsTagSupported(SPath))
                {
                    TagFile = TagLib.File.Create(SPath);
                    MaxDuration = TagFile.Properties.Duration.TotalMilliseconds;
                    if (!string.IsNullOrWhiteSpace(TagFile.Tag.Title) && !string.IsNullOrWhiteSpace(TagFile.Tag.JoinedAlbumArtists))
                    {
                        SongName = $"{TagFile.Tag.JoinedAlbumArtists} - {TagFile.Tag.Title}";
                    }
                    else
                    {
                        SongName = System.IO.Path.GetFileNameWithoutExtension(SPath);
                    }
                }
                else
                {
                    try
                    {
                        TagFile = TagLib.File.Create(SPath);
                    }
                    catch
                    { }
                    finally
                    {
                        if (TagFile != null)
                        {
                            MaxDuration = TagFile.Properties.Duration.TotalMilliseconds;
                            if (!string.IsNullOrWhiteSpace(TagFile.Tag.Title) && !string.IsNullOrWhiteSpace(TagFile.Tag.JoinedAlbumArtists))
                            {
                                SongName = $"{TagFile.Tag.JoinedAlbumArtists} - {TagFile.Tag.Title}";
                            }
                            else
                            {
                                SongName = System.IO.Path.GetFileNameWithoutExtension(SPath);
                            }
                        }
                        else
                        {
                            MaxDuration = 0;
                            SongName = System.IO.Path.GetFileNameWithoutExtension(SPath);
                        }
                    }
                }
            }
            else
            {
                FlyOuts.LogFlyOut.SendLog(null , $"the path {SPath} isn't valid as a file path" , FlyOuts.LogFlyOut.LogTypes.Error);
            }

        }
        public bool IsTagSupported(string SPath)
        {

            if (MIMETypesDictionary.ContainsKey(System.IO.Path.GetExtension(SPath).Replace('.' , '\0').ToLower()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private TagLib.File m_TagFile;

        public TagLib.File TagFile
        {
            get { return m_TagFile; }
            set
            {
                m_TagFile = value;
                OPC(nameof(TagFile));
            }
        }


        private TimeSpan m_Position;
        public double Position
        {
            get { return m_Position.TotalMilliseconds; }
            set
            {
                m_Position = TimeSpan.FromMilliseconds(value);
                OPC(nameof(Position));
                OPC(nameof(ReadableProgress));
            }
        }

        private string m_Name;
        public string SongName
        {
            get { return m_Name; }
            set
            {
                m_Name = value;
                OPC(nameof(SongName));
                OPC(nameof(ReadableRepresentation));
            }
        }

        private string m_Extension;
        public string Extension
        {
            get { return m_Extension; }
            set
            {
                m_Extension = value;
                OPC(nameof(Extension));
            }
        }

        private string m_DirectoryPath;
        public string DirectoryPath
        {
            get { return m_DirectoryPath; }
            set
            {
                m_DirectoryPath = value;
                OPC(nameof(DirectoryPath));
            }
        }

        private string m_FullPath;
        public string FullPath
        {
            get { return m_FullPath; }
            set
            {
                m_FullPath = value;
                OPC(nameof(FullPath));
            }
        }

        private bool m_IsCurrent;
        public bool IsCurrent
        {
            get { return m_IsCurrent; }
            set
            {
                m_IsCurrent = value;
                OPC(nameof(IsCurrent));
            }
        }

        private bool m_IsFav;
        public bool IsFav
        {
            get { return m_IsFav; }
            set
            {
                m_IsFav = value;
                OPC(nameof(IsFav));
            }
        }
        //perfect = 27
        public string ReadableRepresentation
        {
            get
            {
                return $"{TrimExtraThan(SongName , 27)}  ({FormatDurationMS(m_MaximumDuration)})";
            }
        }

        public string ReadableProgress
        {
            get
            {
                return $"({((int)m_Position.TotalMinutes).ToString("00")}:{(m_Position.Seconds).ToString("00")} / {((int)m_MaximumDuration.TotalMinutes).ToString("00")}:{m_MaximumDuration.Seconds.ToString("00")})";
            }
        }


        private TimeSpan m_MaximumDuration;
        public double MaxDuration
        {
            get { return m_MaximumDuration.TotalMilliseconds; }
            set
            {
                m_MaximumDuration = TimeSpan.FromMilliseconds(value);
                OPC(nameof(MaxDuration));
                OPC(nameof(ReadableRepresentation));
                OPC(nameof(ReadableProgress));
            }
        }

        public static bool operator !=(Song m1 , Song m2)
        {
            return !(m1 == m2);
        }
        public static bool operator ==(Song m1 , Song m2)
        {
            if (ReferenceEquals(m1 , m2))
            {
                return true;
            }
            else
            {
                if (ReferenceEquals(m1 , null) || ReferenceEquals(m2 , null))
                {
                    return false;
                }
                if (!string.IsNullOrWhiteSpace(m1.FullPath) && !string.IsNullOrWhiteSpace(m2.FullPath))
                {
                    return m1.FullPath == m2.FullPath;
                }
                else
                {
                    return false;
                }
            }

        }


        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return FullPath.GetHashCode() + MaxDuration.GetHashCode();
        }
        public void OPC(string name)
        {
            PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(name));
        }
        public event PropertyChangedEventHandler PropertyChanged;

    }

    public class SongEqualityComparer : IEqualityComparer<Song>
    {
        public bool Equals(Song x , Song y)
        {
            return x == y;
        }

        public int GetHashCode(Song obj)
        {
            return obj.GetHashCode();
        }
    }
}
