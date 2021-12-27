using MP3DL.Media;
using MP3DL.Encryption;
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
            try
            {
                await Task.Run(() => spotify.Auth());
            }
            catch
            {

            }

            Debug.WriteLine("--Auth Attempt Complete!--");
            if (spotify.Authd)
            {
                AuthMenuItem.Source = new BitmapImage(new Uri("resources\\icon_unlock.png", UriKind.Relative));
                Properties.Settings.Default.ClientID = Cryptography.Encrypt(ID);
                Properties.Settings.Default.ClientSecret = Cryptography.Encrypt(SECRET);
                LinkTextbox.IsEnabled = true;
                DownloadControlsEnabled(true);
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
        private void UpdatePreview(IMedia Media, InputFrom Input)
        {
            Art_Image.Source = Utils.ToBitmapImage(Media.GetArt());
            Title_Textblock.Text = Media.Title;
            FirstAuthor_Textblock.Text = Media.FirstAuthor;

            if (Input == InputFrom.User)
            {
                MediaInput = Media;
            }
            CurrentlyPreviewing = Media;
            SetPreviewToPlayer(Media);

            PreviewProgressRing.Visibility = Visibility.Hidden;
        }
        private void UpdatePreview(IMediaCollection<SpotifyTrack> MediaCollection)
        {
            Art_Image.Source = Utils.ToBitmapImage(MediaCollection.Art);
            Title_Textblock.Text = MediaCollection.Title;
            FirstAuthor_Textblock.Text = MediaCollection.Author;

            MediaInput = MediaCollection;
            CurrentlyPreviewing = MediaCollection;

            PreviewProgressRing.Visibility = Visibility.Hidden;
        }
        private void UpdatePreview(object Item, InputFrom Input)
        {
            switch (Item)
            {
                case IMediaCollection<SpotifyTrack> MediaCollection:
                    UpdatePreview(MediaCollection);
                    break;
                case IMedia Media:
                    UpdatePreview(Media, Input);
                    break;
            }
        }
        private void ClearPreview(InputFrom Input)
        {
            Title_Textblock.Text = "Nothing!";
            FirstAuthor_Textblock.Text = "";
            Art_Image.Source = new BitmapImage(new Uri("resources\\default_art.jpg", UriKind.Relative));

            if (Input == InputFrom.User)
            {
                MediaInput = null;
            }

            PreviewProgressRing.Visibility = Visibility.Hidden;
        }
        private void AddToQueue(object Item)
        {
            DownloadStatusLabel.Content = "Adding to queue...";
            DownloadProgressRing.Visibility = Visibility.Visible;

            int temp = 0;

            switch (Item)
            {
                case IMedia Media:
                    switch (Media)
                    {
                        case YouTubeVideo Video:
                            Media = new YouTubeVideo(Video);
                            break;
                        case SpotifyTrack Track:
                            Media = new SpotifyTrack(Track);
                            break;
                    }
                    QueueBindingList.Add(Media);
                    temp++;
                    break;
                case IMediaCollection<SpotifyTrack> MediaCollection:
                    foreach (var CollectionMedia in MediaCollection.Medias)
                    {
                        QueueBindingList.Add(CollectionMedia);
                        temp++;
                    }
                    break;
            }

            DownloadStatusLabel.Content = $"{temp} items added to queue";
            DownloadProgressRing.Visibility = Visibility.Hidden;
        }
        private void RemoveFromQueue(IMedia Media)
        {
            QueueBindingList.Remove(Media);
        }
        private void DownloadControlsEnabled(bool ENABLED)
        {
            CancelAllButton.IsEnabled = !ENABLED;
            AddToQueueButton.IsEnabled = ENABLED;
            LinkTextbox.IsEnabled = ENABLED;
            DownloadButton.IsEnabled = ENABLED;
            DownloadAllButton.IsEnabled = ENABLED;
            if (MediaInput is YouTubeVideo Video)
            {
                FormatToggle.Visibility = ENABLED switch
                {
                    true => Visibility.Visible,
                    false => Visibility.Hidden,
                }; ;
            }
            else
            {
                FormatToggle.Visibility = Visibility.Hidden;
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
        public void DownloadProgressChanged(object? sender, Media.DownloadProgressEventArgs e)
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
            if (Media is SpotifyTrack Track)
            {
                if (mplayer.WaveOut.PlaybackState != PlaybackState.Playing)
                {
                    Playback = PlaybackType.Preview;
                    playerArt.Source = Utils.ToBitmapImage(Track.GetArt());
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
            double opacity = ENABLED switch
            {
                true => 1,
                false => 0.5,
            };
            switch (Playback)
            {
                case PlaybackType.Preview:
                    Image_PlayButton.Opacity = opacity;
                    img_prevb.Opacity = 0.5;
                    img_nextb.Opacity = 0.5;
                    PlaybackPositionSlider.Opacity = opacity;
                    PlayButton.IsEnabled = ENABLED;
                    prevb.IsEnabled = !ENABLED;
                    nextb.IsEnabled = !ENABLED;
                    PlaybackPositionSlider.IsEnabled = ENABLED;
                    break;
                case PlaybackType.FromFile:
                    Image_PlayButton.Opacity = opacity;
                    img_prevb.Opacity = opacity;
                    img_nextb.Opacity = opacity;
                    PlaybackPositionSlider.Opacity = opacity;
                    PlayButton.IsEnabled = ENABLED;
                    prevb.IsEnabled = ENABLED;
                    nextb.IsEnabled = ENABLED;
                    PlaybackPositionSlider.IsEnabled = ENABLED;
                    break;
            }
        }
        private void NextSong()
        {
            InputType = InputFrom.Code;
            if (!PlayedSongs.IsNextNull())
            {
                PlayedSongs.Next();
                MusicDataGrid.SelectedItem = PlayedSongs.GetCurrent();
            }
            else
            {
                int nextindex;
                if (!SHUFFLE)
                {
                    nextindex = MusicDataGrid.SelectedIndex + 1;
                    if (nextindex > MusicDataGrid.Items.Count - 1)
                    {
                        nextindex = 0;
                    }
                }
                else
                {
                    nextindex = new Random().Next(0, MusicDataGrid.Items.Count - 1);
                    while (nextindex == MusicDataGrid.SelectedIndex)
                    {
                        nextindex = new Random().Next(0, MusicDataGrid.Items.Count - 1);
                    }
                }
                PlayedSongs.Add((MP3File)MusicDataGrid.Items[nextindex]);
                MusicDataGrid.SelectedItem = MusicDataGrid.Items[nextindex];
            }
        }
        private void PrevSong()
        {
            InputType = InputFrom.Code;
            if (!PlayedSongs.IsPrevZero())
            {
                PlayedSongs.Prev();
                MusicDataGrid.SelectedItem = PlayedSongs.GetCurrent();
            }
            else
            {
                int previndex;
                if (!SHUFFLE)
                {
                    previndex = MusicDataGrid.SelectedIndex - 1;
                    if (previndex < 0)
                    {
                        previndex = MusicDataGrid.Items.Count - 1;
                    }
                }
                else
                {
                    previndex = new Random().Next(0, MusicDataGrid.Items.Count - 1);
                    while (previndex == MusicDataGrid.SelectedIndex)
                    {
                        previndex = new Random().Next(0, MusicDataGrid.Items.Count - 1);
                    }
                }
                PlayedSongs.InsertAtReaderHead((MP3File)MusicDataGrid.Items[previndex]);
                MusicDataGrid.SelectedItem = MusicDataGrid.Items[previndex];
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
            scanner.RunWorkerAsync(MusicFolders);
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
                MusicBindingList.Clear();
                delivery.Start();
            }
        }
    }
}