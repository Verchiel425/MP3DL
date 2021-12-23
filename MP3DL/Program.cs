using MP3DL.Libraries;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace MP3DL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private async void Authenticate()
        {
            AuthStatusLabel.Content = "PLEASE WAIT...";
            AuthProgressRing.Visibility = Visibility.Visible;
            string ID = ClientIDTextbox.Password;
            string SECRET = ClientSecretTextbox.Password;

            spotify.ClientID = ID;
            spotify.ClientSecret = SECRET;
            await Task.Run(() => spotify.Auth());

            Debug.WriteLine("--Auth Attempt Complete!--");
            if (spotify.Authd)
            {
                AuthMenuItem.Source = new BitmapImage(new Uri("resources\\icon_unlock.png", UriKind.Relative));
                Properties.Settings.Default.ClientID = cryptography.Encrypt(ID);
                Properties.Settings.Default.ClientSecret = cryptography.Encrypt(SECRET);
                LinkTextbox.IsEnabled = true;
                DownloadControlEnabled(true);
                AuthStatusLabel.Content = "AUTHENTICATED";
            }
            else
            {
                AuthMenuItem.Source = new BitmapImage(new Uri("resources\\icon_lock.png", UriKind.Relative));
                LinkTextbox.IsEnabled = false;
                AuthStatusLabel.Content = "INVALID CLIENT ID OR SECRET";
            }
            AuthProgressRing.Visibility = Visibility.Hidden;
        }
        private void UpdatePreview(IMedia Media)
        {
            Art_Image.Source = Utils.ToBitmapImage(Media.Art);
            Title_Textblock.Text = Media.Title;
            FirstAuthor_Textblock.Text = Media.FirstAuthor;

            PreviewItem = Media;

            SetPreviewToPlayer(Media);

            PreviewProgressRing.Visibility = Visibility.Hidden;
        }
        private void UpdatePreview(SpotifyPlaylist Playlist)
        {
            Art_Image.Source = Utils.ToBitmapImage(Playlist.Art);
            Title_Textblock.Text = Playlist.Title;
            FirstAuthor_Textblock.Text = Playlist.Author;

            PreviewItem = Playlist;

            PreviewProgressRing.Visibility = Visibility.Hidden;
        }
        private void ClearPreview()
        {
            Title_Textblock.Text = "Nothing!";
            FirstAuthor_Textblock.Text = "";
            Art_Image.Source = new BitmapImage(new Uri("resources\\default_art.jpg", UriKind.Relative));

            PreviewItem = null;

            PreviewProgressRing.Visibility = Visibility.Hidden;
        }
        private void AddToQueue(object Item)
        {
            DownloadStatusLabel.Content = "Adding to queue...";
            DownloadProgressRing.Visibility = Visibility.Visible;

            int temp = 0;
            if (Item is IMedia Media)
            {
                QueueBindingList.Add(Media);
                temp++;
            }
            else if (Item is SpotifyPlaylist Playlist)
            {
                foreach (var Track in Playlist.Tracks)
                {
                    QueueBindingList.Add(Track);
                    temp++;
                }
            }

            DownloadStatusLabel.Content = $"{temp} items added to queue";
            DownloadProgressRing.Visibility = Visibility.Hidden;
        }
        private void RemoveFromQueue(IMedia Media)
        {
            QueueBindingList.Remove(Media);
        }
        private void DownloadControlEnabled(bool ENABLED)
        {
            if (ENABLED)
            {
                spotcancel.IsEnabled = false;
                LinkTextbox.IsEnabled = true;
                spotdl.IsEnabled = true;
                spotdl_all.IsEnabled = true;
                if (PreviewItem is YouTubeVideo Video && Video.IsVideo)
                {
                    AudioVideoToggle.Visibility = Visibility.Visible;
                }
            }
            else
            {
                AudioVideoToggle.Visibility = Visibility.Hidden;
                spotcancel.IsEnabled = true;
                LinkTextbox.IsEnabled = false;
                spotdl.IsEnabled = false;
                spotdl_all.IsEnabled = false;
            }
        }
        private async Task InitializeDownload(IMedia Media)
        {
            DownloadStatusLabel.Content = "Starting...";
            DownloadProgressRing.Visibility = Visibility.Visible;

            await Task.Run(() => downloader.DownloadMedia(Media));

            DownloadProgressRing.Visibility = Visibility.Hidden;
        }
        public void DownloadCompleted(object? sender, DownloadCompleteEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                DownloadStatusLabel.Content = e.Result switch
                {
                    Result.Success => "Success",
                    Result.NoMediaFound => "No corresponding media found",
                    Result.DuplicateFile => "File already exists",
                    Result.Cancelled => "Cancelled",
                    Result.FailedRequest => "Could not fetch response from server",
                    Result.NotDetermined => "Error",
                    _ => "Error",
                };
            }));
        }
        public void DownloadProgressChanged(object? sender, Libraries.DownloadProgressEventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                if (e.IsVideo)
                {
                    ProgressBar.Value = (int)(100 * e.Progress);
                    DownloadStatusLabel.Content = $"Downloading... {ProgressBar.Value}%";
                }
                else
                {
                    if (e.Progress <= 0.15)
                    {
                        ProgressBar.Value = (int)(100 * (e.Progress / 0.15));
                        DownloadStatusLabel.Content = $"Downloading... {ProgressBar.Value}%";
                    }
                    else if (e.Progress > 0.15)
                    {
                        ProgressBar.Value = (int)(100 * ((e.Progress - 0.15) / 0.85));
                        DownloadStatusLabel.Content = $"Converting... {ProgressBar.Value}%";
                    }
                }
            }));
        }
        private void SetPreviewToPlayer(IMedia Media)
        {
            if (Media is SpotifyTrack)
            {
                var Track = Media as SpotifyTrack;
                if (mplayer.WaveOut.PlaybackState != PlaybackState.Playing)
                {
                    Playback = PlaybackType.Preview;
                    playerArt.Source = Utils.ToBitmapImage(Track.Art);
                    PlayerTitleTextblock.Text = Track.Title;
                    PlayerArtistTextblock.Text = Track.FirstAuthor;
                    if (!string.IsNullOrWhiteSpace(Track.PreviewURL))
                    {
                        PlayerControlsEnabled(true);
                        mplayer.LoadPreview(Track.PreviewURL, BASSBOOST);
                        mplayer.HasLoadedMedia = true;

                    }
                    else
                    {
                        PlayerControlsEnabled(false);
                        mplayer.HasLoadedMedia = false;
                    }
                }
            }
        }
        private void SetFileToPlayer(MP3File music)
        {
            if (mplayer.WaveOut.PlaybackState != PlaybackState.Playing)
            {
                Playback = PlaybackType.FromFile;
                playerArt.Source = music.GetImageSource();
                PlayerTitleTextblock.Text = music.Title;
                PlayerArtistTextblock.Text = music.PrintedAuthors;
                try
                {
                    mplayer.LoadMedia(music.Filename, BASSBOOST);
                    mplayer.HasLoadedMedia = true;
                    PlayerControlsEnabled(true);
                }
                catch
                {
                    mplayer.HasLoadedMedia = false;
                    PlayerControlsEnabled(false);
                }
            }
        }
        private void PlayerControlsEnabled(bool ENABLED)
        {
            if (ENABLED)
            {
                switch (Playback)
                {
                    case PlaybackType.Preview:
                        Image_PlayButton.Opacity = 1;
                        img_prevb.Opacity = 0.5;
                        img_nextb.Opacity = 0.5;
                        PlaybackPositionSlider.Opacity = 1;
                        PlayButton.IsEnabled = true;
                        prevb.IsEnabled = false;
                        nextb.IsEnabled = false;
                        PlaybackPositionSlider.IsEnabled = true;
                        break;
                    case PlaybackType.FromFile:
                        Image_PlayButton.Opacity = 1;
                        img_prevb.Opacity = 1;
                        img_nextb.Opacity = 1;
                        PlaybackPositionSlider.Opacity = 1;
                        PlayButton.IsEnabled = true;
                        prevb.IsEnabled = true;
                        nextb.IsEnabled = true;
                        PlaybackPositionSlider.IsEnabled = true;
                        break;
                }
            }
            else
            {
                switch (Playback)
                {
                    case PlaybackType.Preview:
                        Image_PlayButton.Opacity = 0.5;
                        img_prevb.Opacity = 0.5;
                        img_nextb.Opacity = 0.5;
                        PlaybackPositionSlider.Opacity = 0.5;
                        PlayButton.IsEnabled = false;
                        prevb.IsEnabled = false;
                        nextb.IsEnabled = false;
                        PlaybackPositionSlider.IsEnabled = false;
                        break;
                    case PlaybackType.FromFile:
                        Image_PlayButton.Opacity = 0.5;
                        img_prevb.Opacity = 0.5;
                        img_nextb.Opacity = 0.5;
                        PlaybackPositionSlider.Opacity = 0.5;
                        PlayButton.IsEnabled = false;
                        prevb.IsEnabled = false;
                        nextb.IsEnabled = false;
                        PlaybackPositionSlider.IsEnabled = false;
                        break;
                }
            }
        }
        private void NextSong()
        {
            if (CurrentIndexInPlayedSongs == PlayedSongs.Count - 1)
            {
                if (!SHUFFLE)
                {
                    int nextindex = musiclist.SelectedIndex + 1;
                    if (nextindex > musiclist.Items.Count - 1)
                    {
                        musiclist.SelectedIndex = 0;
                    }
                    else
                    {
                        musiclist.SelectedIndex = nextindex;
                    }
                }
                else
                {
                    int nextindex = new Random().Next(0, musiclist.Items.Count - 1);
                    musiclist.SelectedIndex = nextindex;
                }
            }
            else
            {
                DoNotAdd = true;
                CurrentIndexInPlayedSongs += 1;
                musiclist.SelectedItem = PlayedSongs[CurrentIndexInPlayedSongs];
            }
        }
        public async Task<string> Search_PlainText()
        {
            string searchquery = LinkTextbox.Text;
            try
            {
                string part1 = searchquery;
                int index = 0;
                if (string.IsNullOrWhiteSpace(searchquery))
                {
                    return "";
                }
                if (searchquery.EndsWith('-'))
                {
                    int x = searchquery.IndexOf('-');
                    string part2 = searchquery.Substring(x, searchquery.Length - x);
                    index = part2.Length;
                    part1 = searchquery.Substring(0, x);
                }
                var searchresult = await Task.Run(() => spotify.SearchTrack(part1, index));
                return searchresult;
            }
            catch
            {
                return "";
            }
        }
        private void RefreshMusicList()
        {
            scanner.RunWorkerAsync(directories);
        }
        private void SortBy(SortType sorttype)
        {
            if (MusicTabInitialized)
            {
                musictabloading.Visibility = Visibility.Visible;
                switch (sorttype)
                {
                    case SortType.Title:
                        temp.Sort();
                        break;
                    case SortType.Artists:
                        temp.Sort(delegate (MP3File x, MP3File y)
                        {
                            if (x.PrintedAuthors == null && y.PrintedAuthors == null) return 0;
                            else if (x.PrintedAuthors == null) return -1;
                            else if (y.PrintedAuthors == null) return 1;
                            else return x.PrintedAuthors.CompareTo(y.PrintedAuthors);
                        });
                        break;
                    case SortType.Album:
                        temp.Sort(delegate (MP3File x, MP3File y)
                        {
                            if (x.Album == null && y.Album == null) return 0;
                            else if (x.Album == null) return -1;
                            else if (y.Album == null) return 1;
                            else return x.Album.CompareTo(y.Album);
                        });
                        break;
                    case SortType.Year:
                        temp.Sort(delegate (MP3File x, MP3File y)
                        {
                            if (x.Year == null && y.Year == null) return 0;
                            else if (x.Year == null) return -1;
                            else if (y.Year == null) return 1;
                            else return x.Year.CompareTo(y.Year);
                        });
                        break;
                    case SortType.Recency:
                        temp.Sort(delegate (MP3File x, MP3File y)
                        {
                            if (x.DateAdded == null && y.DateAdded == null) return 0;
                            else if (x.DateAdded == null) return -1;
                            else if (y.DateAdded == null) return 1;
                            else return x.DateAdded.CompareTo(y.Year);
                        });
                        break;
                }
                MusicList.Clear();
                delivery.Start();
            }
        }
    }
}