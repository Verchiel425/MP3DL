using MP3DL.Media;
using MP3DL.Media.Audio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

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
        #region Timers
        private void Delivery_Tick(object? sender, EventArgs e)
        {
            delivery.Stop();
            foreach (var music in temp)
            {
                if (!MusicBindingList.Contains(music))
                {
                    MusicBindingList.Add(music);
                }
            }
            musictabloading.Visibility = Visibility.Hidden;
            if (!MusicTabInitialized)
            {
                FadeTab();
                MusicTabInitialized = true;
            }

            musiccount.Text = $"{temp.Count} song(s)";
        }
        private async void Waiter_Tick(object? sender, EventArgs e)
        {
            waiter.Stop();

            try
            {
                MediaInput = null;
                await spotify.SetCurrentTrack(await Search_PlainText());
                FormatToggle.Visibility = Visibility.Hidden;

                UpdatePreview(spotify.CurrentTrack, InputFrom.User);
            }
            catch (ArgumentException)
            {
                FormatToggle.Visibility = Visibility.Hidden;
                ClearPreview(InputFrom.User);
            }
        }
        #endregion
        #region SETTINGS
        private bool _AUTOAUTH;
        private bool _AUTOPLAY;
        private bool _BASSBOOST;
        private string _OUTPUT;
        private bool _SHUFFLE;
        private string OUTPUT
        {
            get { return _OUTPUT; }
            set
            {
                _OUTPUT = value;
                OutputFolderTextbox.Text = value;
                downloader.Output = OUTPUT;
                Properties.Settings.Default.OutputDir = value;
            }
        }
        private bool AUTOAUTH
        {
            get { return _AUTOAUTH; }
            set
            {
                _AUTOAUTH = value;
                AutoAuthCheckbox.IsChecked = value;
                Properties.Settings.Default.AutoAuth = value;
            }
        }
        private bool BASSBOOST
        {
            get { return _BASSBOOST; }
            set
            {
                _BASSBOOST = value;
                BassBoostCheckbox.IsChecked = value;
                Properties.Settings.Default.BassBoost = value;
            }
        }
        private bool SHUFFLE
        {
            get { return _SHUFFLE; }
            set
            {
                _SHUFFLE = value;
                if (value == true)
                {
                    ShuffleButton.Content = "Shuffle: On";
                }
                else
                {
                    ShuffleButton.Content = "Shuffle: Off";
                }
                Properties.Settings.Default.Shuffle = value;
            }
        }
        private bool AUTOPLAY
        {
            get { return _AUTOPLAY; }
            set
            {
                _AUTOPLAY = value;
                if (value == true)
                {
                    AutoPlayButton.Content = "AutoPlay: On";
                }
                else
                {
                    AutoPlayButton.Content = "AutoPlay: Off";
                }
                Properties.Settings.Default.AutoPlay = value;
            }
        }
        #endregion

        readonly Downloader downloader = new();
        readonly Spotify spotify = new Spotify();
        readonly YouTube youtube = new();
        readonly MusicPlayer mplayer = new();
        readonly UserFolders client = new UserFolders();
        readonly DispatcherTimer waiter = new();
        readonly DispatcherTimer delivery = new();
        readonly BindingList<IMedia> QueueBindingList = new();
        readonly BindingList<MP3File> MusicBindingList = new();
        public List<string> _MusicFolders = new();
        public List<string> MusicFolders
        {
            get { return _MusicFolders; }
            set
            {
                _MusicFolders = value;
                client.MUSICFOLDERS = value;
                client.Save();
            }
        }
        private NavigatableList<MP3File> PlayedSongs = new();
        List<MP3File> temp = new();
        CancellationTokenSource cts = new();

        private PlaybackType Playback { get; set; }
        private InputFrom InputType { get; set; } = InputFrom.User;
        private bool MOUSEOVERMENU { get; set; } = false;
        private bool MusicTabInitialized = false;
        private object? MediaInput;
        private object? CurrentlyPreviewing;
    }
}