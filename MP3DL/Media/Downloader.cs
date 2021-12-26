using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Converter;
using YoutubeExplode.Search;

namespace MP3DL.Media
{
    public enum Result
    {
        Success,
        DuplicateFile,
        NoMediaFound,
        Cancelled,
        FailedRequest,
        NotDetermined
    }
    public class Downloader
    {
        public Downloader()
        {
            this.Output = "Downloads";
            CancelToken = new CancellationTokenSource().Token;
            Client = new();
        }
        public event EventHandler<DownloadCompleteEventArgs> DownloadCompleted;
        public event EventHandler<DownloadProgressEventArgs> ProgressChanged;
        private YoutubeClient Client;
        public CancellationToken CancelToken;
        private Result _DownloadResult { get; set; }
        public Result DownloadResult
        {
            get { return _DownloadResult; }
            set
            {
                _DownloadResult = value;
                OnDownloadCompleted();
            }
        }
        private double _Progress { get; set; }
        private double Progress
        {
            get { return _Progress; }
            set
            {
                _Progress = value;
                OnProgressChanged();
            }
        }
        private IMedia CurrentlyDownloading { get; set; }
        public string Output { get; set; }
        public async Task DownloadMedia(IMedia Media)
        {
            if (Media is SpotifyTrack Track)
            {
                await DownloadMedia(Track);
            }
            else if (Media is YouTubeVideo Video)
            {
                await DownloadMedia(Video);
            }
        }
        public async Task DownloadMedia(SpotifyTrack Track)
        {
            CurrentlyDownloading = Track;

            Client = new YoutubeClient();
            string CleanFilename = LibUtils.ClearChars(Track.Name);
            var Progress = new Progress<double>(p => this.Progress = p);

            if (File.Exists(System.IO.Path.Combine(Output, $"{CleanFilename}.mp3")))
            {
                this.Progress = 1;
                DownloadResult = Result.DuplicateFile;
                return;
            }

            var SearchResult = await Task.Run(() => Search(CleanFilename + " Audio", Track.Duration));
            if (string.IsNullOrWhiteSpace(SearchResult))
            {
                SearchResult = await Task.Run(() => Search(CleanFilename, Track.Duration));

                if (string.IsNullOrWhiteSpace(SearchResult))
                {
                    this.Progress = 1;
                    DownloadResult = Result.NoMediaFound;
                    return;
                }
            }

            try
            {
                await Client.Videos.DownloadAsync
                    (SearchResult,
                    Path.Combine(Output, $"{CleanFilename}.mp3"), Progress, CancelToken);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                this.Progress = 1;
                DownloadResult = Result.FailedRequest;
                return;
            }
            catch (TaskCanceledException)
            {
                this.Progress = 1;
                DownloadResult = Result.Cancelled;
                return;
            }
            catch
            {
                this.Progress = 1;
                DownloadResult = Result.NotDetermined;
                return;
            }

            Track.SetTags(System.IO.Path.Combine(Output, $"{CleanFilename}.mp3"));

            DownloadResult = Result.Success;
            return;
        }
        public async Task DownloadMedia(YouTubeVideo Video)
        {
            CurrentlyDownloading = Video;
            Client = new YoutubeClient();

            string FileExtension = Video.Type switch
            {
                MediaType.Video => "mp4",
                MediaType.Audio => "mp3",
                _ => "mp3",
            };
            string CleanFilename = LibUtils.ClearChars(Video.Name);
            var Progress = new Progress<double>(p => this.Progress = p);

            if (File.Exists(System.IO.Path.Combine(Output, $"{CleanFilename}.{FileExtension}")))
            {
                this.Progress = 1;
                DownloadResult = Result.DuplicateFile;
                return;
            }

            try
            {
                await Client.Videos.DownloadAsync
                            (Video.ID, System.IO.Path.Combine(Output,
                            $"{CleanFilename}.{FileExtension}"), Progress, CancelToken);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                this.Progress = 1;
                DownloadResult = Result.FailedRequest;
                return;
            }
            catch (TaskCanceledException)
            {
                this.Progress = 1;

                DownloadResult = Result.Cancelled;
                return;
            }
            catch
            {
                this.Progress = 1;
                DownloadResult = Result.NotDetermined;
                return;
            }

            Video.SetTags(System.IO.Path.Combine(Output, $"{CleanFilename}.{FileExtension}"));

            DownloadResult = Result.Success;
            return;
        }
        private async Task<string> Search(string SearchQuery, double Duration)
        {
            int Results = 5;
            VideoSearchResult Result;
            var TempClient = new YoutubeClient();
            string URL = "";
            var Videos = await TempClient.Search.GetVideosAsync(SearchQuery).CollectAsync(Results);
            for (int i = 0; i < Results; i++)
            {
                Result = Videos[i];
                TimeSpan ts = (TimeSpan)Result.Duration;
                URL = Result.Url;
                if (ts.TotalMilliseconds < Duration + 7200 && ts.TotalMilliseconds > Duration - 4000)
                {
                    break;
                }
                else
                {
                    URL = "";
                }
            }

            return URL;
        }
        protected virtual void OnProgressChanged()
        {
            ProgressChanged?.Invoke(this,
                    new DownloadProgressEventArgs()
                    {
                        Progress = Progress,
                        IsVideo = CurrentlyDownloading.IsVideo
                    });
        }
        protected virtual void OnDownloadCompleted()
        {
            DownloadCompleted?.Invoke(this,
                    new DownloadCompleteEventArgs()
                    {
                        Result = DownloadResult
                    });
        }
    }
    public class DownloadProgressEventArgs : EventArgs
    {
        public double Progress { get; set; }
        public bool IsVideo { get; set; }
    }
    public class DownloadCompleteEventArgs : EventArgs
    {
        public Result Result { get; set; }
    }
}
