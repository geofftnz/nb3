using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

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
        // Wave output is owned by caller, do not dispose.
        private IWavePlayer output = null;

        //private WaveStream pcmstream = null;
        //private BlockAlignReductionStream reductionstream = null;
        private AudioFileReader reader = null;
        private SpectrumGenerator spectrum = null;

        public event EventHandler<FftEventArgs> SpectrumReady;

        private object lockObj = new object();




        public Player(IWavePlayer output)
        {
            this.output = output;

        }

        public void Open(string filename)
        {
            Stop();
            Dispose(true);

            //pcmstream = WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(filename));
            //reductionstream = new BlockAlignReductionStream(pcmstream);
            //output.Init(new WaveChannel32(reductionstream));

            reader = new AudioFileReader(filename);
            spectrum = new SpectrumGenerator(reader);
            spectrum.SpectrumReady += Spectrum_SpectrumReady;
            output.Init(spectrum);
        }

        private void Spectrum_SpectrumReady(object sender, FftEventArgs e)
        {
            SpectrumReady?.Invoke(sender, e);
        }

        public void Play()
        {
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
            switch (output.PlaybackState)
            {
                case PlaybackState.Playing:
                    output.Pause();
                    break;
                default:
                    break;
            }
        }

        public void Stop()
        {
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
                if (reader != null)
                {
                    reader.Dispose();
                    reader = null;
                }
            }
        }
    }
}
