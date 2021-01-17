using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Threading;
using nb3.Player.Analysis;

namespace nb3.Player
{
    /// <summary>
    /// Audio player and analyser.
    /// 
    /// Responsible for:
    /// - playing audio
    /// - play/pause/stop/seek controls
    /// - calculating spectrum(s)
    /// - emitting data for rendering
    /// 
    /// 
    /// 
    /// </summary>
    public class Player : IDisposable
    {
        private Func<IWavePlayer> outputFactory = null;
        private IWavePlayer output = null;

        //private WaveStream pcmstream = null;
        //private BlockAlignReductionStream reductionstream = null;
        private AudioFileReader2 reader = null;
        private SpectrumGenerator spectrum = null;

        // hacky exposing of filter output name list
        // TODO: This will probably throw a nullreferenceexception until a file is opened.
        public List<string> FilterOutputNames => spectrum.FilterOutputNames;

        public event EventHandler<FftEventArgs> SpectrumReady;
        public event EventHandler<PlayerStartEventArgs> PlayerStart;

        public int TracksPlayed { get; private set; } = 0;

        private object lockObj = new object();




        public Player(Func<IWavePlayer> outputFactory)
        {
            this.outputFactory = outputFactory;

        }

        public void Open(string filename)
        {
            /*
            if (output != null)
            {
                output.Stop();

                while (output.PlaybackState != PlaybackState.Stopped)
                {
                    Thread.Sleep(10);
                }
            }*/



            Dispose(true);

            //pcmstream = WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(filename));
            //reductionstream = new BlockAlignReductionStream(pcmstream);
            //output.Init(new WaveChannel32(reductionstream));
            output = outputFactory();
            reader = new AudioFileReader2(filename);
            spectrum = new SpectrumGenerator(reader);
            spectrum.SpectrumReady += Spectrum_SpectrumReady;
            output.Init(spectrum);
            TracksPlayed++;
            InvokePlayerStart(filename);
        }

        public WaveFormat WaveFormat { get { return reader?.WaveFormat; } }

        private void InvokePlayerStart(string trackName)
        {
            PlayerStartEventArgs e = new PlayerStartEventArgs()
            {
                SampleRate = reader.WaveFormat.SampleRate,
                TrackLength = reader.TotalTime,
                TrackName = trackName
            };

            PlayerStart?.Invoke(this, e);
        }

        private void Spectrum_SpectrumReady(object sender, FftEventArgs e)
        {
            SpectrumReady?.Invoke(sender, e);
        }



        public void Play()
        {
            if (output == null)
                return;

            switch (output.PlaybackState)
            {
                case PlaybackState.Stopped:
                case PlaybackState.Paused:
                    output.Play();
                    break;
                default:
                    break;
            }
        }

        public void Pause()
        {
            if (output == null)
                return;

            switch (output.PlaybackState)
            {
                case PlaybackState.Playing:
                    output.Pause();
                    break;
                default:
                    break;
            }
        }

        public void TogglePause()
        {
            if (output == null)
                return;

            switch (output.PlaybackState)
            {
                case PlaybackState.Playing:
                    output.Pause();
                    break;
                case PlaybackState.Paused:
                    output.Play();
                    break;
                default:
                    break;
            }

        }

        public void Stop()
        {
            if (output == null)
                return;

            switch (output.PlaybackState)
            {
                case PlaybackState.Playing:
                case PlaybackState.Paused:
                    output.Stop();
                    break;
                default:
                    break;
            }
        }

        public void Skip(int seconds)
        {
            if (reader == null || output == null)
                return;

            if (output.PlaybackState != PlaybackState.Stopped)
            {
                reader.Skip(seconds);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (output != null)
                {
                    output.Stop();
                    output.Dispose();
                    output = null;
                }

                if (reader != null)
                {
                    reader.Dispose();
                    reader = null;
                }
            }
        }
    }
}
