using MP3DL.Media;
using MP3DL.Media.Audio;
using MP3DL.Encryption;
using NAudio.Wave;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MP3DL
{
    public partial class MainWindow : Window
    {
        #region UIHandlers
        private void BassBoostCheckbox_Clicked(object sender, RoutedEventArgs e)
        {
            if (BassBoostCheckbox.IsChecked == true)
            {
                BASSBOOST = true;
            }
            else
            {
                BASSBOOST = false;
            }
        }
        private void autoplaytoggle(object sender, RoutedEventArgs e)
        {
            if (AUTOPLAY)
            {
                AUTOPLAY = false;
            }
            else
            {
                AUTOPLAY = true;
            }
        }

        private void shuffletoggle(object sender, RoutedEventArgs e)
        {
            if (SHUFFLE)
            {
                SHUFFLE = false;
            }
            else
            {
                SHUFFLE = true;
            }
        }
        private void nextb_clicked(object sender, RoutedEventArgs e)
        {
            NextSong();
        }
        private void prevb_clicked(object sender, RoutedEventArgs e)
        {
            PrevSong();
        }
        private void musiclist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source == MusicDataGrid && MusicDataGrid.SelectedItem is MP3File)
            {
                if (InputType == InputFrom.User)
                {
                    PlayedSongs.Add((MP3File)MusicDataGrid.SelectedItem);
                }
                else
                {
                    InputType = InputFrom.User;
                }
                Debug.WriteLine($"--Index: {PlayedSongs.ReaderHead}--");
                Debug.WriteLine($"--In {PlayedSongs.Count} played songs--");
                var item = (MP3File)MusicDataGrid.SelectedItem;
                if (mplayer.WaveOut.PlaybackState == PlaybackState.Playing || mplayer.WaveOut.PlaybackState == PlaybackState.Paused)
                {
                    mplayer.Stop();
                }

                if (!File.Exists(item.Filename))
                {
                    MusicBindingList.Remove(item);
                    if (PlayedSongs.Contains(item))
                    {
                        PlayedSongs.Remove(item);
                    }
                    RefreshMusicList();
                    return;
                }
                else
                {
                    SetFileToPlayer(item);
                    mplayer.Play();
                }
            }
        }
        private void sortby_changed(object sender, SelectionChangedEventArgs e)
        {
            switch (sortbycombobox.SelectedIndex)
            {
                case 0:
                    SortBy(SortType.Recency);
                    break;
                case 1:
                    SortBy(SortType.Title);
                    break;
                case 2:
                    SortBy(SortType.Artists);
                    break;
                case 3:
                    SortBy(SortType.Album);
                    break;
                case 4:
                    SortBy(SortType.Year);
                    break;
            }
        }
        private void ManageFoldersButton_Clicked(object sender, RoutedEventArgs e)
        {
            FolderDialog folderDialog = new FolderDialog();
            this.Opacity = 0.5;
            MainWindowBlur.Radius = 8;
            folderDialog.Owner = this;
            if (folderDialog.ShowDialog() == true)
            {
                if (folderDialog.StringList.Count < MusicFolders.Count)
                {
                    MusicBindingList.Clear();
                }
                MusicFolders = folderDialog.StringList;
                client.MUSICFOLDERS = MusicFolders;
                client.Save();
                RefreshMusicList();
            }
            this.Opacity = 1;
            MainWindowBlur.Radius = 0;
        }
        private void AutoAuthCheckbox_Clicked(object sender, RoutedEventArgs e)
        {
            if (AutoAuthCheckbox.IsChecked == true)
            {
                AUTOAUTH = true;
            }
            else
            {
                AUTOAUTH = false;
            }
        }
        private void MainWindow_SizeChanged(object sender, EventArgs e)
        {
            if (MainWindowUI.Width == 600)
            {
                if (MainMenu.Tag.Equals(ControlState.Active))
                {
                    AnimateMenu();
                }
            }
            if ((MainWindowUI.Width <= 670 && WindowState == WindowState.Normal) && VolumeSlider.Tag.ToString() == "shown")
            {
                VolumeSlider.Tag = "hidden";
                FadeOutElements(PlayerArtistTextblock, TextBlock.OpacityProperty, 1);
                FadeOutElements(PlayerTitleTextblock, TextBlock.OpacityProperty, 1);
                FadeOutElements(VolumeSlider, Slider.OpacityProperty, 1);
                FadeOutElements(ShuffleButton, Slider.OpacityProperty, 0.4);
                FadeOutElements(AutoPlayButton, Slider.OpacityProperty, 0.4);
            }
            else if ((MainWindowUI.Width > 670 || WindowState == WindowState.Maximized) && VolumeSlider.Tag.ToString() == "hidden")
            {
                VolumeSlider.Tag = "shown";
                FadeInElements(PlayerArtistTextblock, TextBlock.OpacityProperty, 1);
                FadeInElements(PlayerTitleTextblock, TextBlock.OpacityProperty, 1);
                FadeInElements(VolumeSlider, Slider.OpacityProperty, 1);
                FadeInElements(ShuffleButton, Slider.OpacityProperty, 0.4);
                FadeInElements(AutoPlayButton, Slider.OpacityProperty, 0.4);
            }
        }
        private void MenuButton_Clicked(object sender, RoutedEventArgs e)
        {
            AnimateMenu();
        }
        private void DownloadsMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 1;
        }
        private void MyMusicMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 2;
        }
        private void AuthMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 3;
        }
        private void SettingsMenuItem_Clicked(object sender, RoutedEventArgs e)
        {
            MainTabControl.SelectedIndex = 4;
        }
        private void Tab_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source == MainTabControl)
            {
                if (MainTabControl.SelectedIndex == 2)
                {
                    musictabloading.Visibility = Visibility.Visible;
                    delivery.Start();
                }
                else
                {
                    delivery.Stop();
                }
                GC.Collect();
                FadeTab();
            }
            else { return; }
        }
        private void ExpandPlayerButton_Clicked(object sender, RoutedEventArgs e)
        {
            ExpandPlayerButton.IsEnabled = false;
            AnimatePlayer();
            RotateButton();
        }
        private void AuthenticateButton_Clicked(object sender, RoutedEventArgs e)
        {
            Authenticate();
        }
        private void LinkTextbox_Changed(object sender, TextChangedEventArgs e)
        {
            UpdateFromTextbox();
        }
        private async void UpdateFromTextbox()
        {
            GC.Collect();
            if (spotify == null) { return; }
            if (!spotify.Authd) { return; }

            Title_Textblock.Text = "Please wait...";
            FirstAuthor_Textblock.Text = "";
            PreviewProgressRing.Visibility = Visibility.Visible;

            var link = Utils.FilterLink(LinkTextbox.Text);

            try
            {
                switch (link.Type)
                {
                    case LinkType.SpotifyTrack:
                        MediaInput = null;
                        await spotify.SetCurrentTrack(link.Link);
                        FormatToggle.Visibility = Visibility.Hidden;
                        UpdatePreview(spotify.CurrentTrack, InputFrom.User);
                        break;
                    case LinkType.SpotifyPlaylist:
                        MediaInput = null;
                        await spotify.SetCurrentPlaylist(link.Link);
                        FormatToggle.Visibility = Visibility.Hidden;
                        UpdatePreview(spotify.CurrentMediaCollection);
                        break;
                    case LinkType.SpotifyAlbum:
                        MediaInput = null;
                        await spotify.SetCurrentAlbum(link.Link);
                        FormatToggle.Visibility = Visibility.Hidden;
                        UpdatePreview(spotify.CurrentMediaCollection);
                        break;
                    case LinkType.YouTubeVideo:
                        MediaInput = null;
                        await youtube.SetCurrentVid(link.Link);
                        FormatToggle.Visibility = Visibility.Visible;
                        UpdatePreview(youtube.CurrentVideo, InputFrom.User);
                        break;
                    case LinkType.YouTubePlaylist:
                        break;
                    case LinkType.PlainText:
                        waiter.Stop();
                        waiter.Start();
                        break;
                }
            }
            catch (ArgumentException)
            {
                FormatToggle.Visibility = Visibility.Hidden;
                ClearPreview(InputFrom.User);
            }
        }
        private void AddToQueueButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (!spotify.Authd) { return; }
            if (MediaInput is not null)
            {
                if (CurrentlyPreviewing != MediaInput)
                {
                    UpdatePreview(MediaInput, InputFrom.Code);
                }
                AddToQueue(MediaInput);
            }
        }
        private void QueueSelection_Changed(object sender, SelectionChangedEventArgs e)
        {
            Title_Textblock.Text = "Please wait...";
            FirstAuthor_Textblock.Text = "";
            PreviewProgressRing.Visibility = Visibility.Visible;

            if (e.Source == QueueDataGrid)
            {
                if (QueueDataGrid.SelectedItem is IMedia temp)
                {
                    UpdatePreview(temp, InputFrom.Code);
                }
                else
                {
                    if (QueueBindingList.Count <= 1)
                    {
                        UpdateFromTextbox();
                        return;
                    }
                    else
                    {
                        ClearPreview(InputFrom.Code);
                        return;
                    }
                }
            }
        }
        private async void DownloadButton_Clicked(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();
            downloader.CancelToken = cts.Token;
            DownloadControlsEnabled(false);

            if (QueueDataGrid.SelectedItem is IMedia Selected)
            {
                await InitializeDownload(Selected);
                try
                {
                    RemoveFromQueue(Selected);
                }
                catch (System.InvalidCastException)
                {
                    QueueDataGrid.SelectedItem = null;
                    return;
                }
            }
            else
            {
                if (MediaInput is IMedia temp)
                {
                    await InitializeDownload(temp);
                }
            }

            DownloadControlsEnabled(true);
        }
        private async void DownloadAll_Clicked(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();
            downloader.CancelToken = cts.Token;
            DownloadControlsEnabled(false);
            List<IMedia> tmp = QueueBindingList.ToList();
            foreach (IMedia media in tmp)
            {
                if (cts.IsCancellationRequested) { break; }
                UpdatePreview(media, InputFrom.Code);
                await InitializeDownload(media);
                RemoveFromQueue(media);
            }
            DownloadControlsEnabled(true);
        }
        private void CancelAll_Clicked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("--Cancelled--");
            cts.Cancel();
        }
        private void VolumeSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mplayer.Volume = (float)(VolumeSlider.Value);
        }
        private void PlayButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (mplayer.HasLoadedMedia)
            {
                if (PlayButton.Tag.ToString() == "np")
                {
                    mplayer.Play();
                }
                else
                {
                    mplayer.Pause();
                }
            }
            return;
        }
        private void PlaybackPositionSlider_Dragging(object sender, EventArgs e)
        {
            mplayer.Pause();
        }
        private void PlaybackPositionSlider_Dragged(object sender, EventArgs e)
        {
            mplayer.Seek((int)PlaybackPositionSlider.Value);
            mplayer.Play();
        }
        private void DownloadFolder_Clicked(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", @OUTPUT);
        }
        private void BrowseOutputFolder_Clicked(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog browser = new VistaFolderBrowserDialog();
            if (browser.ShowDialog() == true)
            {
                OUTPUT = browser.SelectedPath;
            }
        }
        private void CoverArt_Clicked(object sender, MouseButtonEventArgs e)
        {
            if (CurrentlyPreviewing is IMedia Media)
            {
                mplayer.Stop();
                SetPreviewToPlayer(Media);
            }
        }
        private void FormatToggle_Clicked(object sender, RoutedEventArgs e)
        {
            if (MediaInput is YouTubeVideo Video)
            {
                if (Video.IsVideo)
                {
                    Video.IsVideo = false;
                    FormatToggle.Content = "Audio";
                }
                else
                {
                    Video.IsVideo = true;
                    FormatToggle.Content = "Video";
                }
            }
        }
        private void OnStartUp(object sender, RoutedEventArgs e)
        {
            ClientIDTextbox.Password = Cryptography.Decrypt(Properties.Settings.Default.ClientID);
            ClientSecretTextbox.Password = Cryptography.Decrypt(Properties.Settings.Default.ClientSecret);
            OUTPUT = Properties.Settings.Default.OutputDir;
            AUTOAUTH = Properties.Settings.Default.AutoAuth;
            SHUFFLE = Properties.Settings.Default.Shuffle;
            AUTOPLAY = Properties.Settings.Default.AutoPlay;
            BASSBOOST = Properties.Settings.Default.BassBoost;
            MusicFolders = client.GetMUSICFOLDERS();

            if (string.IsNullOrWhiteSpace(OUTPUT))
            {
                OUTPUT = "Downloads";
            }
            if (!Directory.Exists(@OUTPUT))
            {
                Directory.CreateDirectory(OUTPUT);
            }
            if (AUTOAUTH)
            {
                Authenticate();
            }
            RefreshMusicList();
        }
        private void OnClose(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
        private void MouseOnMenu(object sender, MouseEventArgs e)
        {
            MOUSEOVERMENU = true;
        }
        private void MouseOutsideMenu(object sender, MouseEventArgs e)
        {
            MOUSEOVERMENU = false;
        }
        private void MouseClicked(object sender, MouseButtonEventArgs e)
        {
            if (!MOUSEOVERMENU && MainMenu.Tag.Equals(ControlState.Active))
            {
                AnimateMenu();
            }
        }
        private void QueueBindingList_ListChanged(object? sender, ListChangedEventArgs e)
        {
            QueueCounter.Text = $"{QueueBindingList.Count} items in queue";
        }

        private void PlaybackPositionChanged(object? sender, PlaybackPositionEventArgs e)
        {
            PlaybackPositionSlider.Value = e.PlaybackPositionMs;
        }

        private void ResumedPlayback(object? sender, EventArgs e)
        {
            PlaybackPositionSlider.Maximum = mplayer.Duration.TotalMilliseconds;

            Image_PlayButton.Source = new BitmapImage(new Uri("resources\\icon_pause.png", UriKind.Relative));

            PlayButton.Tag = "p";
        }
        private void StoppedPlayback(object? sender, EventArgs e)
        {
            Image_PlayButton.Source = new BitmapImage(new Uri("resources\\icon_play.png", UriKind.Relative));

            PlayButton.Tag = "np";
        }
        private void PlaybackFinished(object? sender, EventArgs e)
        {
            Debug.WriteLine("Playback finished");
            if (!AUTOPLAY)
            {
                mplayer.Rewind();
                mplayer.Stop();
            }
            else switch (Playback)
                {
                    case PlaybackType.Preview:
                        mplayer.Rewind();
                        mplayer.Stop();
                        break;
                    case PlaybackType.FromFile:
                        NextSong();
                        break;
                }
        }
        private void Spotify_PlaylistFetchingDone(object? sender, EventArgs e)
        {
            DownloadStatusLabel.Content = $"Success";
            DownloadProgressRing.Visibility = Visibility.Hidden;
        }

        private void Spotify_PlaylistFetchingProgressChanged(object? sender, CollectionProgressEventArgs e)
        {
            DownloadProgressRing.Visibility = Visibility.Visible;
            DownloadStatusLabel.Content = $"{e.Finished}/{e.Total} songs fetched";
        }
        private void FetchDefault_Entered(object sender, System.Windows.Input.MouseEventArgs e)
        {
            FetchDefault.Content = "Click here to fetch default info";
        }
        private void FetchDefault_Left(object sender, System.Windows.Input.MouseEventArgs e)
        {
            FetchDefault.Content = "Can't be bothered to use your own?";
        }

        private void FetchDefault_Clicked(object sender, RoutedEventArgs e)
        {
            ClientIDTextbox.Password = Encryption.Cryptography.Decrypt("N44WaH/WXcq+HY2FpXWJX5TjCCFc5mJdF7s5axOqMLvJ/d8AysCAtyvOOtL7UbRwvjZJhmmUNF0U4vzQNHTW3bAZ9JP6rRvJypXmRCcKHqM=");
            ClientSecretTextbox.Password = Encryption.Cryptography.Decrypt("vGd1VR5mFB118g4+HEsd3S95XppW7JIcuG7Eo4SKDiYpGI3wXsI/xFg6NbzLUdwMeeo35lK1LUlBFTSTu9rVmH93tm7A95ZpSDZx1PZ5118=");
        }
        #endregion
    }
}
