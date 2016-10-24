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
        private int fft_m; // log2 size of FFT
        private int frameInterval = 44100 / 90;  // target 60 FPS
        private int sampleCounter = 0;
        private float[] fftWindowShape = new float[fftSize];

        private RingBuffer<float> ringbuffer = new RingBuffer<float>(8192);

        public event EventHandler<FftEventArgs> SpectrumReady;

        public WaveFormat WaveFormat => source.WaveFormat;

        public SpectrumGenerator(ISampleProvider source)
        {
            this.source = source;
            this.channels = source.WaveFormat.Channels;
            this.fft_m = (int)Math.Log(fftSize, 2.0);

            for (int i = 0; i < fftSize; i++)
            {
                fftWindowShape[i] = (float)FastFourierTransform.HammingWindow(i, fftSize);
            }
        }


        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = source.Read(buffer, offset, count);

            for(int i = 0; i < samplesRead; i+=channels)
            {
                // mix channels
                // TODO: multi-channel FFT for stereo imaging.
                float mix = 0f;
                for(int c = 0; c < channels; c++)
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
                // calculate and emit FFT
                var fftWindow = ringbuffer.Last().Take(fftSize).Select(x => new Complex { X = x, Y = 0f }).ToArray();

                for (int i = 0; i < fftSize; i++)
                {
                    fftWindow[i].X *= fftWindowShape[i];
                }

                FastFourierTransform.FFT(true, fft_m, fftWindow);

                float[] f = fftWindow.Select(c=>(float)Math.Sqrt(c.X*c.X+c.Y*c.Y)).Take(fftSize/2).ToArray();

                var analysisSample = new AudioAnalysisSample(f);

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
