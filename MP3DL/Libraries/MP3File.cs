using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using TagLib;
using YoutubeExplode.Videos;
using MP3DL.Libraries;

namespace MP3DL.Libraries
{
    public enum InputType
    {
        Null,
        Spotify,
        YouTubeMedia,
    }
    class MP3File : IEquatable<MP3File>, IComparable<MP3File>
    {
        public string Filename { get; set; }
        public string Year { get; set; }
        public string DateAdded { get; set; }

        public string Name { get; private set; }

        public string Title { get; private set; }

        public string[] Authors { get; private set; }

        public string PrintedAuthors { get; private set; }

        public string FirstAuthor { get; private set; }
        public string Album { get; private set; }
        public uint DiscNo { get; private set; }
        public System.Drawing.Image? Art { get; private set; }

        public uint Number { get; private set; }

        public double Duration { get; private set; }

        public string ID { get; private set; }

        public bool IsVideo { get; private set; }

        public MP3File(string Filename)
        {
            this.Filename = Filename;

            var ts = TagLib.File.Create(this.Filename);
            Title = ts.Tag.Title;
            Authors = ts.Tag.Performers;
            PrintedAuthors = PrintAuthors();
            Album = ts.Tag.Album;
            DiscNo = ts.Tag.Disc;
            Year = ts.Tag.Year.ToString();
            ID = "";
            DateAdded = System.IO.File.GetLastWriteTimeUtc(this.Filename).ToString();

            if(Year == "0")
            {
                Year = "";
            }
            ts.Dispose();
        }
        public string PrintAuthors()
        {
            if (Authors.Length == 0)
            {
                return "";
            }
            string temp = "";
            foreach (string author in Authors)
            {
                temp = $"{temp}, {author}";
            }
            return temp[2..];
        }
        public BitmapImage GetImageSource()
        {
            BitmapImage ImageSource = new();
            try
            {
                var ts = TagLib.File.Create(Filename);

                MemoryStream ms = new MemoryStream(ts.Tag.Pictures[0].Data.Data);
                System.Drawing.Image tmp = System.Drawing.Image.FromStream(ms);

                Bitmap image = (Bitmap)Utils.PadImage(tmp);

                MemoryStream memstream = new();
                image.Save(memstream, System.Drawing.Imaging.ImageFormat.Jpeg);
                memstream.Position = 0;

                ImageSource.BeginInit();
                ImageSource.CacheOption = BitmapCacheOption.None;
                ImageSource.UriSource = null;
                ImageSource.StreamSource = memstream;
                ImageSource.EndInit();

                ts.Dispose();
                return ImageSource;
            }
            catch
            {
                ImageSource = new BitmapImage(new Uri("resourecs\\default_art.jpg", UriKind.Relative));
                return ImageSource;
            }
        }

        public bool Equals(MP3File? other)
        {
            if (other == null) return false;
            return (this.Filename.Equals(other.Filename));
        }

        public int CompareTo(MP3File? other)
        {
            if (other == null)
                return 1;

            else
                return Title.CompareTo(other.Title);
        }
    }
}
