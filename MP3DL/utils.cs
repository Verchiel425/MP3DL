using System;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace MP3DL
{
    class Utils
    {
        public static string SpotifyID(string LINK)
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
        public static string YouTubeID(string LINK)
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
        public static int GetLinkType(string LINK)
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
            tmp = tmp.Replace('?', ' ');
            return tmp;
        }
        public static BitmapImage ToBitmapImage(Image Image)
        {
            Bitmap tempbitmap = (Bitmap)Image;
            BitmapImage tempbitmapimage = new BitmapImage();


            MemoryStream tempstream = new MemoryStream();

            tempbitmap.Save(tempstream, System.Drawing.Imaging.ImageFormat.Png);
            tempstream.Position = 0;

            tempbitmapimage.BeginInit();
            tempbitmapimage.CacheOption = BitmapCacheOption.None;
            tempbitmapimage.UriSource = null;
            tempbitmapimage.StreamSource = tempstream;
            tempbitmapimage.EndInit();

            return tempbitmapimage;
        }
    }
}