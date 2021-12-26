using System;
using System.Windows;

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
            MusicDataGrid.ItemsSource = MusicBindingList;

            waiter.Interval = TimeSpan.FromMilliseconds(500);
            waiter.Tick += Waiter_Tick;
            delivery.Interval = TimeSpan.FromSeconds(1);
            delivery.Tick += Delivery_Tick;
            scanner.DoWork += Scan;
            scanner.RunWorkerCompleted += ScanComplete;

            spotify.CollectionFetchingProgressChanged += Spotify_PlaylistFetchingProgressChanged;
            spotify.CollectionFetchingDone += Spotify_PlaylistFetchingDone;

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