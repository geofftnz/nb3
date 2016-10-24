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
        private int frameInterval = 44100 / 90;  // target 60 FPS
        private int sampleCounter = 0;
        private const int outputResolution = 1024;
        //private int[] fftRepmap = new int[outputResolution];  // remap from larger spectrum to smaller output.
        private RingBuffer<float> ringbuffer = new RingBuffer<float>(8192);
        private FFT FFTlow = new FFT(fftSize);

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
                // get the last size/2 samples into the first half of our temp buffer
                //ringbuffer.CopyLastTo(fftTempSamples, 0, fftSize);

                // mirror the first half of the buffer into the second half
                //for(int i = 0; i < fftSize / 2; i++)
                //{
                //fftTempSamples[fftSize - i - 1] = fftTempSamples[i];
                //}

                // copy to complex buffer, apply hamming window
                //for (int i = 0; i < fftSize; i++)
                //{
                //    fftTempComplex[i].X = fftTempSamples[i] * fftWindowShape[i];
                //    fftTempComplex[i].Y = 0f;
                //}

                // transform
                //FastFourierTransform.FFT(true, fft_m, fftTempComplex);

                // take magnitude of half of the FFT for output

                //for (int i = 0; i < outputResolution; i++)
                //{
                //    int index = fftRepmap[i];
                //
                //    f[i] = (float)Math.Sqrt(fftTempComplex[index].X * fftTempComplex[index].X + fftTempComplex[index].Y * fftTempComplex[index].Y);
                //}
                //fftTempComplex.Take(fftSize / 2).Select(c => (float)Math.Sqrt(c.X * c.X + c.Y * c.Y)).ToArray();

                /*
                var fftWindow = ringbuffer.Last().Take(fftSize).Select(x => new Complex { X = x, Y = 0f }).ToArray();

                for (int i = 0; i < fftSize; i++)
                {
                    fftWindow[i].X *= fftWindowShape[i];
                }

                FastFourierTransform.FFT(true, fft_m, fftWindow);

                float[] f = fftWindow.Select(c=>(float)Math.Sqrt(c.X*c.X+c.Y*c.Y)).Take(fftSize/2).ToArray();
                */
                float[] f = new float[outputResolution];
                FFTlow.Generate(ringbuffer);
                FFTlow.CopyTo(f, 0, outputResolution);

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
