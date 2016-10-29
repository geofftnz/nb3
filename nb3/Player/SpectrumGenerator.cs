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

        private int frameInterval = 44100 / 120;  // target 120 FPS
        private int sampleCounter = 0;
        private const int outputResolution = 1024;
        private RingBuffer<float> ringbuffer = new RingBuffer<float>(8192);

        private FFT fft = new FFT(fftSize);

        public event EventHandler<FftEventArgs> SpectrumReady;
        public WaveFormat WaveFormat => source.WaveFormat;

        public int FrameInterval { get { return frameInterval; } }



        public SpectrumGenerator(ISampleProvider source)
        {
            this.source = source;
            this.channels = source.WaveFormat.Channels;
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
                // mix channels
                // TODO: multi-channel FFT for stereo imaging.
                float mix = 0f;
                for (int c = 0; c < channels; c++)
                {
                    mix += buffer[offset + i + c];
                }
                mix /= channels;

                // add to ring buffer
                AddSample(mix);
            }

            return samplesRead;
        }

        private void AddSample(float mix)
        {
            ringbuffer.Add(mix);

            sampleCounter++;
            if (sampleCounter > frameInterval)
            {
                float[] f = new float[outputResolution];
                fft.Generate(ringbuffer);
                fft.CopyTo(f, 0, outputResolution);
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
