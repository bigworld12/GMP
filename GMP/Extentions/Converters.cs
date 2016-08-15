using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using GMP.Enums;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows;

namespace GMP.Converters
{
    public class RepeatModeStringConverter : IValueConverter
    {
        public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(RepeatModes)) return "No Repeat";
            var enumval = (RepeatModes)value;
            switch (enumval)
            {
                case RepeatModes.None:
                    return "No Repeat";
                case RepeatModes.SingleSong:
                    return "Single Song";
                case RepeatModes.SingleList:
                    return "Single List";
                case RepeatModes.AllLists:
                    return "All Lists";
                default:
                    return "No Repeat";
            }
        }

        public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(string)) return RepeatModes.None;

            var stringval = value.ToString();
            switch (stringval)
            {
                case "No Repeat":
                    return RepeatModes.None;
                case "Single Song":
                    return RepeatModes.SingleSong;
                case "Single List":
                    return RepeatModes.SingleList;
                case "All Lists":
                    return RepeatModes.AllLists;
                default:
                    return RepeatModes.None;
            }
        }
    }

    public class TooltipValueConverter : IValueConverter
    {
        public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
        {
            var ms = double.Parse(value.ToString());
            var curpos = TimeSpan.FromMilliseconds(ms);
            return $"{((int)curpos.TotalMinutes).ToString("00")}m : {curpos.Seconds.ToString("00")}s";
        }

        public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    public class EnumConverter<T> where T : struct, IConvertible, IFormattable
    {
        private Type EnumType { get; set; }

        public Type UnderlyingType
        {
            get
            {
                return Enum.GetUnderlyingType(EnumType);
            }
        }
        public EnumConverter()
        {
            if (typeof(T).IsEnum)
                EnumType = typeof(T);
            else
                throw new ArgumentException("Provided type must be an enum");
        }
        public IEnumerable<T> ToFlagsList(T FromSingleEnum)
        {
            return FromSingleEnum.ToString()
            .Split(new[] { "," } , StringSplitOptions.RemoveEmptyEntries)
            .Select(
                strenum =>
                {
                    T outenum = default(T);
                    Enum.TryParse(strenum , true , out outenum);
                    return outenum;
                });
        }
        public IEnumerable<T> ToFlagsList(IEnumerable<string> FromStringEnumList)
        {
            return FromStringEnumList
            .Select(
                strenum =>
                {
                    T outenum = default(T);
                    Enum.TryParse(strenum , true , out outenum);
                    return outenum;
                });
        }

        public T ToEnum(string FromString)
        {
            T outenum = default(T);
            Enum.TryParse(FromString , true , out outenum);
            return outenum;
        }
        public T ToEnum(IEnumerable<T> FromListOfEnums)
        {
            var provider = new NumberFormatInfo();
            var intlist = FromListOfEnums.Select(x => x.ToInt32(provider));
            var aggregatedint = intlist.Aggregate((prev , next) => prev | next);
            return (T)Enum.ToObject(EnumType , aggregatedint);
        }
        public T ToEnum(IEnumerable<string> FromListOfString)
        {
            var enumlist = FromListOfString.Select(x =>
            {
                T outenum = default(T);
                Enum.TryParse(x , true , out outenum);
                return outenum;
            });
            return ToEnum(enumlist);
        }

        public string ToString(T FromEnum)
        {
            return FromEnum.ToString();
        }
        public string ToString(IEnumerable<T> FromFlagsList)
        {
            return ToString(ToEnum(FromFlagsList));
        }

        public object ToUnderlyingType(T FromeEnum)
        {
            return Convert.ChangeType(FromeEnum , UnderlyingType);
        }
    }



    public class FalseToVisibleConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">True to collapse, false to hide</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
        {
            //if value = false, return visible
            var val = (bool)value;
            bool? IsCollapse = (bool)parameter;
            if (val)
            {
                if (IsCollapse.HasValue && IsCollapse.Value)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Hidden;
                }
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
        {
            Visibility visib = (Visibility)value;
            //if value = visible, return false
            switch (visib)
            {
                case Visibility.Visible:
                    return false;

                case Visibility.Hidden:
                case Visibility.Collapsed:
                    return true;
                default:
                    return false;
            }
        }
    }
    public class TrueToVisibleConverter : IValueConverter
    {
        public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
        {
            //if visible return true, if not return false
            bool val = (bool)value;
            bool? IsCollapse = (bool)parameter;
            if (val)
            {
                return Visibility.Visible;
            }
            else
            {
                if (IsCollapse.HasValue && IsCollapse.Value)
                {
                    return Visibility.Collapsed;
                }
                else
                {
                    return Visibility.Hidden;
                }
            }


        }

        public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
        {
            Visibility visib = (Visibility)value;
            //if value = visible, return false
            switch (visib)
            {
                case Visibility.Visible:
                    return true;

                case Visibility.Hidden:
                case Visibility.Collapsed:
                    return false;
                default:
                    return true;
            }
        }
    }
    public class EqualsToTrueConverter : IValueConverter
    {
        public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
        {
            return value == parameter;
        }

        public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return parameter;
            }
            else
            {
                return null;
            }
        }
    }

    public class EqualsToVisibleConverter : IValueConverter
    {
        public object Convert(object value , Type targetType , object parameter , CultureInfo culture)
        {
            if (value == parameter)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value , Type targetType , object parameter , CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
