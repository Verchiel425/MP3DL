using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MP3DL.Libraries;

namespace MP3DL
{
    public partial class MainWindow : Window
    {
        #region scanner
        BackgroundWorker scanner = new();
        private void Scanner_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            temp = (List<MP3File>)e.Result;
            temp.Sort(delegate (MP3File x, MP3File y)
            {
                if (x.DateAdded == null && y.DateAdded == null) return 0;
                else if (x.DateAdded == null) return -1;
                else if (y.DateAdded == null) return 1;
                else return x.DateAdded.CompareTo(y.DateAdded);
            });
            Debug.WriteLine("--Finished scanning--");

            if (MusicTabInitialized)
            {
                musictabloading.Visibility = Visibility.Visible;
                delivery.Start();
            }
        }

        private void Scanner_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Scanner_DoWork(object? sender, DoWorkEventArgs e)
        {
            List<string> directories = (List<string>)e.Argument;
            var tmp = new ConcurrentBag<MP3File>();

            Parallel.ForEach(directories, directory =>
            {
                foreach(var file in ProcessDirectory(directory))
                {
                    if (file.EndsWith(".mp3"))
                    {
                        var music = new MP3File(file);
                        if (!tmp.Contains(music))
                        {
                            tmp.Add(music);
                        }
                    }
                }
            });

            e.Result = tmp.ToList();
        }
        private List<string> ProcessDirectory(string directory)
        {
            string[] subdirectories = Directory.GetDirectories(directory);
            List<string> files = Directory.GetFiles(directory).ToList();

            foreach(string subdirectory in subdirectories)
            {
                files.AddRange(ProcessDirectory(subdirectory));
            }
            return files;
        }
        #endregion
    }
}
