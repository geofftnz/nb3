using NAudio.Dsp;
using nb3.Player.Analysis.LoudnessWeighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis
{
    /// <summary>
    /// Wraps up a given-sized FFT in a way designed for multiple calls.
    /// </summary>
    public class FFT
    {
        private int size;
        private int fft_m;
        private float[] fftWindowShape;
        private float[] fftTempSamples;
        private Complex[] fftTempComplex;
        private float[] fftSpectrum;

        public FFT(int size)
        {
            this.size = size;
            fftWindowShape = new float[size];
            fftTempSamples = new float[size];
            fftTempComplex = new Complex[size];
            fftSpectrum = new float[size / 2];
            this.fft_m = (int)Math.Log(size, 2.0);
            GenerateFFTWindow();
        }

        private void GenerateFFTWindow()
        {
            for (int i = 0; i < size; i++)
            {
                fftWindowShape[i] = (float)FastFourierTransform.HammingWindow(i, size);
            }
        }

        public void Generate(RingBuffer<float> ringbuffer, int offsetIntoPast = 0, bool mirror = false)
        {
            if (mirror)
            {
                ringbuffer.CopyLastTo(fftTempSamples, 0, size / 2, offsetIntoPast);
                for (int i = 0; i < size / 2; i++)
                    fftTempSamples[size - i - 1] = fftTempSamples[i];
            }
            else
            {
                ringbuffer.CopyLastTo(fftTempSamples, 0, size, offsetIntoPast);
            }


            for (int i = 0; i < size; i++)
            {
                fftTempComplex[i].X = fftTempSamples[i] * fftWindowShape[i];
                fftTempComplex[i].Y = 0f;
            }

            FastFourierTransform.FFT(true, fft_m, fftTempComplex);

            // generate spectrum

        }

        public void CopyTo(float[] dest, int offset, int count, int stride = 1)
        {
            int max = count;
            if (max > size / 2)
                max = size / 2;

            int desti = offset;

            for (int i = 0; i < max; i++)
            {
                dest[desti] = (float)Math.Sqrt(fftTempComplex[i].X * fftTempComplex[i].X + fftTempComplex[i].Y * fftTempComplex[i].Y);
                desti += stride;
            }
        }

        public void CopyTo(float[] dest, int offset, int count, int stride, ILoudnessWeighting weighting)
        {
            int max = count;
            if (max > size / 2)
                max = size / 2;

            if (weighting.Size < max)
                throw new ArgumentOutOfRangeException("Weighting not large enough");

            int desti = offset;

            for (int i = 0; i < max; i++)
            {
                float freq = (float)i / (float)((size / 2) - 1);

                dest[desti] = (float)(Math.Sqrt(fftTempComplex[i].X * fftTempComplex[i].X + fftTempComplex[i].Y * fftTempComplex[i].Y) * weighting[freq]);
                desti += stride;
            }
        }


    }
}
