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
        private const int fftSize = 4096;
        private const int fftSizeHigh = 512;
        private int frameInterval = 44100 / 90;  // target 60 FPS
        private int sampleCounter = 0;
        private const int outputResolution = 1024;
        //private int[] fftRepmap = new int[outputResolution];  // remap from larger spectrum to smaller output.
        private RingBuffer<float> ringbuffer = new RingBuffer<float>(8192);
        private FFT FFTlow = new FFT(fftSize);
        private FFT FFThigh = new FFT(fftSizeHigh);

        public event EventHandler<FftEventArgs> SpectrumReady;
        public WaveFormat WaveFormat => source.WaveFormat;


        public SpectrumGenerator(ISampleProvider source)
        {
            this.source = source;
            this.channels = source.WaveFormat.Channels;

            //GenerateFFTRemap();
        }

        //private void GenerateFFTRemap()
        //{
        //    int fftMax = fftSize / 2;
        //    for (int i = 0; i < outputResolution; i++)
        //    {
        //        float fi = (float)i / (float)outputResolution;

        //        fftRepmap[i] = i + (int)(fi * fi * (float)(fftMax - outputResolution));
        //        if (fftRepmap[i] >= fftMax)
        //            fftRepmap[i] = fftMax - 1;

        //    }
        //}

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
                FFTlow.Generate(ringbuffer);
                FFTlow.CopyTo(f, 0, outputResolution);

                //FFThigh.Generate(ringbuffer);
                //FFThigh.CopyTo(f, outputResolution - 256, 256);

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
