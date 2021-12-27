using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using TagLib;
using YoutubeExplode.Videos;

namespace MP3DL.Media
{
    public class YouTubeVideo : IMedia
    {
        public YouTubeVideo(Video Video, bool IsVideo)
        {
            this.Video = Video;
            Title = Video.Title;
            Authors = new string[1] { Video.Author.Title };
            PrintedAuthors = PrintAuthors();

            Number = 1;
            Year = Video.UploadDate.Year.ToString();

            Duration = Video.Duration.Value.TotalMilliseconds;
            ID = Video.Id;
            this.IsVideo = IsVideo;
        }
        public YouTubeVideo(YouTubeVideo Video)
        {
            this.Video = Video.Video;
            Title = Video.Title;
            Authors = Video.Authors;
            PrintedAuthors = Video.PrintedAuthors;

            Number = Video.Number;
            Year = Video.Year;

            Duration = Video.Duration;
            ID = Video.ID;
            IsVideo = Video.IsVideo;
        }
        public string Name
        {
            get
            {
                return $"{FirstAuthor} - {Title}";
            }
        }
        private Video Video { get; set; }
        public string Title { get; set; }

        public string[] Authors { get; private set; }
        public string Album { get; set; }

        public Image? Art { get; set; }

        public uint Number { get; private set; }

        public double Duration { get; private set; }

        public string ID { get; private set; }

        public string Year { get; private set; }

        public string PrintedAuthors { get; set; }

        public string FirstAuthor
        {
            get { return FirstFromPrinted(); }
        }
        public bool IsVideo { get; set; }

        public bool Equals(IMedia? other)
        {
            if (other == null)
                return false;

            if (this.Name == other.Name)
                return true;
            else
                return false;
        }

        public void SetTags(string Filename)
        {
            var Tagger = TagLib.File.Create(Filename);
            Tagger.Tag.Title = Title;
            Tagger.Tag.Performers = PrintedAuthorsToArray();
            Tagger.Tag.Album = Title;
            Tagger.Tag.Track = Number;
            Tagger.Tag.Year = (uint)Int32.Parse(Year);

            if (Art != null)
            {
                MemoryStream TempStream = new MemoryStream();
                Art.Save(TempStream, System.Drawing.Imaging.ImageFormat.Jpeg);

                TempStream.Position = 0;
                TagLib.Picture pic = new()
                {
                    Data = ByteVector.FromStream(TempStream),
                    Type = PictureType.FrontCover
                };

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
            List<string> templist = new();
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
        public override string ToString()
        {
            return Name;
        }

        public Image GetArt()
        {
            if (Art is null)
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(LibUtils.IsolateJPG(Video.Thumbnails[0].Url));
                System.Drawing.Image thumbnail = System.Drawing.Image.FromStream(stream);
                Art = LibUtils.CropToSquare(thumbnail, 100);
                return Art;
            }
            else
            {
                return Art;
            }
        }
    }
}
