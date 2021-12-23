using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using TagLib;
using YoutubeExplode.Videos;

namespace MP3DL.Libraries
{
    public class YouTubeVideo : IMedia
    {
        public YouTubeVideo(Video Video, Type Type)
        {
            this.Video = Video;
            Title = Video.Title;
            Authors = new string[1] { Video.Author.Title };
            PrintedAuthors = PrintAuthors();

            Number = 1;
            Year = Video.UploadDate.Year.ToString();

            Duration = Video.Duration.Value.TotalMilliseconds;
            ID = Video.Id;
            this.Type = Type;

            if (this.Type == Type.Video)
            {
                IsVideo = true;
            }
            else
            {
                IsVideo = false;
            }
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

        public Image? Art
        {
            get
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(LibUtils.IsolateJPG(Video.Thumbnails[0].Url));
                System.Drawing.Image thumbnail = System.Drawing.Image.FromStream(stream);
                return LibUtils.CropToSquare(thumbnail, 100);
            }
        }

        public uint Number { get; private set; }

        public double Duration { get; private set; }

        public string ID { get; private set; }

        public string Year { get; private set; }

        public string PrintedAuthors { get; set; }

        public string FirstAuthor
        {
            get { return FirstFromPrinted(); }
        }
        public Type Type { get; private set; }
        public bool IsVideo { get; private set; }

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
                TagLib.Picture pic = new TagLib.Picture();
                pic.Data = ByteVector.FromStream(TempStream);
                pic.Type = PictureType.FrontCover;

                Tagger.Tag.Pictures = new IPicture[] { pic };
            }

            Tagger.Save();
            Tagger.Dispose();
        }
        public void SetAsVideo()
        {
            Type = Type.Video;
            IsVideo = true;
        }
        public void SetAsAudio()
        {
            Type = Type.Audio;
            IsVideo = false;
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
        public override string ToString()
        {
            return Name;
        }
    }
}
