using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nb3.Common;

namespace nb3.Player.Analysis.Filter
{
    public class KickDrumFilter2 : ISpectrumFilter
    {
        private const int NUMOUTPUTS = 4;
        public int OutputOffset { get; set; }
        public int OutputSlots { get { return NUMOUTPUTS; } }
        private float[] output = new float[NUMOUTPUTS];

        public float Threshold { get; set; } = 0.05f;
        public float Trigger { get; set; } = 0.8f;
        public float Decay { get; set; } = 0.995f;
        //public float Release { get; set; } = 0.2f;

        private RingBuffer<float> buffer = new RingBuffer<float>(64);
        private int freqStart, freqCount;
        private float lowpassCoeff;

        private float max = 0f;
        private float activation = 0f;


        public KickDrumFilter2(int freq_start = 0, int freq_count = 8, float lowpass_coeff = 0.98f)
        {
            this.freqStart = freq_start;
            this.freqCount = freq_count;
            this.lowpassCoeff = lowpass_coeff;

        }

        public float[] GetValues(FilterParameters frame)
        {
            float current = 0f;
            for (int i = freqStart; i < freqStart + freqCount; i++)
            {
                current += frame.Spectrum[i];
            }
            current /= (float)freqCount;
            current = current.NormDB();
            output[0] = current;

            buffer.Add(current);

            float previous = buffer.Last().Skip(1).Take(8).Average();

            float diff = current - previous;

            output[1] = diff + 0.5f;

            if (diff > max * Trigger)
            {
                max = diff;
                activation = 1f;
            }

            output[2] = max;
            output[3] = activation;

            max = Math.Max(Threshold, max * Decay);

            activation *= 0.8f;

            return output;
        }

        public void Reset()
        {
        }
    }
}
