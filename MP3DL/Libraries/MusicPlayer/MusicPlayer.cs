using NAudio.Extras;
using NAudio.Wave;
using System;
using System.IO;
using System.Net;
using System.Windows.Threading;

namespace MP3DL
{
    class MusicPlayer
    {
        public MusicPlayer()
        {
            Volume = 0.8f;
            PlaybackPositionMonitor = new();
            PlaybackPositionMonitor.Interval = TimeSpan.FromMilliseconds(10);
            PlaybackPositionMonitor.Tick += PlaybackPositionMonitor_Tick;
        }
        private float _Volume;
        public float Volume
        {
            get { return _Volume; }
            set
            {
                _Volume = value;
                WaveOut.Volume = value;
            }
        }
        public bool HasLoadedMedia { get; set; } = false;
        public TimeSpan Duration { get; set; }
        public WaveOutEvent WaveOut = new WaveOutEvent();
        public WaveStream WaveChannel;
        public event EventHandler<PlaybackPositionEventArgs> PlaybackPositionChanged;
        public event EventHandler ResumedPlayback;
        public event EventHandler StoppedPlayback;
        public event EventHandler PlaybackFinished;
        private DispatcherTimer PlaybackPositionMonitor;
        public void LoadPreview(string url, bool bassboost)
        {
            if (WaveOut.PlaybackState == PlaybackState.Paused)
            {
                WaveOut.Stop();
            }
            var ms = new MemoryStream();
            Stream urlstream = WebRequest.Create(url).GetResponse().GetResponseStream();
            byte[] buffer = new byte[32768];
            int read;
            while ((read = urlstream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            ms.Position = 0;
            var blockAlignedStream =
               new BlockAlignReductionStream(
                   WaveFormatConversionStream.CreatePcmStream(
                       new Mp3FileReader(ms)));
            WaveChannel = new WaveChannel32(blockAlignedStream);


            WaveOut.DesiredLatency = 100;
            if (!bassboost)
            {
                WaveOut.Init(WaveChannel);
            }
            else
            {
                var filter = ApplyBass(WaveChannel.ToSampleProvider());
                WaveOut.Init(filter);
            }

            Duration = WaveChannel.TotalTime;
        }
        public void LoadMedia(string filename, bool bassboost)
        {
            if (WaveOut.PlaybackState == PlaybackState.Paused)
            {
                WaveOut.Dispose();
                WaveOut.Stop();
            }
            var reader = new Mp3FileReader(filename);
            WaveChannel = new WaveChannel32(reader);


            WaveOut.DesiredLatency = 100;

            if (!bassboost)
            {
                WaveOut.Init(WaveChannel);
            }
            else
            {
                var filter = ApplyBass(WaveChannel.ToSampleProvider());
                WaveOut.Init(filter);
            }
            Duration = WaveChannel.TotalTime;
        }
        public void Play()
        {
            PlaybackPositionMonitor.Start();
            WaveOut.Volume = Volume;
            WaveOut.Play();
            OnResumedPlayback();
        }
        public void Pause()
        {
            PlaybackPositionMonitor.Stop();
            WaveOut.Pause();
            OnStoppedPlayback();
        }
        public void Stop()
        {
            PlaybackPositionMonitor.Stop();
            WaveOut.Pause();
            OnStoppedPlayback();
        }
        public void Seek(int position)
        {
            PlaybackPositionMonitor.Stop();
            WaveOut.Pause();
            if (position == 0)
            {
                position += 1;
            }
            WaveChannel.CurrentTime = TimeSpan.FromMilliseconds(position);
        }
        public void Rewind()
        {
            PlaybackPositionMonitor.Stop();
            WaveOut.Pause();
            OnStoppedPlayback();
            WaveChannel.CurrentTime = TimeSpan.Zero;
            OnPlaybackPositionChanged();
        }
        private static Equalizer ApplyBass(ISampleProvider source)
        {
            var bands = new EqualizerBand[]
                   {
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 125, Gain = 2},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 200, Gain = 5},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 400, Gain = 2},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 800, Gain = 1},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 1200, Gain = 0},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 2400, Gain = -3},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 4800, Gain = -2},
                        new EqualizerBand {Bandwidth = 0.8f, Frequency = 9600, Gain = -3},
                        new EqualizerBand {Bandwidth = 0.9f, Frequency = 14000, Gain = -3},
                   };
            var equalizer = new Equalizer(source, bands);
            return equalizer;
        }
        private void PlaybackPositionMonitor_Tick(object? sender, EventArgs e)
        {
            //System.Diagnostics.Debug.($"Current: {WaveChannel.CurrentTime.TotalMilliseconds}, Total: {WaveChannel.TotalTime.TotalMilliseconds}");
            if (WaveChannel.CurrentTime.TotalMilliseconds >= Duration.TotalMilliseconds)
            {
                OnPlaybackFinished();
            }
            else
            {
                OnPlaybackPositionChanged();
            }
        }
        protected virtual void OnPlaybackPositionChanged()
        {
            PlaybackPositionChanged?.Invoke(this,
                new PlaybackPositionEventArgs()
                { PlaybackPositionMs = WaveChannel.CurrentTime.TotalMilliseconds });
        }
        protected virtual void OnResumedPlayback()
        {
            ResumedPlayback?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnStoppedPlayback()
        {
            StoppedPlayback?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnPlaybackFinished()
        {
            PlaybackFinished?.Invoke(this, EventArgs.Empty);
        }
    }
    public class PlaybackPositionEventArgs : EventArgs
    {
        public double PlaybackPositionMs { get; set; }
    }
}
