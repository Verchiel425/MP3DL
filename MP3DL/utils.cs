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
                return "";
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
            return "";
        }
        public static FilteredLink FilterLink(string LINK)
        {
            if (LINK.Contains("/track/"))
            {
                return new FilteredLink(SpotifyID(LINK), LinkType.SpotifyTrack);
            }
            else if (LINK.Contains("/playlist/"))
            {
                return new FilteredLink(SpotifyID(LINK), LinkType.SpotifyPlaylist);
            }
            else if (LINK.Contains("/album/"))
            {
                return new FilteredLink(SpotifyID(LINK), LinkType.SpotifyAlbum);
            }
            else if (LINK.Contains("youtube.com/watch") || LINK.Contains("youtu.be"))
            {
                return new FilteredLink(LINK, LinkType.YouTubeVideo);
            }
            else
            {
                return new FilteredLink(LINK, LinkType.PlainText);
            }
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