using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using TagLib;

namespace MP3DL.Libraries
{
    public class SpotifyTrack : IMedia
    {
        public SpotifyTrack(FullTrack Track, FullAlbum Album)
        {
            Title = Track.Name;
            Authors = ((IEnumerable<SimpleArtist>)Track.Artists).Select(p => p.Name).ToArray();
            PrintedAuthors = PrintAuthors();
            _Album = Album;

            this.Album = Track.Album.Name;
            Number = (uint)Track.TrackNumber;
            Year = GetReleaseYear(Track.Album.ReleaseDate);

            PreviewURL = Track.PreviewUrl;
            Duration = Track.DurationMs;
            ID = Track.Id;
        }
        public System.Drawing.Image? Art
        {
            get
            {
                WebClient TempWeb = new();
                SpotifyAPI.Web.Image AlbumCover = _Album.Images[0];
                Stream TempStream = TempWeb.OpenRead(AlbumCover.Url);
                return System.Drawing.Image.FromStream(TempStream);
            }
        }
        public string Name
        {
            get { return $"{FirstAuthor} - {Title}"; }
        }
        public string Title { get; set; }
        public string[] Authors { get; private set; }
        public string PrintedAuthors { get; set; }
        public string FirstAuthor
        {
            get {
                return FirstFromPrinted(); }
        }
        private FullAlbum _Album { get; set; }
        public string Album { get; set; }
        public uint Number { get; private set; }
        public string Year { get; private set; }
        public string ID { get; private set; }
        public string PreviewURL { get; private set; }
        public double Duration { get; private set; }
        public bool IsVideo { get; private set; } = false;
        public void SetTags(string Filename)
        {
            var Tagger = TagLib.File.Create(Filename);
            Tagger.Tag.Title = Title;
            Tagger.Tag.Performers = PrintedAuthorsToArray();
            Tagger.Tag.Album = Album;
            Tagger.Tag.Track = Number;
            Tagger.Tag.Year = (uint)Int32.Parse(Year);

            if (Art != null)
            {
                MemoryStream TempStream = new MemoryStream();
                Art.Save(TempStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                TempStream.Position = 0;
                TagLib.Picture pic = new TagLib.Picture();
                pic.Data = ByteVector.FromStream(TempStream);
                pic.Type = PictureType.FrontCover;

                Tagger.Tag.Pictures = new IPicture[] { pic };
            }

            Tagger.Save();
            Tagger.Dispose();
        }
        private string PrintAuthors()
        {
            string print = "";
            foreach (string author in Authors)
            {
                print = $"{print}, {author}";
            }
            return print.Substring(2, print.Length - 2);
        }
        private string[] PrintedAuthorsToArray()
        {
            List<string> templist = new List<string>();
            string tempstring;
            string tempprintedauthors = PrintedAuthors;
            int i = 0;

            if (!tempprintedauthors.EndsWith(","))
            {
                tempprintedauthors = tempprintedauthors + ",";
            }
            while (tempprintedauthors.Contains(","))
            {
                int x = tempprintedauthors.IndexOf(",");
                tempstring = tempprintedauthors.Substring(0, x);
                tempprintedauthors = tempprintedauthors.Substring(x + 1, tempprintedauthors.Length - x - 1);
                if (tempstring.StartsWith(" "))
                {
                    tempstring = tempstring.Substring(1, tempstring.Length - 1);
                }
                templist.Add(tempstring);
                i++;
            }
            return templist.ToArray();
        }
        private string FirstFromPrinted()
        {
            if (!PrintedAuthors.EndsWith(","))
            {
                PrintedAuthors += ",";
            }
            int x = PrintedAuthors.IndexOf(",");
            string temp = PrintedAuthors[..x];

            if (temp.StartsWith(" "))
            {
                temp = temp[1..];
            }
            return temp;
        }
        private string GetReleaseYear(string fulldate)
        {
            if (fulldate.Contains('-'))
            {
                int x = fulldate.IndexOf('-');
                return fulldate[..x];
            }
            else
            {
                return fulldate;
            }
        }
        public override string ToString()
        {
            return Name;
        }

        public bool Equals(IMedia? other)
        {
            if (other == null)
                return false;

            if (this.Name == other.Name)
                return true;
            else
                return false;
        }
    }
}
