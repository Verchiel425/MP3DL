using System;
using System.Threading.Tasks;
using YoutubeExplode;

namespace MP3DL.Media
{
    public class YouTube
    {
        private YoutubeClient Client;
        public YouTubeVideo CurrentVideo { get; private set; }
        public async Task SetCurrentVid(string URL)
        {
            try
            {
                Client = new YoutubeClient();
                var temp = await Client.Videos.GetAsync(URL);
                CurrentVideo = new(temp, false);
            }
            catch
            {
                throw new ArgumentException("Invalid URL");
            }
        }
    }
}
