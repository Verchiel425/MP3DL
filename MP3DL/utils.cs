using NAudio.Wave;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TagLib;
using YoutubeExplode;
using YoutubeExplode.Videos;

namespace MP3DL
{
    class Spotify
    {
        public Spotify(string ID, string SECRET)
        {
            client_id = ID;
            client_secret = SECRET;
        }

        private SpotifyClient _spotify;
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public bool authd { get; set; } = false;
        public FullTrack currentTrack { get; set; }
        public FullPlaylist currentPlaylist { get; set; }
        public string[] currentTrackArtists
        {
            get { return ((IEnumerable<SimpleArtist>)currentTrack.Artists).Select(p => p.Name).ToArray(); }
        }
        public List<string> currentPlaylistTracksID
        {
            get
            {
                List<string> IDs = new List<string>();
                foreach (PlaylistTrack<IPlayableItem> item in currentPlaylist.Tracks.Items)
                {
                    if (item.Track is FullTrack track)
                    {
                        IDs.Add(track.Id);
                    }
                }
                return IDs;
            }
        }
        public System.Drawing.Image currentTrackArt { get; set; }
        public System.Drawing.Image currentPlaylistArt { get; set; }
        public BitmapImage currentTrackImageSource { get; set; }
        public BitmapImage currentPlaylistImageSource { get; set; }
        public async Task<int> Auth()
        {
            try
            {
                var config = SpotifyClientConfig.CreateDefault();
                var request = new ClientCredentialsRequest(client_id, client_secret);
                var response = await new OAuthClient(config).RequestToken(request);

                _spotify = new SpotifyClient(config.WithToken(response.AccessToken));
                Debug.WriteLine("--Auth Success!--");
                authd = true;
                return 0;
            }
            catch (Exception)
            {
                Debug.WriteLine("--Auth Fail!--");
                authd = false;
                return 1;
            }
        }
        public async Task<int> SetTrack(string TRACK_ID)
        {
            try
            {
                currentTrack = await _spotify.Tracks.Get(TRACK_ID);

                FullAlbum simpleAlbum = new FullAlbum();
                WebClient client = new();

                simpleAlbum = await _spotify.Albums.Get(currentTrack.Album.Id);
                SpotifyAPI.Web.Image AlbumCover = simpleAlbum.Images[0];
                Stream imageStream = client.OpenRead(AlbumCover.Url);
                currentTrackArt = System.Drawing.Image.FromStream(imageStream);

                Bitmap image = (Bitmap)currentTrackArt;
                currentTrackImageSource = new BitmapImage();

                MemoryStream memstream = new();
                image.Save(memstream, System.Drawing.Imaging.ImageFormat.Jpeg);
                memstream.Position = 0;

                currentTrackImageSource.BeginInit();
                currentTrackImageSource.CacheOption = BitmapCacheOption.OnDemand;
                currentTrackImageSource.UriSource = null;
                currentTrackImageSource.StreamSource = memstream;
                currentTrackImageSource.EndInit();

                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }
        public async Task<FullTrack?> QueueTrack(string TRACK_ID)
        {
            try
            {
                FullTrack x = await _spotify.Tracks.Get(TRACK_ID);

                return x;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public string PrintArtists()
        {
            string print = "";
            foreach (string artist in currentTrackArtists)
            {
                print = $"{print}, {artist}";
            }
            return print.Substring(2, print.Length - 2);
        }
        public async Task<int> SetPlaylist(string PLAYLIST_ID)
        {
            try
            {
                WebClient client = new WebClient();
                currentPlaylist = await _spotify.Playlists.Get(PLAYLIST_ID);

                Stream imageStream = client.OpenRead(currentPlaylist.Images[0].Url);
                currentPlaylistArt = System.Drawing.Image.FromStream(imageStream);

                Bitmap image = (Bitmap)currentPlaylistArt;
                currentPlaylistImageSource = new BitmapImage();

                MemoryStream memstream = new();
                image.Save(memstream, System.Drawing.Imaging.ImageFormat.Jpeg);
                memstream.Position = 0;

                currentPlaylistImageSource.BeginInit();
                currentPlaylistImageSource.CacheOption = BitmapCacheOption.OnDemand;
                currentPlaylistImageSource.UriSource = null;
                currentPlaylistImageSource.StreamSource = memstream;
                currentPlaylistImageSource.EndInit();

                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }
        public async Task<string> SearchTrack(string SearchQuery)
        {
            var search = await _spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, SearchQuery));
            var item = search.Tracks.Items[0].Id;
            return item;
        }
        public async Task<string> SearchPlaylist(string SearchQuery)
        {
            var search = await _spotify.Search.Item(new SearchRequest(SearchRequest.Types.Playlist, SearchQuery));
            var item = search.Playlists.Items[0].Id;
            return item;
        }
        public async Task<System.Drawing.Image> GetArtFromTrack(string TRACK_ID)
        {
            FullTrack newtrack = await _spotify.Tracks.Get(TRACK_ID);

            FullAlbum simpleAlbum = new FullAlbum();
            WebClient client = new WebClient();

            simpleAlbum = await _spotify.Albums.Get(newtrack.Album.Id);
            SpotifyAPI.Web.Image albumcover = simpleAlbum.Images[0];
            Stream imageStream_ = client.OpenRead(albumcover.Url);
            System.Drawing.Image coverart_ = System.Drawing.Image.FromStream(imageStream_);

            return coverart_;
        }
    }
    class YouTube
    {
        YoutubeClient youtube;
        public Video currentVideo { get; set; }
        public System.Drawing.Image currentVideoThumbnail { get; set; } = null;
        public BitmapImage currentVideoImageSource { get; set; }
        public async Task<int> SetVid(string url)
        {
            try
            {
                youtube = new YoutubeClient();
                currentVideo = await youtube.Videos.GetAsync(url);
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(utils.JPG(currentVideo.Thumbnails[0].Url));
                System.Drawing.Image thumbnail = System.Drawing.Image.FromStream(stream);
                currentVideoThumbnail = utils.PadImage(thumbnail);

                Bitmap image = (Bitmap)currentVideoThumbnail;
                currentVideoImageSource = new BitmapImage();

                MemoryStream memstream = new();
                image.Save(memstream, System.Drawing.Imaging.ImageFormat.Jpeg);
                memstream.Position = 0;

                currentVideoImageSource.BeginInit();
                currentVideoImageSource.CacheOption = BitmapCacheOption.OnDemand;
                currentVideoImageSource.UriSource = null;
                currentVideoImageSource.StreamSource = memstream;
                currentVideoImageSource.EndInit();

                return 0;
            }
            catch
            {
                return 1;
            }
        }
    }
    class SONG
    {
        public string Name { get; } = "";
        public string Title { get; set; } = "";
        public string[] Artists { get; set; } = new string[1] { "" };
        public string Album { get; set; } = "";
        public uint DiscNo { get; set; } = 0;
        public string PrintedArtists { get; set; } = "";
        public string Id { get; set; } = "";
        public int Duration { get; set; } = 0;
        public System.Drawing.Image? Art { get; set; } = null;
        public int Type { get; set; }

        public SONG(string title, string[] artists, string album, int discno, string id)
        {
            Title = title;
            Artists = artists;
            Album = album;
            DiscNo = (uint)discno;
            Name = $"{Artists[0]} - {Title}";
            PrintedArtists = PrintArtists();
            Id = id;
        }
        public SONG(string title, string artist, string album, int discno, string id)
        {
            Title = title;
            Artists = new string[1] { artist };
            Album = album;
            DiscNo = (uint)discno;
            Name = $"{Artists[0]} - {Title}";
            PrintedArtists = artist;
            Id = id;
        }
        public SONG(FullTrack track)
        {
            Title = track.Name;
            Artists = ((IEnumerable<SimpleArtist>)track.Artists).Select(p => p.Name).ToArray();
            Album = track.Album.Name;
            DiscNo = (uint)track.TrackNumber;
            Name = $"{Artists[0]} - {Title}";
            PrintedArtists = PrintArtists();
            Id = track.Id;
            Duration = track.DurationMs;
            Type = 0;
        }
        public SONG(FullTrack track, System.Drawing.Image art)
        {
            Title = track.Name;
            Artists = ((IEnumerable<SimpleArtist>)track.Artists).Select(p => p.Name).ToArray();
            Album = track.Album.Name;
            DiscNo = (uint)track.TrackNumber;
            Name = $"{Artists[0]} - {Title}";
            PrintedArtists = PrintArtists();
            Id = track.Id;
            Duration = track.DurationMs;
            Art = art;
            Type = 0;
        }
        public SONG(Video video, System.Drawing.Image art)
        {
            Title = video.Title;
            Artists = new string[1] { video.Author.Title };
            Album = video.Title;
            DiscNo = 1;
            Name = $"{Artists[0]} - {Title}";
            PrintedArtists = PrintArtists();
            Id = video.Id;
            if (video.Duration != null)
            {
                Duration = (int)video.Duration.Value.TotalMilliseconds;
            }
            Art = art;
            Type = 1;
        }
        public SONG(Video video)
        {
            Title = video.Title;
            Artists = new string[1] { video.Author.Title };
            Album = video.Title;
            DiscNo = 1;
            Name = $"{Artists[0]} - {Title}";
            PrintedArtists = PrintArtists();
            Id = video.Id;
            if (video.Duration != null)
            {
                Duration = (int)video.Duration.Value.TotalMilliseconds;
            }
            Type = 1;
        }
        public SONG() { }
        public void setTags(string FILENAME)
        {
            var ts = TagLib.File.Create(FILENAME);
            ts.Tag.Title = Title;
            ts.Tag.Performers = StringToArray(PrintedArtists);
            ts.Tag.Album = Album;
            ts.Tag.Track = DiscNo;

            if (Art != null)
            {
                MemoryStream ms = new MemoryStream();
                Art.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

                ms.Position = 0;
                TagLib.Picture pic = new TagLib.Picture();
                pic.Data = ByteVector.FromStream(ms);
                pic.Type = PictureType.FrontCover;

                ts.Tag.Pictures = new IPicture[] { pic };
            }

            ts.Save();
        }
        public string PrintArtists()
        {
            string print = "";
            foreach (string artist in Artists)
            {
                print = $"{print}, {artist}";
            }
            return print.Substring(2, print.Length - 2);
        }
        public string[] StringToArray(string str)
        {
            List<string> tmp = new List<string>();
            string element = "";
            int i = 0;

            if (!str.EndsWith(","))
            {
                str = str + ",";
            }
            while (str.Contains(","))
            {
                int x = str.IndexOf(",");
                element = str.Substring(0, x);
                str = str.Substring(x + 1, str.Length - x - 1);
                if (element.StartsWith(" "))
                {
                    element = element.Substring(1, element.Length - 1);
                }
                tmp.Add(element);
                i++;
            }
            return tmp.ToArray();
        }
        public string FirstFromPrinted(string str)
        {
            if (!str.EndsWith(","))
            {
                str = str + ",";
            }
            int x = str.IndexOf(",");
            string element = str.Substring(0, x);

            if (element.StartsWith(" "))
            {
                element = element.Substring(1, element.Length - 1);
            }
            return element;
        }
    }
    class check
    {
        public static bool IsSpotifyLink(string LINK)
        {
            if (LINK.Contains("open.spotify.com"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static int LinkType(string LINK)
        {
            if (LINK.Contains("/track/"))
            {
                return 0;
            }
            else if (LINK.Contains("/playlist/"))
            {
                return 1;
            }
            else if (LINK.Contains("youtube.com") || LINK.Contains("youtu.be"))
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
    }
    class _CLIENT
    {
        public string EncryptedID { get; set; }
        public string EncryptedSecret { get; set; }
        public string DownloadsFolder { get; set; }
        public bool AutoAuth { get; set; }
    }
    class CLIENT
    {
        private string name = "client.json";
        public string SECRET = "";
        public string ID = "";
        public string OUTPUT = "";
        public bool AUTOAUTH;
        public void Save()
        {

            Debug.WriteLine("--Saving to config--");
            Debug.WriteLine($"--CLIENTID--");
            Debug.WriteLine($"--CLIENTSECRET--");
            Debug.WriteLine($"--{OUTPUT}--");
            Debug.WriteLine($"--{AUTOAUTH}--");
            _CLIENT _client = new _CLIENT
            {
                EncryptedID = cryptography.Encrypt(ID, "yomama"),
                EncryptedSecret = cryptography.Encrypt(SECRET, "yomama"),
                DownloadsFolder = OUTPUT,
                AutoAuth = AUTOAUTH
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_client, options);
            System.IO.File.WriteAllText(name, json);
        }
        public string GetID()
        {
            if (System.IO.File.Exists(name))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(name);
                    _CLIENT _client = JsonSerializer.Deserialize<_CLIENT>(json);
                    string ID = cryptography.Decrypt(_client.EncryptedID, "yomama");
                    return ID;
                }
                catch (System.Text.Json.JsonException)
                {
                    System.IO.File.Delete(name);
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
        public string GetSECRET()
        {
            if (System.IO.File.Exists(name))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(name);
                    _CLIENT _client = JsonSerializer.Deserialize<_CLIENT>(json);
                    string SECRET = cryptography.Decrypt(_client.EncryptedSecret, "yomama");
                    return SECRET;
                }
                catch (Exception)
                {
                    System.IO.File.Delete(name);
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
        public string GetOUTPUT()
        {
            if (System.IO.File.Exists(name))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(name);
                    _CLIENT _client = JsonSerializer.Deserialize<_CLIENT>(json);
                    string OUTPUT = _client.DownloadsFolder;
                    return OUTPUT;
                }
                catch (Exception)
                {
                    System.IO.File.Delete(name);
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
        public bool GetAUTOAUTH()
        {
            if (System.IO.File.Exists(name))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(name);
                    _CLIENT _client = JsonSerializer.Deserialize<_CLIENT>(json);
                    bool AUTOAUTH = _client.AutoAuth;
                    return AUTOAUTH;
                }
                catch (Exception)
                {
                    System.IO.File.Delete(name);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
    class cryptography
    {
        private const int Keysize = 128;
        private const int DerivationIterations = 1000;
        public static string Encrypt(string plainText, string passPhrase)
        {
            var saltStringBytes = Generate128BitsOfRandomEntropy();
            var ivStringBytes = Generate128BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();

                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }
        public static string Decrypt(string cipherText, string passPhrase)
        {
            try
            {
                var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);

                var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();

                var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();

                var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

                using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
                {
                    var keyBytes = password.GetBytes(Keysize / 8);
                    using (var symmetricKey = new RijndaelManaged())
                    {
                        symmetricKey.BlockSize = 128;
                        symmetricKey.Mode = CipherMode.CBC;
                        symmetricKey.Padding = PaddingMode.PKCS7;
                        using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                        {
                            using (var memoryStream = new MemoryStream(cipherTextBytes))
                            {
                                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                                {
                                    var plainTextBytes = new byte[cipherTextBytes.Length];
                                    var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                    memoryStream.Close();
                                    cryptoStream.Close();
                                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return "";
            }
        }
        private static byte[] Generate128BitsOfRandomEntropy()
        {
            var randomBytes = new byte[16];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
    class utils
    {
        public static string ID(string LINK)
        {
            int a = LINK.LastIndexOf("/");
            int b = LINK.LastIndexOf("?");
            try
            {
                return LINK.Substring(a + 1, (b - a) - 1);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                return null;
            }
        }
        public static string YTID(string LINK)
        {
            if (LINK.Contains("youtube.com"))
            {
                return LINK;
            }
            if (LINK.Contains("youtu.be"))
            {
                int a = LINK.LastIndexOf("/");
                return LINK.Substring(a + 1, (LINK.Length - a) - 1);
            }
            return null;
        }
        public static string JPG(string LINK)
        {
            try
            {
                int a = LINK.LastIndexOf(".jpg");
                return LINK.Substring(0, a + 4);
            }
            catch
            {
                return "";
            }
        }
        public static System.Drawing.Image PadImage(System.Drawing.Image originalImage)
        {
            int squaredimensions = Math.Min(originalImage.Height, originalImage.Width);
            Size squareSize = new Size(squaredimensions, squaredimensions);
            Bitmap squareImage = new Bitmap(squareSize.Width, squareSize.Height);
            using (Graphics graphics = Graphics.FromImage(squareImage))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                graphics.DrawImage(originalImage, (squareSize.Width / 2) - (originalImage.Width / 2), (squareSize.Height / 2) - (originalImage.Height / 2), originalImage.Width, originalImage.Height);
            }
            return squareImage;
        }
        public static string ClearChars(string input)
        {
            string tmp = input;
            tmp = tmp.Replace('/', '-');
            tmp = tmp.Replace('|', ' ');
            tmp = tmp.Replace('\"', ' ');
            tmp = tmp.Replace('[', ' ');
            tmp = tmp.Replace(']', ' ');
            tmp = tmp.Replace('{', ' ');
            tmp = tmp.Replace('}', ' ');
            tmp = tmp.Replace('\'', ' ');
            tmp = tmp.Replace(',', ' ');
            tmp = tmp.Replace('.', ' ');
            tmp = tmp.Replace(':', ' ');
            return tmp;
        }
    }
    class clientplayer
    {
        private float _volume = 1f;
        public float volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                waveOut.Volume = value;
            }
        }
        public TimeSpan previewDuration { get; set; }
        public WaveOutEvent waveOut = new WaveOutEvent();
        private Stream ms;
        public WaveChannel32 wavech;
        private WaveStream blockAlignedStream;
        public clientplayer()
        {
        }

        public void loadpreview(string url)
        {
            if (waveOut.PlaybackState == PlaybackState.Paused)
            {
                waveOut.Stop();
            }
            ms = new MemoryStream();
            Stream urlstream = WebRequest.Create(url).GetResponse().GetResponseStream();
            byte[] buffer = new byte[32768];
            int read;
            while ((read = urlstream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            ms.Position = 0;
            blockAlignedStream =
               new BlockAlignReductionStream(
                   WaveFormatConversionStream.CreatePcmStream(
                       new Mp3FileReader(ms)));
            wavech = new WaveChannel32(blockAlignedStream);
            waveOut.DesiredLatency = 200;
            waveOut.Init(wavech);

            previewDuration = wavech.TotalTime;
        }
        public void play()
        {
            waveOut.Volume = volume;
            waveOut.Play();
        }
        public void pause()
        {
            waveOut.Pause();
        }
        public void stop()
        {
            waveOut.Pause();
        }
        public void seek(int position)
        {
            waveOut.Pause();
            wavech.CurrentTime = TimeSpan.FromMilliseconds(position);
        }
        public void rewind()
        {
            waveOut.Pause();
            wavech.CurrentTime = TimeSpan.Zero;
        }
    }
}
