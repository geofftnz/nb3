using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nb3.Common;

namespace nb3.Player.Analysis.Filter
{
    public class KickDrumFilter3 : ISpectrumFilter
    {
        private const int NUMOUTPUTS = 6;
        public int OutputOffset { get; set; }
        public int OutputSlots { get { return NUMOUTPUTS; } }
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

        private float max = 0f;
        private bool state = false;
        private float activation = 0f;


        public KickDrumFilter3(int freq_start = 0, int freq_count = 8)
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
            output[0] = current;

            buffer.Add(current);

            // step 2: maintain a long-term average
            output[1] = AvgLowpass.Mix(current, output[1]);

            // step 3: get a recent moving average
            float lowpass = buffer.Last().Take(8).Average();
            output[2] = lowpass;

            // step 4: peak tracker
            max = Math.Max(Threshold, Math.Max(max, lowpass));
            output[3] = max;

            // step 5: state transition
            if (!state)
            {
                if (lowpass > Trigger.Mix(output[1], max))
                {
                    state = !state;
                    activation = 1f;
                }
            }
            else
            {
                // while we're high, bring the avg up faster
                output[1] = AvgLowpassHigh.Mix(current, output[1]);

                if (lowpass < output[1])
                {
                    state = !state;
                }
            }

            output[4] = state ? 1f : 0f;
            output[5] = activation;
            activation = Math.Max(0f, activation - ActivationLinearDecay);
            max *= Decay;

            return output;
        }

        public void Reset()
        {
        }
    }
}
