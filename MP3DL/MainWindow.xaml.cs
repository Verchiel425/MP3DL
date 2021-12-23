using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MP3DL.Libraries;

namespace MP3DL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            QueueDataGrid.ItemsSource = QueueBindingList;
            musiclist.ItemsSource = MusicList;

            waiter.Interval = TimeSpan.FromMilliseconds(500);
            waiter.Tick += Waiter_Tick;
            delivery.Interval = TimeSpan.FromSeconds(1);
            delivery.Tick += Delivery_Tick;
            scanner.DoWork += Scanner_DoWork;
            scanner.WorkerReportsProgress = true;
            scanner.ProgressChanged += Scanner_ProgressChanged;
            scanner.RunWorkerCompleted += Scanner_RunWorkerCompleted;

            spotify.PlaylistFetchingProgressChanged += Spotify_PlaylistFetchingProgressChanged;
            spotify.PlaylistFetchingDone += Spotify_PlaylistFetchingDone;

            downloader.DownloadCompleted += DownloadCompleted;
            downloader.ProgressChanged += DownloadProgressChanged;

            mplayer.ResumedPlayback += ResumedPlayback;
            mplayer.StoppedPlayback += StoppedPlayback;
            mplayer.PlaybackFinished += PlaybackFinished;
            mplayer.PlaybackPositionChanged += PlaybackPositionChanged;

            QueueBindingList.ListChanged += QueueBindingList_ListChanged;
        }
    }
}