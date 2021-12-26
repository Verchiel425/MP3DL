using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MP3DL.Media
{
    public class Spotify
    {
        public Spotify()
        {
            ClientID = "";
            ClientSecret = "";
        }
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public bool Authd { get; private set; } = false;
        public SpotifyTrack CurrentTrack { get; set; }
        public IMediaCollection<SpotifyTrack> CurrentMediaCollection { get; set; }

        private SpotifyClient Client;
        public event EventHandler<CollectionProgressEventArgs> CollectionFetchingProgressChanged;
        public event EventHandler CollectionFetchingDone;
        public async Task Auth()
        {
            try
            {
                var config = SpotifyClientConfig
                    .CreateDefault()
                    .WithRetryHandler(new SimpleRetryHandler() { RetryAfter = TimeSpan.FromSeconds(2), RetryTimes = 2 });
                var request = new ClientCredentialsRequest(ClientID, ClientSecret);
                var response = await new OAuthClient(config).RequestToken(request);

                Client = new SpotifyClient(config.WithToken(response.AccessToken));
                Debug.WriteLine("--Auth Success!--");
                Authd = true;
            }
            catch (ArgumentNullException)
            {
                Authd = false;
                throw new ArgumentNullException("Must set Client ID and Secret");
            }
            catch (Exception)
            {
                Authd = false;
                throw new ArgumentException("Invalid Client ID and Secret");
            }
        }
        public async Task SetCurrentTrack(string TRACK_ID)
        {
            try
            {
                var temp = await Client.Tracks.Get(TRACK_ID);
                CurrentTrack = new(temp, await Client.Albums.Get(temp.Album.Id));
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid ID! Please enter a valid track ID");
            }
        }
        public async Task<SpotifyTrack> GetTrack(string TRACK_ID)
        {
            try
            {
                var tempx = await Client.Tracks.Get(TRACK_ID);
                var tempy = new SpotifyTrack(tempx, await Client.Albums.Get(tempx.Album.Id));

                return tempy;
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid ID! Please enter a valid track ID");
            }
        }
        public async Task SetCurrentPlaylist(string PLAYLIST_ID)
        {
            try
            {
                var temp = await Client.Playlists.Get(PLAYLIST_ID);
                var tempplaylist = new SpotifyPlaylist(temp);
                tempplaylist.Medias = await GetCurrentCollectionTracks(temp);
                CurrentMediaCollection = tempplaylist;
                OnCollectionFetchingDone();
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid ID! Please enter a valid track ID");
            }
        }
        public async Task SetCurrentAlbum(string ALBUM_ID)
        {
            try
            {
                var temp = await Client.Albums.Get(ALBUM_ID);
                var tempalbum = new SpotifyAlbum(temp);
                tempalbum.Medias = await GetCurrentCollectionTracks(temp);
                CurrentMediaCollection = tempalbum;
                OnCollectionFetchingDone();
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid ID! Please enter a valid track ID");
            }
        }
        public async Task<string> SearchTrack(string SearchQuery, int Index)
        {
            var search = await Client.Search.Item(new SearchRequest(SearchRequest.Types.Track, SearchQuery));
            var item = search.Tracks.Items[Index].Id;
            return item;
        }
        public async Task<string> SearchPlaylist(string SearchQuery, int Index)
        {
            var search = await Client.Search.Item(new SearchRequest(SearchRequest.Types.Playlist, SearchQuery));
            var item = search.Playlists.Items[Index].Id;
            return item;
        }
        private async Task<List<SpotifyTrack>> GetCurrentCollectionTracks(FullPlaylist Playlist)
        {
            var temp = new List<SpotifyTrack>();

            Debug.WriteLine($"--{Playlist.Tracks.Total} Total IDs found in playlist--");

            int x = (int)Math.Ceiling((decimal)Playlist.Tracks.Total / 100);
            int finished = 0;
            int total = (int)Playlist.Tracks.Total;
            Debug.WriteLine($"--Playlist going through {x} loop(s)--");
            for (int i = 0; i < x; i++)
            {
                foreach (PlaylistTrack<IPlayableItem> item in Playlist.Tracks.Items)
                {
                    if (item.Track is FullTrack track)
                    {
                        var album = await Client.Albums.Get(track.Album.Id);
                        temp.Add(new SpotifyTrack(track, album));
                    }
                    finished++;
                    OnPlaylistFetchingProgressChanged(finished, total);
                }
                await Offset(Playlist, i);
            }
            return temp;
        }
        private async Task<List<SpotifyTrack>> GetCurrentCollectionTracks(FullAlbum Album)
        {
            var temp = new List<SpotifyTrack>();

            Debug.WriteLine($"--{Album.Tracks.Total} Total IDs found in playlist--");

            int x = (int)Math.Ceiling((decimal)Album.Tracks.Total / 100);
            int finished = 0;
            int total = (int)Album.Tracks.Total;
            Debug.WriteLine($"--Playlist going through {x} loop(s)--");
            for (int i = 0; i < x; i++)
            {
                foreach (SimpleTrack item in Album.Tracks.Items)
                {
                    temp.Add(new SpotifyTrack(item, Album));
                    finished++;
                    OnPlaylistFetchingProgressChanged(finished, total);
                }
                await Offset(Album, i);
            }
            return temp;
        }
        private async Task Offset(FullPlaylist Playlist, int Index)
        {
            int offset = (Index + 1) * 100;
            Playlist.Tracks = await Client.Playlists.GetItems
                (Playlist.Id,
                new PlaylistGetItemsRequest { Offset = offset });

        }
        private async Task Offset(FullAlbum Album, int Index)
        {
            int offset = (Index + 1) * 100;
            Album.Tracks = await Client.Albums.GetTracks
                (Album.Id,
                new AlbumTracksRequest { Offset = offset });

        }
        protected virtual void OnPlaylistFetchingProgressChanged(int Finished, int Total)
        {
            CollectionFetchingProgressChanged?.Invoke(this,
                new CollectionProgressEventArgs()
                {
                    Total = Total,
                    Finished = Finished,
                });
        }
        protected virtual void OnCollectionFetchingDone()
        {
            CollectionFetchingDone?.Invoke(this, EventArgs.Empty);
        }
    }
    public class CollectionProgressEventArgs : EventArgs
    {
        public int Finished { get; set; }
        public int Total { get; set; }
    }
}