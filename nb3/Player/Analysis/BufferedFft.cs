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
            this.LoudnessWeighting = weighting != null ? weighting : new NullWeighting(fftSize / 4);

            ringBuffer = new RingBuffer<float>(bufferSize);
            fft = new FFT(fftSize);
        }

        /// <summary>
        /// Creates a buffered FFT sized to generate the given output.
        /// 
        /// FFT is made twice the size of the output buffer, because we discard all frequencies above the Nyquist rate.
        /// The ring buffer is made twice the size of the FFT (so 4 times the size of the output).
        /// 
        /// </summary>
        /// <param name="outputSize"></param>
        /// <param name="weighting"></param>
        public BufferedFFT(int outputSize, ILoudnessWeighting weighting = null) : this(outputSize * 4, outputSize * 2, weighting)
        { 
        }

        public void Add(float sample)
        {
            ringBuffer.Add(sample);
        }

        public void Add(IEnumerable<float> samples)
        {
            foreach (var sample in samples)
                ringBuffer.Add(sample);
        }

        public void GenerateTo(float[] dest, int offset, int count, int stride = 1)
        {
            fft.Generate(ringBuffer);
            fft.CopyTo(dest, offset, count, stride, LoudnessWeighting);
        }
    }
}
