using MP3DL.Media;
using System.Windows;

namespace MP3DL
{
    /// <summary>
    /// Interaction logic for YouTubeVideoDialog.xaml
    /// </summary>
    public partial class YouTubeVideoDialog : Window
    {
        public YouTubeVideoDialog(YouTubeVideo Video, double Width, double Height)
        {
            InitializeComponent();

            this.Video = Video;
            this.Height = Height;
            this.Width = Width;
            VideoFormat = Video.Type;
            if (VideoFormat == MediaType.Audio)
            {
                FormatToggle.Content = "Format: MP3";
            }
            else
            {
                FormatToggle.Content = "Format: MP4";
            }

            WebBrowser.Address = $"www.youtube.com/embed/{Video.ID}";
        }
        public YouTubeVideo Video;
        public MediaType VideoFormat { get; set; }

        private void Confirm_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void FormatToggle_Clicked(object sender, RoutedEventArgs e)
        {
            if (VideoFormat == MediaType.Audio)
            {
                VideoFormat = MediaType.Video;
                FormatToggle.Content = "Format: MP4";
            }
            else
            {
                VideoFormat = MediaType.Audio;
                FormatToggle.Content = "Format: MP3";
            }
        }
    }
}
