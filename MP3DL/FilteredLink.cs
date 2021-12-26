namespace MP3DL
{
    public enum LinkType
    {
        SpotifyTrack,
        SpotifyPlaylist,
        SpotifyAlbum,
        YouTubeVideo,
        YouTubePlaylist,
        PlainText
    }
    public class FilteredLink
    {
        public FilteredLink(string FilteredLink, LinkType Type)
        {
            this.Link = FilteredLink;
            this.Type = Type;
        }
        public string Link { get; set; }
        public LinkType Type { get; set; }
    }
}
