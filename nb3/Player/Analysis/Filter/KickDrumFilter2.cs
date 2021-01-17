using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nb3.Common;

namespace nb3.Player.Analysis.Filter
{
    public class KickDrumFilter2 : SpectrumFilterBase, ISpectrumFilter
    {
        private const int NUMOUTPUTS = 4;
        public int OutputOffset { get; set; }
        public int OutputSlotCount { get { return NUMOUTPUTS; } }
        private float[] output = new float[NUMOUTPUTS];

        public int FreqStart { get; set; }
        public int FreqCount { get; set; }
        public float Threshold { get; set; } = 0.2f;
        public float Trigger { get; set; } = 0.95f;
        public float Decay { get; set; } = 0.999f;
        //public float Release { get; set; } = 0.2f;

        private RingBuffer<float> buffer = new RingBuffer<float>(64);

        private float max = 0f;
        private float activation = 0f;


        public KickDrumFilter2(string name = "KD2", int freq_start = 0, int freq_count = 8) : base(name, "cur", "lp", "max", "out")
        {
            FreqStart = freq_start;
            FreqCount = freq_count;
        }

        public float[] GetValues(FilterParameters frame)
        {
            float current = 0f;
            for (int i = FreqStart; i < FreqStart + FreqCount; i++)
            {
                current += frame.Spectrum[i];
            }
            current /= (float)FreqCount;
            current = current.NormDB();
            output[0] = current;

            buffer.Add(current);

            //float previous = buffer.Last().Skip(4).Take(2).Average();
            //float diff = current - previous;
            //output[1] = diff + 0.5f;

            float lowpass = buffer.Last().Take(8).Average();
            output[1] = lowpass;
            float diff = lowpass;

            if (diff > max * Trigger)
            {
                max = Math.Max(diff, max);
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
