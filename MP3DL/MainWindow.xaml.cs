using NAudio.Wave;
using Ookii.Dialogs.Wpf;
using SpotifyAPI.Web;
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
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

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

            queue.ItemsSource = list;

            playbacktimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            playbacktimer.Tick += Playbacktimer_Tick;
            waiter.Interval = TimeSpan.FromMilliseconds(500);
            waiter.Tick += Waiter_Tick;

        }

        #region BackgroundWorkers
        private async void Waiter_Tick(object? sender, EventArgs e)
        {
            waiter.Stop();
            var searchresult = await plainsearch();
            var result = await spotify.SetTrack(searchresult);
            if (result != 0)
            {
                return;
            }
            audiovidtoggle.Visibility = Visibility.Hidden;
            updatePreview(0);
            updatePlayer();
        }
        private void Playbacktimer_Tick(object? sender, EventArgs e)
        {
            double ms = p.wavech.CurrentTime.TotalMilliseconds;
            playbackposition.Value = (int)ms;
            //Debug.WriteLine((int)ms);
            if (playbackposition.Value == playbackposition.Maximum)
            {
                playbackFinished();
            }
        }
        #endregion

        CLIENT client = new CLIENT();
        DispatcherTimer waiter = new();
        DispatcherTimer playbacktimer = new();
        Spotify spotify = new Spotify("", "");
        YouTube youtube = new();
        BindingList<SONG> list = new();
        CancellationTokenSource cts = new();
        clientplayer p = new();


        public double _dlprog;
        public double dlprog
        {
            get { return _dlprog; }
            set
            {
                _dlprog = value;
                if (value < 0.15)
                {
                    spotify_pb.Value = (int)(100 * ((100 * value) / 15));
                    status.Content = $"Downloading... {spotify_pb.Value}%";
                }
                else
                {
                    spotify_pb.Value = (int)(100 * (value - 0.15) / 0.85);
                    status.Content = $"Converting... {spotify_pb.Value}%";
                }
            }
        }
        private int CURRENT { get; set; } = -1;
        private string CURRENTPREVURL { get; set; } = "";
        private bool MOUSEOVERMENU { get; set; } = false;
        private bool AUDIOONLY { get; set; } = true;

        #region SETTINGS
        private string _OUTPUT;
        private string OUTPUT
        {
            get { return _OUTPUT; }
            set
            {
                _OUTPUT = value;
                dlfoldertb.Text = value;
                client.OUTPUT = value;
            }
        }
        private bool _AUTOAUTH;
        private bool AUTOAUTH
        {
            get { return _AUTOAUTH; }
            set
            {
                _AUTOAUTH = value;
                autoauth.IsChecked = value;
                client.AUTOAUTH = value;
            }
        }
        #endregion

        #region EventHandlers
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
                animateMenu();
            }
        }

        private void autoauth_clicked(object sender, RoutedEventArgs e)
        {
            if (autoauth.IsChecked == true)
            {
                AUTOAUTH = true;
            }
            else
            {
                AUTOAUTH = false;
            }
            client.Save();
        }
        private void mainwindow_sizechanged(object sender, SizeChangedEventArgs e)
        {
            if (main_window.Width == 600)
            {
                if (main_menu.Tag.ToString() == "nexp")
                {
                    return;
                }
                else
                {
                    animateMenu();
                }
            }
        }
        private void menubutton_click(object sender, RoutedEventArgs e)
        {
            animateMenu();
        }
        private void downloadb_click(object sender, RoutedEventArgs e)
        {
            main_tabc.SelectedIndex = 1;
        }
        private void mymusicb_click(object sender, RoutedEventArgs e)
        {
            main_tabc.SelectedIndex = 2;
        }
        private void authb_click(object sender, RoutedEventArgs e)
        {
            main_tabc.SelectedIndex = 3;
        }
        private void settingsb_click(object sender, RoutedEventArgs e)
        {
            main_tabc.SelectedIndex = 4;
        }
        private void tabchanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source == main_tabc)
            {
                fadeTab();
            }
            else { return; }
        }
        private void expplayerb_click(object sender, RoutedEventArgs e)
        {
            expandplayerbutton.IsEnabled = false;
            animatePlayer();
            rotateButton();
        }
        private async void authenticate(object sender, RoutedEventArgs e)
        {
            authstatus.Content = "PLEASE WAIT...";
            authloading.Visibility = Visibility.Visible;
            string ID = textboxID.Password;
            string SECRET = textboxSecret.Password;

            spotify = new Spotify(ID, SECRET);
            await Task.Run(() => spotify.Auth());

            Debug.WriteLine("--Auth Attempt Complete!--");
            if (spotify.authd)
            {
                authb_img.Source = new BitmapImage(new Uri("resources\\icon_unlock.png", UriKind.Relative));
                client.ID = ID;
                client.SECRET = SECRET;
                client.Save();
                dltb.IsEnabled = true;
                authstatus.Content = "AUTHENTICATED";
            }
            else
            {
                authb_img.Source = new BitmapImage(new Uri("resources\\icon_lock.png", UriKind.Relative));
                dltb.IsEnabled = false;
                authstatus.Content = "INVALID CLIENT ID OR SECRET";
            }
            authloading.Visibility = Visibility.Hidden;
        }
        private void onStartUp(object sender, RoutedEventArgs e)
        {
            textboxID.Password = client.GetID();
            textboxSecret.Password = client.GetSECRET();
            OUTPUT = client.GetOUTPUT();
            AUTOAUTH = client.GetAUTOAUTH();

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
                authenticate();
            }
        }
        private async void spotifytb_changed(object sender, TextChangedEventArgs e)
        {
            if (spotify == null) { return; }
            if (!spotify.authd) { return; }

            title_textblock.Text = "Please wait...";
            artists_textblock.Text = "";
            spot_changering.Visibility = Visibility.Visible;
            string LINK = dltb.Text;

            string ID = utils.ID(LINK);

            int result;
            switch (check.LinkType(LINK))
            {
                case 0:
                    result = await spotify.SetTrack(ID);
                    if (result != 0)
                    {
                        return;
                    }
                    audiovidtoggle.Visibility = Visibility.Hidden;
                    updatePreview(0);
                    updatePlayer();
                    break;
                case 1:
                    result = await spotify.SetPlaylist(ID);
                    if (result != 0)
                    {
                        return;
                    }
                    audiovidtoggle.Visibility = Visibility.Hidden;
                    updatePreview(1);
                    break;
                case 2:
                    result = await youtube.SetVid(LINK);
                    if (result != 0)
                    {
                        return;
                    }
                    audiovidtoggle.Visibility = Visibility.Visible;
                    updatePreview(2);
                    break;
                case 3:
                    waiter.Stop();
                    waiter.Start();
                    break;
            }
        }
        private async void spotatq_click(object sender, RoutedEventArgs e)
        {
            if (!spotify.authd)
            {
                return;
            }
            status.Content = "Adding to queue...";
            spot_dlring.Visibility = Visibility.Visible;

            if (!AUDIOONLY)
            {
                status.Content = "Cannot add video to queue";
                spot_dlring.Visibility = Visibility.Hidden;
                return;
            }

            switch (CURRENT)
            {
                case -1:
                    break;
                case 0:
                    addToQueue(spotify.currentTrack);
                    queue.Items.Refresh();
                    break;
                case 1:
                    foreach (var trackID in spotify.currentPlaylistTracksID)
                    {
                        var track = await spotify.QueueTrack(trackID);
                        if (track != null)
                        {
                            addToQueue(track);
                        }
                    }
                    queue.Items.Refresh();
                    break;
                case 2:
                    addToQueue(youtube.currentVideo);
                    break;
            }
            status.Content = "Added to queue";
            spot_dlring.Visibility = Visibility.Hidden;
        }
        private async void selectionchanged(object sender, SelectionChangedEventArgs e)
        {
            title_textblock.Text = "Please wait...";
            artists_textblock.Text = "";
            spot_changering.Visibility = Visibility.Visible;

            if (e.Source == queue)
            {
                if (queue.SelectedItem == null)
                {
                    CURRENT = -1;
                    nothing();
                    return;
                }
                try
                {
                    SONG x = (SONG)queue.SelectedItem;
                    switch (x.Type)
                    {
                        case 0:
                            await spotify.SetTrack(x.Id);
                            updatePreview(0);
                            updatePlayer();
                            break;
                        case 1:
                            await youtube.SetVid(x.Id);
                            updatePreview(2);
                            break;
                    }
                }
                catch (System.InvalidCastException)
                {
                    CURRENT = -1;
                    nothing();
                    return;
                }
            }
        }
        private async void download_click(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();
            dl_controls(false);
            if (CURRENT != -1 && queue.SelectedItem is SONG)
            {
                await singleDownload((SONG)queue.SelectedItem);
                try
                {
                    removeFromQueue((SONG)queue.SelectedItem);
                }
                catch (System.InvalidCastException)
                {
                    queue.SelectedItem = null;
                    return;
                }
            }
            else if (CURRENT != -1)
            {
                await singleDownload();
            }
            dl_controls(true);
        }
        private async void downloadall_click(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();
            dl_controls(false);
            List<SONG> tmp = list.ToList();
            foreach (SONG media in tmp)
            {
                if (cts.IsCancellationRequested) { break; }
                if (media.Type == 0)
                {
                    await spotify.SetTrack(media.Id);
                    updatePreview(0);

                }
                else if (media.Type == 1)
                {
                    await youtube.SetVid(media.Id);
                    updatePreview(2);

                }
                await singleDownload(media);
                removeFromQueue(media);
            }
            dl_controls(true);
        }
        private void spotcancelb_click(object sender, RoutedEventArgs e)
        {
            cts.Cancel();
        }
        private void volumechanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            p.volume = (float)(volumeslider.Value / 100);
        }
        private void previewb_click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CURRENTPREVURL))
            {
                if (previewb.Tag.ToString() == "np")
                {
                    img_previewb.Source = new BitmapImage(new Uri("resources\\icon_pause.png", UriKind.Relative));
                    p.play();
                    playbackposition.Maximum = p.previewDuration.TotalMilliseconds;
                    playbacktimer.Start();
                    previewb.Tag = "p";
                }
                else
                {
                    img_previewb.Source = new BitmapImage(new Uri("resources\\icon_play.png", UriKind.Relative));
                    p.pause();
                    playbacktimer.Stop();
                    previewb.Tag = "np";
                }
            }
            return;
        }
        private void dragging(object sender, EventArgs e)
        {
            p.pause();
            playbacktimer.Stop();
        }
        private void dragged(object sender, EventArgs e)
        {
            p.seek((int)playbackposition.Value);
            if (previewb.Tag.ToString() == "p")
            {
                p.play();
                playbacktimer.Start();
            }
            else
            {

            }
        }
        private void dlb_click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", @OUTPUT);
        }
        private void browsedlfolder_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog browser = new VistaFolderBrowserDialog();
            if (browser.ShowDialog() == true)
            {
                OUTPUT = browser.SelectedPath;
            }
            client.Save();
        }
        private void overridepreview(object sender, MouseButtonEventArgs e)
        {
            if (CURRENT != 0)
            {
                return;
            }
            img_previewb.Source = new BitmapImage(new Uri("resources\\icon_play.png", UriKind.Relative));
            p.stop();
            playbacktimer.Stop();
            previewb.Tag = "np";
            updatePlayer();
        }
        private void toggle_clicked(object sender, RoutedEventArgs e)
        {
            if (AUDIOONLY)
            {
                AUDIOONLY = false;
                audiovidtoggle.Content = "+ Video";
            }
            else
            {
                AUDIOONLY = true;
                audiovidtoggle.Content = "Audio Only";
            }
        }
        #endregion

        //problematic

        private async Task<int> DownloadTrack(SONG song, string output, CancellationToken ct)
        {
            while (!cts.IsCancellationRequested)
            {
                string firstartist = song.FirstFromPrinted(song.PrintedArtists);
                string songName = utils.ClearChars($"{firstartist} - {song.Title}");

                if (File.Exists(System.IO.Path.Combine(output, $"{songName}.mp3")))
                {
                    this.dlprog = 1;
                    return 2;
                }

                var spot_youtube = new YoutubeClient();
                var searchresult = await Task.Run(() => Search(songName + " Audio", song.Duration));

                if (string.IsNullOrWhiteSpace(searchresult))
                {
                    searchresult = await Task.Run(() => Search(songName, song.Duration));
                    if (string.IsNullOrWhiteSpace(searchresult))
                    {
                        this.dlprog = 1;
                        return 4;
                    }
                }

                var progress = new Progress<double>(p => dlprog = p);

                try
                {
                    await spot_youtube.Videos.DownloadAsync
                        (searchresult,
                        System.IO.Path.Combine(output, $"{songName}.mp3"), progress, ct);
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    this.dlprog = 1;
                    return 3;
                }
                catch (TaskCanceledException)
                {
                    this.dlprog = 1;
                    return -1;
                }
                catch (Exception)
                {
                    this.dlprog = 1;
                    return 3;
                }

                song.setTags(System.IO.Path.Combine(output, $"{songName}.mp3"));

                return 0;
            }
            return 1;
        }
        private async Task<int> DownloadYT(SONG media, string output, bool AudioOnly, CancellationToken ct)
        {
            while (!cts.IsCancellationRequested)
            {
                if (AudioOnly)
                {
                    string firstartist = media.FirstFromPrinted(media.PrintedArtists);
                    string songName = utils.ClearChars($"{firstartist} - {media.Title}");

                    if (File.Exists(System.IO.Path.Combine(output, $"{songName}.mp3")))
                    {
                        this.dlprog = 1;
                        return 2;
                    }

                    var spot_youtube = new YoutubeClient();
                    var progress = new Progress<double>(p => dlprog = p);

                    try
                    {
                        await spot_youtube.Videos.DownloadAsync
                            (media.Id,
                            System.IO.Path.Combine(output, $"{songName}.mp3"), progress, ct);
                    }
                    catch (System.Net.Http.HttpRequestException)
                    {
                        this.dlprog = 1;
                        return 3;
                    }
                    catch (TaskCanceledException)
                    {
                        this.dlprog = 1;
                        return -1;
                    }
                    catch (Exception)
                    {
                        this.dlprog = 1;
                        return 3;
                    }

                    media.setTags(System.IO.Path.Combine(output, $"{songName}.mp3"));

                    return 0;
                }
                else
                {
                    string vidName = utils.ClearChars($"{media.Title}");

                    if (File.Exists(System.IO.Path.Combine(output, $"{vidName}.mp4")))
                    {
                        this.dlprog = 1;
                        return 2;
                    }

                    var spot_youtube = new YoutubeClient();
                    var progress = new Progress<double>(p => dlprog = p);

                    try
                    {
                        await spot_youtube.Videos.DownloadAsync
                            (media.Id,
                            System.IO.Path.Combine(output, $"{vidName}.mp4"), progress, ct);
                    }
                    catch (System.Net.Http.HttpRequestException)
                    {
                        this.dlprog = 1;
                        return 3;
                    }
                    catch (TaskCanceledException)
                    {
                        this.dlprog = 1;
                        return -1;
                    }
                    catch (Exception)
                    {
                        this.dlprog = 1;
                        return 3;
                    }

                    return 0;
                }
            }
            return 1;
        }
        public async Task<string> Search(string SearchQuery, int duration)
        {
            int searchresults = 5;
            VideoSearchResult videoResult;
            var spot_youtube = new YoutubeClient();
            string url = "";
            var videos = await spot_youtube.Search.GetVideosAsync(SearchQuery).CollectAsync(searchresults);
            for (int i = 0; i < searchresults; i++)
            {
                videoResult = videos[i];
                TimeSpan ts = (TimeSpan)videoResult.Duration;
                url = videoResult.Url;
                if (ts.TotalMilliseconds < duration + 7200 && ts.TotalMilliseconds > duration - 4000)
                {
                    break;
                }
                else
                {
                    url = "";
                }
            }

            return url;
        }

        //problematic

        private void animateMenu()
        {
            double x = 50d;
            double y = 250d;
            Storyboard storyboard = new();
            var changeWidth = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.6
            };
            storyboard.Children.Add(changeWidth);
            Storyboard.SetTarget(storyboard, main_menu);
            Storyboard.SetTargetProperty(storyboard, new PropertyPath(Grid.WidthProperty));

            storyboard.Stop();
            if (main_menu.Width > x && main_menu.Tag.ToString() == "exp")
            {
                changeWidth.From = main_menu.Width;
                changeWidth.To = x;
                storyboard.Begin();
            }
            else if (main_menu.Width < y && main_menu.Tag.ToString() == "nexp")
            {
                changeWidth.From = main_menu.Width;
                changeWidth.To = y;
                storyboard.Begin();
            }

            if (main_menu.Tag.ToString() == "nexp")
            {
                main_menu.Tag = "exp";
            }
            else
            {
                main_menu.Tag = "nexp";
            }
        }
        private void animatePlayer()
        {
            double x = 60d;
            double y = 120d;
            Storyboard storyboard = new();
            Storyboard title = new();
            Storyboard artist = new();
            var changeHeight = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.6
            };
            var changeTitleMargin = new ThicknessAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.6
            };
            var changeArtistMargin = new ThicknessAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.6
            };

            storyboard.Children.Add(changeHeight);
            title.Children.Add(changeTitleMargin);
            artist.Children.Add(changeArtistMargin);
            Storyboard.SetTarget(storyboard, player);
            Storyboard.SetTarget(title, playerTitle);
            Storyboard.SetTarget(artist, playerArtist);
            Storyboard.SetTargetProperty(storyboard, new PropertyPath(Grid.HeightProperty));
            Storyboard.SetTargetProperty(title, new PropertyPath(Label.MarginProperty));
            Storyboard.SetTargetProperty(artist, new PropertyPath(Label.MarginProperty));

            var thick1 = new Thickness();
            var thick2 = new Thickness();
            var thick3 = new Thickness();
            var thick4 = new Thickness();
            thick1.Right = 0;
            thick1.Top = 0;
            thick1.Bottom = 0;
            thick1.Left = 67;
            thick2.Right = 0;
            thick2.Top = 0;
            thick2.Bottom = 0;
            thick2.Left = 127;
            thick3.Right = 0;
            thick3.Top = 28;
            thick3.Bottom = 0;
            thick3.Left = 67;
            thick4.Right = 0;
            thick4.Top = 28;
            thick4.Bottom = 0;
            thick4.Left = 127;

            storyboard.Stop();
            title.Stop();
            artist.Stop();

            if (player.Height > x && player.Tag.ToString() == "exp")
            {
                changeHeight.From = player.Height;
                changeHeight.To = x;

                changeTitleMargin.From = thick2;
                changeTitleMargin.To = thick1;
                changeArtistMargin.From = thick4;
                changeArtistMargin.To = thick3;
                artist.Begin();
                title.Begin();
                storyboard.Begin();
            }
            else if (player.Height < y && player.Tag.ToString() == "nexp")
            {
                changeHeight.From = player.Height;
                changeHeight.To = y;

                changeTitleMargin.From = thick1;
                changeTitleMargin.To = thick2;
                changeArtistMargin.From = thick3;
                changeArtistMargin.To = thick4;
                artist.Begin();
                title.Begin();
                storyboard.Begin();
            }

            if (player.Tag.ToString() == "nexp")
            {
                player.Tag = "exp";
            }
            else
            {
                player.Tag = "nexp";
            }
        }
        private void fadeTab()
        {
            Storyboard storyboard = new();
            var changeOpacity = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(320)),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.6,
                From = 0,
                To = 1
            };
            storyboard.Children.Add(changeOpacity);
            Storyboard.SetTarget(storyboard, main_tabc);
            Storyboard.SetTargetProperty(storyboard, new PropertyPath(TabControl.OpacityProperty));
            storyboard.Begin();
        }
        private void rotateButton()
        {

            var x = exprotate.Angle / 180;

            Storyboard storyboard = new();
            var rotate = new DoubleAnimation
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                AccelerationRatio = 0.4,
                DecelerationRatio = 0.6
            };
            rotate.From = exprotate.Angle;

            if (exprotate.Angle >= 360)
            {
                rotate.To = (Math.Round(x) * 180) - 180;
            }
            else
            {
                rotate.To = (Math.Round(x) * 180) + 180;
            }

            storyboard.Children.Add(rotate);
            Storyboard.SetTargetName(storyboard, "exprotate");
            Storyboard.SetTargetProperty(storyboard, new PropertyPath(RotateTransform.AngleProperty));
            storyboard.Completed += rotationCompleted;

            storyboard.Begin(this.expandicon);

        }
        private void rotationCompleted(object? sender, EventArgs e)
        {
            expandplayerbutton.IsEnabled = true;
        }
        private void updatePreview(int TYPE)
        {
            switch (TYPE)
            {
                case 0:
                    img_Art.Source = spotify.currentTrackImageSource;
                    title_textblock.Text = spotify.currentTrack.Name;
                    artists_textblock.Text = spotify.currentTrackArtists[0];

                    CURRENT = 0;
                    break;
                case 1:
                    img_Art.Source = spotify.currentPlaylistImageSource;
                    title_textblock.Text = spotify.currentPlaylist.Name;
                    artists_textblock.Text = "";

                    CURRENT = 1;
                    break;
                case 2:
                    img_Art.Source = youtube.currentVideoImageSource;
                    title_textblock.Text = youtube.currentVideo.Title;
                    artists_textblock.Text = youtube.currentVideo.Author.Title;

                    CURRENT = 2;
                    break;
            }
            spot_changering.Visibility = Visibility.Hidden;
        }
        private void nothing()
        {
            title_textblock.Text = "Nothing!";
            artists_textblock.Text = "";
            img_Art.Source = new BitmapImage(new Uri("resources\\default_art.jpg", UriKind.Relative));
            spot_changering.Visibility = Visibility.Hidden;
        }
        private void addToQueue(FullTrack Track)
        {
            SONG x = new SONG(Track);
            list.Add(x);
        }
        private void addToQueue(Video Video)
        {
            if (AUDIOONLY)
            {
                SONG x = new SONG(Video);
                list.Add(x);
            }
            else
            {
                return;
            }
        }
        private void removeFromQueue(SONG song)
        {
            list.Remove(song);
            queue.Items.Refresh();
        }
        private void dl_controls(bool ENABLED)
        {
            if (ENABLED)
            {
                spotcancel.IsEnabled = false;
                dltb.IsEnabled = true;
                spotdl.IsEnabled = true;
                spotdl_all.IsEnabled = true;
                if (CURRENT == 2)
                {
                    audiovidtoggle.Visibility = Visibility.Visible;
                }
            }
            else
            {
                audiovidtoggle.Visibility = Visibility.Hidden;
                spotcancel.IsEnabled = true;
                dltb.IsEnabled = false;
                spotdl.IsEnabled = false;
                spotdl_all.IsEnabled = false;
            }
        }
        private async Task singleDownload()
        {
            if (CURRENT == -1)
            {
                status.Content = "No track selected!";
                return;
            }
            status.Content = "Starting...";
            spot_dlring.Visibility = Visibility.Visible;

            int dlresult = 3;

            switch (CURRENT)
            {
                case 0:
                    SONG x = new SONG(spotify.currentTrack, spotify.currentTrackArt);
                    dlresult = await DownloadTrack(x, OUTPUT, cts.Token);
                    break;
                case 2:
                    SONG y = new SONG(youtube.currentVideo, youtube.currentVideoThumbnail);
                    dlresult = await DownloadYT(y, OUTPUT, AUDIOONLY, cts.Token);
                    break;
            }
            switch (dlresult)
            {
                case 0:
                    status.Content = "Success";
                    break;
                case 1:
                    status.Content = "Cancelled";
                    break;
                case 2:
                    status.Content = "File already exists";
                    break;
                case 3:
                    status.Content = "An unknown exception occured";
                    break;
                case 4:
                    status.Content = "Nothing found!";
                    break;
                case -1:
                    status.Content = "Cancelled";
                    break;
            }
            spot_dlring.Visibility = Visibility.Hidden;
        }
        private async Task singleDownload(SONG song)
        {
            if (CURRENT == -1)
            {
                status.Content = "No track selected!";
                return;
            }
            status.Content = "Starting...";
            spot_dlring.Visibility = Visibility.Visible;

            int dlresult = 3;

            switch (CURRENT)
            {
                case 0:
                    song.Art = spotify.currentTrackArt;
                    dlresult = await DownloadTrack(song, OUTPUT, cts.Token);
                    break;
                case 2:
                    song.Art = youtube.currentVideoThumbnail;
                    dlresult = await DownloadYT(song, OUTPUT, true, cts.Token);
                    break;
            }
            switch (dlresult)
            {
                case 0:
                    status.Content = "Success";
                    break;
                case 1:
                    status.Content = "Cancelled";
                    break;
                case 2:
                    status.Content = "File already exists";
                    break;
                case 3:
                    status.Content = "An unknown exception occured";
                    break;
                case 4:
                    status.Content = "Nothing found!";
                    break;
                case -1:
                    status.Content = "Cancelled";
                    break;
            }
            spot_dlring.Visibility = Visibility.Hidden;
        }
        private void updatePlayer()
        {
            if (p.waveOut.PlaybackState != PlaybackState.Playing)
            {

                playerArt.Source = spotify.currentTrackImageSource;
                pt_textblock.Text = spotify.currentTrack.Name;
                pa_textblock.Text = spotify.currentTrackArtists[0];
                if (!string.IsNullOrWhiteSpace(spotify.currentTrack.PreviewUrl))
                {
                    player_controls(true);
                    CURRENTPREVURL = spotify.currentTrack.PreviewUrl;
                    p.loadpreview(CURRENTPREVURL);
                }
                else
                {
                    CURRENTPREVURL = "";
                    player_controls(false);
                }
            }
        }
        private void player_controls(bool ENABLED)
        {
            if (ENABLED)
            {
                img_previewb.Opacity = 1;
                playbackposition.Opacity = 1;
                previewb.IsEnabled = true;
                playbackposition.IsEnabled = true;
            }
            else
            {
                img_previewb.Opacity = 0.5;
                playbackposition.Opacity = 0.5;
                previewb.IsEnabled = false;
                playbackposition.IsEnabled = false;
            }
        }
        public async Task<string> plainsearch()
        {
            string searchquery = dltb.Text;
            try
            {
                if (string.IsNullOrWhiteSpace(searchquery))
                {
                    return "";
                }
                var searchresult = await Task.Run(() => spotify.SearchTrack(searchquery));
                return searchresult;
            }
            catch
            {
                return "";
            }
        }
        private void playbackFinished()
        {
            img_previewb.Source = new BitmapImage(new Uri("resources\\icon_play.png", UriKind.Relative));
            p.rewind();
            playbacktimer.Stop();

            previewb.Tag = "np";
        }
        private async void authenticate()
        {
            authstatus.Content = "PLEASE WAIT...";
            authloading.Visibility = Visibility.Visible;
            string ID = textboxID.Password;
            string SECRET = textboxSecret.Password;

            spotify = new Spotify(ID, SECRET);
            await Task.Run(() => spotify.Auth());

            Debug.WriteLine("--Auth Attempt Complete!--");
            if (spotify.authd)
            {
                authb_img.Source = new BitmapImage(new Uri("resources\\icon_unlock.png", UriKind.Relative));
                client.ID = ID;
                client.SECRET = SECRET;
                client.Save();
                dltb.IsEnabled = true;
                authstatus.Content = "AUTHENTICATED";
            }
            else
            {
                authb_img.Source = new BitmapImage(new Uri("resources\\icon_lock.png", UriKind.Relative));
                dltb.IsEnabled = false;
                authstatus.Content = "INVALID CLIENT ID OR SECRET";
            }
            authloading.Visibility = Visibility.Hidden;
        }
    }
}