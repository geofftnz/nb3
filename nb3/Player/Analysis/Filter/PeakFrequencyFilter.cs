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
        public float FreqSensitivity { get; set; } = 0.001f;

        private float[] LowPassSpectrum = new float[Globals.SPECTRUMRES];
        private float[] SmoothSpectrum = new float[Globals.SPECTRUMRES];

        private float[] FilterKernel = new float[] { 0.0093f, 0.028002f, 0.065984f, 0.121703f, 0.175713f, 0.198596f, 0.175713f, 0.121703f, 0.065984f, 0.028002f, 0.0093f };
        private float prev_index;
        private float prev_max;

        public PeakFrequencyFilter(int freq_start = 0, int freq_count = 8)
        {
            FreqStart = freq_start;
            FreqCount = freq_count;
            prev_index = freq_count / 2;
        }

        private void GenerateLowPassSpectrum(float[] s, float k)
        {
            for (int i = 0; i < LowPassSpectrum.Length; i++)
            {
                LowPassSpectrum[i] = LowPassSpectrum[i] * k + (1f - k) * s[i];
            }

        }

        private void GenerateSmoothSpectrum(float[] s, float[] k)
        {
            int filterSize = k.Length;
            int filterRadius = filterSize / 2;

            for (int i = 0; i < SmoothSpectrum.Length; i++)
            {
                float total = 0f;

                for (int j = 0; j < filterSize; j++)
                {
                    int si = i + j - filterRadius;
                    if (si < 0) si = 0;
                    if (si >= s.Length) si = s.Length - 1;

                    total += s[si] * k[j];
                }
                SmoothSpectrum[i] = total;
            }
        }

        public float[] GetValues(FilterParameters frame)
        {
            GenerateLowPassSpectrum(frame.SpectrumDB, 0.6f);

            // Generate a smooth spectrum
            GenerateSmoothSpectrum(LowPassSpectrum, FilterKernel);

            // 1: find the highest point on the normalized spectrum
            // calculate the average along the way
            float current = 0f, max = 0f;
            int index = 0;
            float total = 0f;
            float distance;

            for (int i = 0; i < FreqCount; i++)
            {
                distance = (float)Math.Abs(i - prev_index) * (FreqSensitivity + prev_max * 0.01f);
                //current = Math.Max(0f, LowPassSpectrum[FreqStart + i] - SmoothSpectrum[FreqStart + i]) / (1f + distance);
                current = Math.Max(0f, LowPassSpectrum[FreqStart + i] - SmoothSpectrum[FreqStart + i] * 0.5f) / (1f + distance);
                total += current;
                if (current > max)
                {
                    max = current;
                    index = i;
                }
            }
            total /= FreqCount;

            output[(int)FilterOutputs.Frequency] = (float)index / FreqCount;
            output[(int)FilterOutputs.AbsLevel] = max * 4f;
            output[(int)FilterOutputs.RelLevel] = (max - total) * 2f;

            prev_max = max;
            prev_index = index;
            if (max < 0.02f)
                prev_index = FreqCount / 2;

            return output;
        }

        public void Reset()
        {
            prev_index = FreqCount / 2;
            prev_max = 0f;
        }
    }
}
