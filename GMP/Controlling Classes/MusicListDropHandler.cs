using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GongSolutions.Wpf.DragDrop;
using System.Windows;
using GMP.Classes;
using GMP.ViewModel;
using System.IO;
using GMP.Extentions;

namespace GMP.Controlling_Classes
{
    public class MusicListDropHandler : IDropTarget
    {
        public MainViewModel ViewModel { get; set; }

        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = typeof(DropTargetInsertionAdorner);

            if (dropInfo.Data is DataObject)
            {
                DataObject dat = dropInfo.Data as DataObject;
                
                dropInfo.Effects = DragDropEffects.Copy | DragDropEffects.Scroll;
            }
            else if (dropInfo.Data is IEnumerable<Song> || dropInfo.Data is Song)
            {
                var dataAsList = DefaultDropHandler.ExtractData(dropInfo.Data).Cast<Song>().ToList();
                dropInfo.Effects = DragDropEffects.Move;
            }         
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is DataObject)
            {
                DataObject dat = dropInfo.Data as DataObject;
                if (dat.ContainsFileDropList())
                {
                    var files = dat.GetFileDropList();
                    int count = 0;
                    foreach (var item in files)
                    {
                        if (File.Exists(item))
                        {
                            Song news = new Song(item , ViewModel.Music);
                            ViewModel.UserSelectedPlayList.Insert(dropInfo.InsertIndex , news);
                            count += 1;
                        }
                        else if (Directory.Exists(item))
                        {
                            IninstCounter counter = new IninstCounter();
                            ViewModel.AddDirectory(item , counter);
                        }                      
                    }
                    if (count > 0)
                        FlyOuts.LogFlyOut.SendLog(null , $"Added {count} Songs", FlyOuts.LogFlyOut.LogTypes.Normal);
                }              
            }
            else if (dropInfo.Data is IEnumerable<Song> || dropInfo.Data is Song)
            {
                
                var dataAsList = DefaultDropHandler.ExtractData(dropInfo.Data).Cast<Song>().ToList();

                for (int i = dataAsList.Count - 1; i >= 0; i--)
                {
                    Song item = dataAsList[i];                    
                    if (dropInfo.DragInfo.SourceIndex > dropInfo.InsertIndex)
                    {
                        //from bottom to top
                        ViewModel.UserSelectedPlayList.Move(ViewModel.UserSelectedPlayList.IndexOf(item) , dropInfo.InsertIndex);
                    }
                    else
                    {
                        //from top to bottom                        
                        ViewModel.UserSelectedPlayList.Move(ViewModel.UserSelectedPlayList.IndexOf(item) , dropInfo.InsertIndex - 1);
                    }                    
                }               
            }
        }
    }
}
