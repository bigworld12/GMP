using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GMP.Extentions
{
    public static class Extentions
    {
        public static T GV<T>(this JToken from , string name)
        {
            if (from == null) return default(T);
            else
            {
                if (from[name] == null)
                {
                    return default(T);
                }
                else
                {
                    return from[name].ToObject<T>();
                }
                
            }
        }
        public static T GV<T>(this JToken from , params string[] names)
        {
            if (from == null) return default(T);
            else
            {
                JToken start = from;
                foreach (var item in names)
                {
                    if (start == null || start[item] == null)
                    {
                        return default(T);
                    }
                    else
                    {
                        start = start[item];
                    }                    
                }
                return start.ToObject<T>();
            }
        }

        public static string FormatDurationMS(TimeSpan duration)
        {
            return ($"{((int)duration.TotalMinutes).ToString("00")}m : {duration.Seconds.ToString("00")}s");
        }
        public static string TrimExtraThan(string trimmable , int maxcharcount)
        {
            if (trimmable.Length > maxcharcount)
                return trimmable.Remove(maxcharcount , trimmable.Length - maxcharcount) + "...";
            else
            {
                return trimmable;
            }
        }

        public static Enum Or(Enum a , Enum b)
        {
            // consider adding argument validation here

            if (Enum.GetUnderlyingType(a.GetType()) != typeof(ulong))
                return (Enum)Enum.ToObject(a.GetType() , Convert.ToInt64(a) | Convert.ToInt64(b));
            else
                return (Enum)Enum.ToObject(a.GetType() , Convert.ToUInt64(a) | Convert.ToUInt64(b));
        }

        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(this DependencyObject parent , string childName)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            var children = LogicalTreeHelper.GetChildren(parent).Cast<DependencyObject>();
            foreach (var child in children)
            {
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child , childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }
    }
    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object , bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute , Func<object , bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
    public class IninstCounter
    {
        public int Count { get; set; }
        public override string ToString()
        {
            return Count.ToString();
        }
    }
}
