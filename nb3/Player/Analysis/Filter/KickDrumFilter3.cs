using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nb3.Common;

namespace nb3.Player.Analysis.Filter
{
    public class KickDrumFilter3 : SpectrumFilterBase, ISpectrumFilter
    {
        private const int NUMOUTPUTS = 2;

        private const int OUT_EDGE = 0;
        private const int OUT_LEVEL = 1;

        public int OutputOffset { get; set; }
        public int OutputSlotCount { get { return NUMOUTPUTS; } }
        private float[] output = new float[NUMOUTPUTS];

        public int FreqStart { get; set; }
        public int FreqCount { get; set; }
        public float Threshold { get; set; } = 0.4f;
        public float Trigger { get; set; } = 0.95f;
        public float Decay { get; set; } = 0.9995f;
        public float AvgLowpass { get; set; } = 0.999f;
        public float AvgLowpassHigh { get; set; } = 0.98f;
        public float ActivationLinearDecay { get; set; } = 0.005f;

        //public float Release { get; set; } = 0.2f;

        private RingBuffer<float> buffer = new RingBuffer<float>(64);

        private float avg_long = 0f;
        private float max = 0f;
        private bool state = false;
        private float activation_rising_edge = 0f;
        private float activation_high = 0f;


        public KickDrumFilter3(string name = "KD3", int freq_start = 0, int freq_count = 8) : base(name, "edge", "level")
        {
            FreqStart = freq_start;
            FreqCount = freq_count;
        }

        public float[] GetValues(FilterParameters frame)
        {

            // step 1: accumulate over frequency range
            float current = 0f;
            for (int i = FreqStart; i < FreqStart + FreqCount; i++)
            {
                current += frame.Spectrum[i];
            }
            current /= (float)FreqCount;
            current = current.NormDB();

            buffer.Add(current);

            // step 2: maintain a long-term average
            avg_long = AvgLowpass.Mix(current, avg_long);

            // step 3: get a recent moving average
            float lowpass = buffer.Last().Take(8).Average();

            // step 4: peak tracker
            max = Math.Max(Threshold, Math.Max(max, lowpass));

            // step 5: state transition
            if (!state)
            {
                if (lowpass > Trigger.Mix(avg_long, max))
                {
                    state = !state;
                    activation_rising_edge = 1f;
                }
            }
            else
            {
                // while we're high, bring the avg up faster
                avg_long = AvgLowpassHigh.Mix(current, avg_long);

                // on-state activation
                activation_high = 1f;

                if (lowpass < avg_long)
                {
                    state = !state;
                }
            }

            output[OUT_EDGE] = activation_rising_edge;
            output[OUT_LEVEL] = activation_high;
            activation_rising_edge = Math.Max(0f, activation_rising_edge - ActivationLinearDecay);
            activation_high = Math.Max(0f, activation_high - ActivationLinearDecay);
            max *= Decay;

            return output;
        }

        public void Reset()
        {
        }
    }
}
