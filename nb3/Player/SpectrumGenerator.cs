using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player
{
    /// <summary>
    /// Generates FFT spectrums from audio.
    /// Based heavily on the SampleAggregator from the NAudio samples.
    /// 
    /// Main difference is that it generates FFTs more frequently, using a ring buffer.
    /// 
    /// Target is some sort of hybrid spectrum calculated from overlaid FFTs of different sizes, in order to get good time
    /// resolution for high frequencies and good frequency resolution for low frequencies.
    /// </summary>
    public class SpectrumGenerator : ISampleProvider
    {
        /// <summary>
        /// 
        /// </summary>
        private ISampleProvider source;
        private int channels;

        private const int fftSize = 2048;
        private const int targetFrameRate = 120;

        private int frameInterval = 100;
        private int sampleCounter = 0;
        private const int outputResolution = 1024;

        private const int MAXCHANNELS = 2;
        private const int BUFFERLEN = 8192;
        private RingBuffer<float>[] ringbuffer = new RingBuffer<float>[MAXCHANNELS];
        //private RingBuffer<float> ringbuffer = new RingBuffer<float>(8192);

        private FFT fft = new FFT(fftSize);

        public event EventHandler<FftEventArgs> SpectrumReady;
        public WaveFormat WaveFormat => source.WaveFormat;

        public int FrameInterval { get { return frameInterval; } }

        public SpectrumGenerator(ISampleProvider source)
        {
            this.source = source;
            this.channels = source.WaveFormat.Channels;
            this.frameInterval = source.WaveFormat.SampleRate / targetFrameRate;

            for (int i = 0; i < MAXCHANNELS; i++)
                ringbuffer[i] = new RingBuffer<float>(BUFFERLEN);
        }


        public int Read(float[] buffer, int offset, int count)
        {
            if (source == null)
                return 0;


            int samplesRead = 0;

            try
            {
                samplesRead = source.Read(buffer, offset, count);
            }
            catch (NullReferenceException)  // TODO: FIX FILTHY HACK
            {
                return 0;
            }

            for (int i = 0; i < samplesRead; i += channels)
            {
                // add to ring buffer
                AddSample(buffer, i, channels);
            }

            return samplesRead;
        }

        private void AddSample(float[] samples, int offset, int channels)
        {
            // mono source - copy to both channels
            if (channels == 1)
            {
                ringbuffer[0].Add(samples[offset]);
                ringbuffer[1].Add(samples[offset]);
            }
            else
            {
                ringbuffer[0].Add(samples[offset]);
                ringbuffer[1].Add(samples[offset + 1]);
            }

            sampleCounter++;
            if (sampleCounter > frameInterval)
            {
                float[] f = new float[outputResolution * MAXCHANNELS];

                for (int i = 0; i < MAXCHANNELS; i++)
                {
                    fft.Generate(ringbuffer[i]);
                    fft.CopyTo(f, i, outputResolution, MAXCHANNELS);
                }

                var analysisSample = new AudioAnalysisSample(f, frameInterval);

                SpectrumReady?.Invoke(this, new FftEventArgs(analysisSample));

                sampleCounter = 0;
            }
        }

    }


    public class FftEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public FftEventArgs(AudioAnalysisSample sample)
        {
            Sample = sample;
        }

        public AudioAnalysisSample Sample { get; set; }
    }

}
