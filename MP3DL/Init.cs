using MP3DL.Libraries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace MP3DL
{
    public partial class MainWindow : Window
    {
        #region Timers
        private void Delivery_Tick(object? sender, EventArgs e)
        {
            delivery.Stop();
            foreach (var music in temp)
            {
                if (!MusicList.Contains(music))
                {
                    MusicList.Add(music);
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
                await spotify.SetCurrentTrack(await Search_PlainText());
                AudioVideoToggle.Visibility = Visibility.Hidden;

                UpdatePreview(spotify.CurrentTrack);
            }
            catch (ArgumentException)
            {
                AudioVideoToggle.Visibility = Visibility.Hidden;
                ClearPreview();
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
                    shufflebutton.Content = "Shuffle: On";
                }
                else
                {
                    shufflebutton.Content = "Shuffle: Off";
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
                    autoplaybutton.Content = "AutoPlay: On";
                }
                else
                {
                    autoplaybutton.Content = "AutoPlay: Off";
                }
                Properties.Settings.Default.AutoPlay = value;
            }
        }
        #endregion

        Downloader downloader = new();
        Spotify spotify = new Spotify();
        YouTube youtube = new();
        MusicPlayer mplayer = new();
        CancellationTokenSource cts = new();
        readonly UserFolders client = new UserFolders();
        readonly DispatcherTimer waiter = new();
        readonly DispatcherTimer delivery = new();
        readonly BindingList<IMedia> QueueBindingList = new();
        readonly BindingList<MP3File> MusicList = new();
        public List<string> _directories = new();
        public List<string> directories
        {
            get { return _directories; }
            set
            {
                _directories = value;
                client.MUSICFOLDERS = value;
                client.Save();
            }
        }
        private List<MP3File> PlayedSongs = new();
        List<MP3File> temp = new();



        private enum SortType
        {
            Title,
            Artists,
            Album,
            Year,
            Recency
        }
        private enum PlaybackType
        {
            Preview,
            FromFile
        }
        private PlaybackType Playback { get; set; }
        private bool MOUSEOVERMENU { get; set; } = false;
        private bool AUDIOONLY { get; set; } = true;
        private bool MusicTabInitialized = false;
        private int CurrentIndexInPlayedSongs;
        private bool DoNotAdd = false;
        private bool InsertFromStart = false;
        private object? PreviewItem;
    }
}
