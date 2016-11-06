using nb3.Player.Analysis.LoudnessWeighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis
{
    /// <summary>
    /// Encapsulates a ring buffer and provides a FFT as output
    /// </summary>
    public class BufferedFFT
    {
        private int bufferSize;
        private int fftSize;

        private RingBuffer<float> ringBuffer;
        private FFT fft;

        public ILoudnessWeighting LoudnessWeighting { get; set; }

        public BufferedFFT(int bufferSize, int fftSize, ILoudnessWeighting weighting = null)
        {

            this.bufferSize = bufferSize;
            this.fftSize = fftSize;
            this.LoudnessWeighting = weighting != null ? weighting : new NullWeighting(fftSize/2);

            ringBuffer = new RingBuffer<float>(bufferSize);
            fft = new FFT(fftSize);
        }

        public void Add(float sample)
        {
            ringBuffer.Add(sample);
        }

        public void GenerateTo(float[] dest, int offset, int count, int stride = 1)
        {
            fft.Generate(ringBuffer);
            fft.CopyTo(dest, offset, count, stride);
        }
    }
}
