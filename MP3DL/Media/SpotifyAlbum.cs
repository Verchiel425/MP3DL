using SpotifyAPI.Web;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace MP3DL.Media
{
    internal class SpotifyAlbum : IMediaCollection<SpotifyTrack>
    {
        public SpotifyAlbum(FullAlbum Album)
        {
            Title = Album.Name;
            Author = Album.Artists[0].Name;

            WebClient TempClient = new();
            Stream ImageStream = TempClient.OpenRead(Album.Images[0].Url);
            Art = System.Drawing.Image.FromStream(ImageStream);

            ID = Album.Id;
            MediaCount = (uint)Album.Tracks.Total;
        }
        public string Title { get; private set; }

        public string Author { get; private set; }

        public System.Drawing.Image? Art { get; private set; }

        public string ID { get; private set; }

        public uint MediaCount { get; private set; }

        public List<SpotifyTrack> Medias { get; internal set; }
    }
}
