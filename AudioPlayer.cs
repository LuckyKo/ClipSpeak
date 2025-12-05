using System;
using System.IO;
using System.Threading;
using NAudio.Wave;

namespace ClipSpeak
{
    public class AudioPlayer : IDisposable
    {
        private IWavePlayer _waveOut;
        private WaveStream _waveStream;
        private readonly SynchronizationContext _syncContext;

        public event EventHandler PlaybackStopped;

        public AudioPlayer()
        {
            _syncContext = SynchronizationContext.Current;
        }

        private float _volume = 1.0f;
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Max(0.0f, Math.Min(1.0f, value));
                if (_waveOut != null)
                {
                    _waveOut.Volume = _volume;
                }
            }
        }

        public void Play(Stream audioStream)
        {
            Stop(); // Stop any current playback

            try
            {
                // NAudio needs a seekable stream for some readers, but MemoryStream is fine.
                // If the API returns a non-seekable stream, copy to MemoryStream.
                Stream seekableStream = audioStream;
                if (!audioStream.CanSeek)
                {
                    var ms = new MemoryStream();
                    audioStream.CopyTo(ms);
                    ms.Position = 0;
                    seekableStream = ms;
                }

                // Try to detect format. Mp3FileReader or WaveFileReader.
                // For simplicity, assuming MP3 since we requested it. 
                // A more robust way is to use MediaFoundationReader which handles most formats.
                
                try 
                {
                    _waveStream = new Mp3FileReader(seekableStream);
                }
                catch (InvalidDataException)
                {
                    // Fallback to WaveFileReader if MP3 fails (maybe it's WAV)
                    seekableStream.Position = 0;
                    _waveStream = new WaveFileReader(seekableStream);
                }

                _waveOut = new WaveOutEvent();
                _waveOut.Volume = _volume;
                _waveOut.Init(_waveStream);
                _waveOut.PlaybackStopped += OnPlaybackStopped;
                _waveOut.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error playing audio: {ex.Message}");
                Stop();
            }
        }

        public void Stop()
        {
            if (_waveOut != null)
            {
                _waveOut.Stop();
                _waveOut.Dispose();
                _waveOut = null;
            }
            if (_waveStream != null)
            {
                _waveStream.Dispose();
                _waveStream = null;
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            PlaybackStopped?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
