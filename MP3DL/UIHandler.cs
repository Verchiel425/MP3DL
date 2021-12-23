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
using MP3DL.Libraries;

namespace MP3DL
{
    public partial class MainWindow : Window
    {
        #region EventHandlers
        private void bassboost_clicked(object sender, RoutedEventArgs e)
        {
            if (bassboost.IsChecked == true)
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
            if (CurrentIndexInPlayedSongs <= 0)
            {
                InsertFromStart = true;
                int previndex = musiclist.SelectedIndex - 1;
                if (previndex < 0)
                {
                    musiclist.SelectedIndex = musiclist.Items.Count - 1;
                }
                else
                {
                    musiclist.SelectedIndex = previndex;
                }
            }
            else
            {
                DoNotAdd = true;
                CurrentIndexInPlayedSongs -= 1;
                musiclist.SelectedItem = PlayedSongs[CurrentIndexInPlayedSongs];
            }
        }
        private void musiclist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source == musiclist && musiclist.SelectedItem is MP3File)
            {
                if (!DoNotAdd)
                {
                    if (PlayedSongs.Count > 0)
                    {
                        if (musiclist.SelectedItem != PlayedSongs[PlayedSongs.Count - 1])
                        {
                            if (!InsertFromStart)
                            {
                                PlayedSongs.Add((MP3File)musiclist.SelectedItem);
                                CurrentIndexInPlayedSongs = PlayedSongs.Count - 1;
                            }
                            else
                            {
                                PlayedSongs.Insert(0, (MP3File)musiclist.SelectedItem);
                                CurrentIndexInPlayedSongs = 0;
                                InsertFromStart = false;
                            }
                        }
                    }
                    else
                    {
                        if (!InsertFromStart)
                        {
                            PlayedSongs.Add((MP3File)musiclist.SelectedItem);
                            CurrentIndexInPlayedSongs = PlayedSongs.Count -1;
                        }
                        else
                        {
                            PlayedSongs.Insert(0, (MP3File)musiclist.SelectedItem);
                            CurrentIndexInPlayedSongs = 0;
                            InsertFromStart = false;
                        }
                    }
                }
                else
                {
                    DoNotAdd = false;
                }
                Debug.WriteLine($"Index: {CurrentIndexInPlayedSongs}");
                Debug.WriteLine($"In {PlayedSongs.Count} played songs");
                var item = (MP3File)musiclist.SelectedItem;
                if (mplayer.WaveOut.PlaybackState == PlaybackState.Playing || mplayer.WaveOut.PlaybackState == PlaybackState.Paused)
                {
                    mplayer.Stop();
                }

                if (!File.Exists(item.Filename))
                {
                    MusicList.Remove(item);
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
            folderDialog.Owner = this;
            if(folderDialog.ShowDialog() == true)
            {
                if (folderDialog.StringList.Count < directories.Count)
                {
                    MusicList.Clear();
                }
                directories = folderDialog.StringList;
                client.MUSICFOLDERS = directories;
                client.Save();
                RefreshMusicList();
            }
            this.Opacity = 1;
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
                if (main_menu.Tag.ToString() == "nexp")
                {
                    return;
                }
                else
                {
                    AnimateMenu();
                }
            }
            if ((MainWindowUI.Width <= 650 && WindowState == WindowState.Normal) && volumeslider.Tag.ToString() == "shown")
            {
                volumeslider.Tag = "hidden";
                FadeOutElements(pa_textblock, TextBlock.OpacityProperty);
                FadeOutElements(pt_textblock, TextBlock.OpacityProperty);
                FadeOutElements(volumeslider, Slider.OpacityProperty);
            }
            else if ((MainWindowUI.Width > 650 || WindowState == WindowState.Maximized) && volumeslider.Tag.ToString() == "hidden")
            {
                volumeslider.Tag = "shown";
                FadeInElements(pa_textblock, TextBlock.OpacityProperty);
                FadeInElements(pt_textblock, TextBlock.OpacityProperty);
                FadeInElements(volumeslider, Slider.OpacityProperty);
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
            if (spotify == null) { return; }
            if (!spotify.Authd) { return; }

            Title_Textblock.Text = "Please wait...";
            FirstAuthor_Textblock.Text = "";
            PreviewProgressRing.Visibility = Visibility.Visible;
            string LINK = LinkTextbox.Text;

            string ID = Utils.SpotifyID(LINK);

            try
            {
                switch (Utils.GetLinkType(LINK))
                {
                    case 0://Track
                        await spotify.SetCurrentTrack(ID);
                        AudioVideoToggle.Visibility = Visibility.Hidden;
                        UpdatePreview(spotify.CurrentTrack);
                        break;
                    case 1://Playlist
                        await spotify.SetCurrentPlaylist(ID);
                        AudioVideoToggle.Visibility = Visibility.Hidden;
                        UpdatePreview(spotify.CurrentPlaylist);
                        break;
                    case 2://Video
                        await youtube.SetCurrentVid(LINK);
                        AudioVideoToggle.Visibility = Visibility.Visible;
                        UpdatePreview(youtube.CurrentVideo);
                        break;
                    case 3://Plain_Search
                        waiter.Stop();
                        waiter.Start();
                        break;
                }
            }
            catch (ArgumentException)
            {
                AudioVideoToggle.Visibility = Visibility.Hidden;
                ClearPreview();
            }
        }
        private void AddToQueueButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (!spotify.Authd) { return; }
            if (PreviewItem is not null)
            {
                if (PreviewItem is YouTubeVideo Video)
                {
                    if (AUDIOONLY)
                    {
                        Video.SetAsAudio();
                    }
                    else
                    {
                        Video.SetAsVideo();
                    }
                }
                AddToQueue(PreviewItem);
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
                    UpdatePreview(temp);
                }
                else
                {
                    if (QueueBindingList.Count == 0)
                    {
                        UpdateFromTextbox();
                        return;
                    }
                    else
                    {
                        ClearPreview();
                        return;
                    }
                }
            }
        }
        private async void DownloadButton_Clicked(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();
            downloader.CancelToken = cts.Token;
            DownloadControlEnabled(false);

            if (QueueDataGrid.SelectedItem is IMedia Selected)
            {
                if (Selected is YouTubeVideo Video)
                {
                    if (AUDIOONLY)
                    {
                        Video.SetAsAudio();
                    }
                    else
                    {
                        Video.SetAsVideo();
                    }
                }
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
                if(PreviewItem is IMedia temp)
                {
                    if (temp is YouTubeVideo Video)
                    {
                        if (AUDIOONLY)
                        {
                            Video.SetAsAudio();
                        }
                        else
                        {
                            Video.SetAsVideo();
                        }
                    }
                    await InitializeDownload(temp);
                }
            }

            DownloadControlEnabled(true);
        }
        private async void DownloadAll_Clicked(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();
            downloader.CancelToken = cts.Token;
            DownloadControlEnabled(false);
            List<IMedia> tmp = QueueBindingList.ToList();
            foreach (IMedia media in tmp)
            {
                if (cts.IsCancellationRequested) { break; }
                UpdatePreview(media);
                await InitializeDownload(media);
                RemoveFromQueue(media);
            }
            DownloadControlEnabled(true);
        }
        private void CancelAll_Clicked(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("--Cancelled--");
            cts.Cancel();
        }
        private void VolumeSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mplayer.Volume = (float)(volumeslider.Value);
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
            if (PreviewItem is IMedia Media)
            {
                mplayer.Stop();
                SetPreviewToPlayer(Media);
            }
        }
        private void AudioVideoToggle_Clicked(object sender, RoutedEventArgs e)
        {
            if (AUDIOONLY)
            {
                AUDIOONLY = false;
                AudioVideoToggle.Content = "+ Video";
            }
            else
            {
                AUDIOONLY = true;
                AudioVideoToggle.Content = "Audio Only";
            }
        }
        private void OnStartUp(object sender, RoutedEventArgs e)
        {
            ClientIDTextbox.Password = cryptography.Decrypt(Properties.Settings.Default.ClientID);
            ClientSecretTextbox.Password = cryptography.Decrypt(Properties.Settings.Default.ClientSecret);
            OUTPUT = Properties.Settings.Default.OutputDir;
            AUTOAUTH = Properties.Settings.Default.AutoAuth;
            SHUFFLE = Properties.Settings.Default.Shuffle;
            AUTOPLAY = Properties.Settings.Default.AutoPlay;
            BASSBOOST = Properties.Settings.Default.BassBoost;
            directories = client.GetMUSICFOLDERS();

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
            if (!MOUSEOVERMENU && main_menu.Tag.ToString() == "exp")
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

                PlayButton.Tag = "np";
            }
            else switch (Playback)
                {
                    case PlaybackType.Preview:
                        mplayer.Rewind();
                        mplayer.Stop();

                        PlayButton.Tag = "np";
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

        private void Spotify_PlaylistFetchingProgressChanged(object? sender, PlaylistProgressEventArgs e)
        {
            DownloadProgressRing.Visibility = Visibility.Visible;
            DownloadStatusLabel.Content = $"{e.Finished}/{e.Total} songs fetched";
        }
        #endregion
    }
}
