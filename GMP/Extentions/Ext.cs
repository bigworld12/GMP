﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GMP.Extentions
{
    public static class Extentions
    {
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
    }
}