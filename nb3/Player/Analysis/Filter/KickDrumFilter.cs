using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nb3.Common;

namespace nb3.Player.Analysis.Filter
{
    public class KickDrumFilter : ISpectrumFilter
    {
        private const int NUMOUTPUTS = 1;
        public int OutputOffset { get; set; }
        public int OutputSlots { get { return NUMOUTPUTS; } }
        private float[] outputs = new float[NUMOUTPUTS];


        private int freqStart, freqCount;
        private float lowpassCoeff;
        private float avg = 0f;


        public KickDrumFilter(int freq_start = 0, int freq_count = 8, float lowpass_coeff = 0.98f)
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

            avg = avg * lowpassCoeff + current * (1f - lowpassCoeff);

            outputs[0] = Math.Max(0f, (current - avg) * 4.0f);


            return outputs;
        }

        public void Reset()
        {
        }
    }
}
