using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nb3.Common;

namespace nb3.Player.Analysis.Filter
{
    public class PeakFrequencyFilter : ISpectrumFilter
    {
        private const int NUMOUTPUTS = 3;

        public enum FilterOutputs
        {
            Frequency = 0,
            AbsLevel,
            RelLevel
        }

        public int OutputOffset { get; set; }
        public int OutputSlots { get { return NUMOUTPUTS; } }
        private float[] output = new float[NUMOUTPUTS];

        public int FreqStart { get; set; }
        public int FreqCount { get; set; }

        public PeakFrequencyFilter(int freq_start = 0, int freq_count = 8)
        {
            FreqStart = freq_start;
            FreqCount = freq_count;
        }

        public float[] GetValues(FilterParameters frame)
        {

            // 1: find the highest point on the normalized spectrum
            // calculate the average along the way
            float current = 0f, max = 0f;
            int index = 0;
            float total = 0f;

            for (int i = 0; i < FreqCount; i++)
            {
                current = frame.SpectrumDB[FreqStart + i];
                total += current;
                if (current > max)
                {
                    max = current;
                    index = i;
                }
            }
            total /= FreqCount;

            output[(int)FilterOutputs.Frequency] = (float)index / FreqCount;
            output[(int)FilterOutputs.AbsLevel] = max;
            output[(int)FilterOutputs.RelLevel] = (max - total) * 2f;

            return output;
        }

        public void Reset()
        {
        }
    }
}
