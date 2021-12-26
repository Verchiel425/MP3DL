using SpotifyAPI.Web;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace MP3DL.Media
{
    public class SpotifyPlaylist : IMediaCollection<SpotifyTrack>
    {
        public SpotifyPlaylist(FullPlaylist Playlist)
        {
            Title = Playlist.Name;
            Author = Playlist.Owner.DisplayName;

            WebClient TempClient = new();
            Stream ImageStream = TempClient.OpenRead(Playlist.Images[0].Url);
            Art = System.Drawing.Image.FromStream(ImageStream);

            ID = Playlist.Id;
            MediaCount = (uint)Playlist.Tracks.Total;
        }
        public string Title { get; private set; }
        public string Author { get; private set; }
        public System.Drawing.Image? Art { get; set; }
        public string ID { get; private set; }
        public uint MediaCount { get; private set; }
        public List<SpotifyTrack> Medias { get; internal set; }
    }
}
