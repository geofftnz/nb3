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

        private const int fftSize0 = 2048;
        private const int fftSize1 = 1024;
        private const int fftSize2 = 256;

        private int frameInterval = 44100 / 180;  // target 60 FPS
        private int sampleCounter = 0;
        private const int outputResolution = 1024;
        //private int[] fftRepmap = new int[outputResolution];  // remap from larger spectrum to smaller output.
        private RingBuffer<float> ringbuffer = new RingBuffer<float>(8192);

        private FFT FFT0 = new FFT(fftSize0);
        private FFT FFT1 = new FFT(fftSize1);
        private FFT FFT2 = new FFT(fftSize2);

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

        private float[] tempFFT0 = new float[fftSize0 / 2];
        private float[] tempFFT1 = new float[fftSize1 / 2];
        private float[] tempFFT2 = new float[fftSize2 / 2];

        private void AddSample(float mix)
        {
            ringbuffer.Add(mix);

            sampleCounter++;
            if (sampleCounter > frameInterval)
            {


                //FFT0.Generate(ringbuffer);
                //FFT0.CopyTo(tempFFT0, 0, fftSize0 / 2);

                //FFT1.Generate(ringbuffer, (fftSize0 / 2) - (fftSize1 / 2));
                //FFT1.CopyTo(tempFFT1, 0, fftSize1 / 2);

                //FFT2.Generate(ringbuffer, (fftSize0 / 2) - (fftSize2 / 2));
                //FFT2.CopyTo(tempFFT2, 0, fftSize2 / 2);
                //var analysisSample = new AudioAnalysisSample(CombineFFTs());

                float[] f = new float[outputResolution];
                FFT0.Generate(ringbuffer);
                FFT0.CopyTo(f, 0, outputResolution);
                var analysisSample = new AudioAnalysisSample(f);

                SpectrumReady?.Invoke(this, new FftEventArgs(analysisSample));

                sampleCounter = 0;
            }
        }

        private float[] CombineFFTs()
        {
            float[] f = new float[outputResolution];
            for (int i = 0; i < outputResolution; i++)
            {
                float x = (float)i / (float)(outputResolution - 1);
                x = x * 0.5f + 0.5f * x * x;

                f[i] = MixFFTSample2(x);
            }

            return f;
        }

        private float MixFFTSample2(float x)
        {
            return
                tempFFT0[(int)(x * ((float)fftSize0 / 2f - 1f))] * 0.5f +
                tempFFT1[(int)(x * ((float)fftSize1 / 2f - 1f))] * 0.25f +
                tempFFT2[(int)(x * ((float)fftSize2 / 2f - 1f))] * 0.25f;
        }


        private float MixFFTSample(float x)
        {
            // a should range from 0 to 1.0, where 1.0 is the Nyquist freq (half of FFT)

            // calculate blending coefficients for the 3 spectrums
            // 0 - 0.25 -> 0
            // 0.25 - 0.5 -> blend 0-1
            // 0.5 - 0.75 -> blend 1-2
            // 0.75 - 1.0 -> 2
            float a, b;

            if (x >= 0.9f)
            {
                return tempFFT2[(int)(x * ((float)fftSize2 / 2f - 1f))];
            }
            else if (x >= 0.75f)
            {
                a = smootherstep(0.75f, 0.9f, x);
                b = 1f - a;
                return tempFFT2[(int)(x * ((float)fftSize2 / 2f - 1f))] * a + tempFFT1[(int)(x * ((float)fftSize1 / 2f - 1f))] * b;
            }
            else if (x > 0.5f)
            {
                a = smootherstep(0.5f, 0.75f, x);
                b = 1f - a;
                return tempFFT1[(int)(x * ((float)fftSize1 / 2f - 1f))] * a + tempFFT0[(int)(x * ((float)fftSize0 / 2f - 1f))] * b;
            }

            return tempFFT0[(int)(x * ((float)fftSize0 / 2f - 1f))];
        }

        private float smootherstep(float edge0, float edge1, float x)
        {
            // Scale, bias and saturate x to 0..1 range
            x = (x - edge0) / (edge1 - edge0);
            if (x < 0f) x = 0f;
            else if (x > 1f) x = 1f;

            // Evaluate polynomial
            //return x * x * (3f - 2f * x);
            return x * x * x * (x * (x * 6f - 15f) + 10f);
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
