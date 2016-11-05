﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nb3.Common;

namespace nb3.Player.Analysis.Filter
{
    public class KickDrumFilter : ISpectrumFilter
    {
        private const int NUMOUTPUTS = 4;
        public int OutputOffset { get; set; }
        public int OutputSlots { get { return NUMOUTPUTS; } }
        private float[] outputs = new float[NUMOUTPUTS];

        public float Threshold { get; set; } = 0.2f;
        public float Decay { get; set; } = 0.02f;
        public float Release { get; set; } = 0.2f;
        

        private int freqStart, freqCount;
        private float lowpassCoeff;
        private float avg = 0f;
        private float activation = 1f;
        

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
            outputs[0] = current;

            avg = avg * lowpassCoeff + current * (1f - lowpassCoeff);
            outputs[1] = avg;

            avg = Math.Max(0.3f, avg);

            outputs[2] = Math.Max(0f, (current - avg) * 4.0f);

            activation += Decay;
            activation = Math.Min(1f, activation);

            if (outputs[2] > Threshold && activation > Release)
            {
                activation = 0f;
            }

            outputs[3] = 1f - activation;

            return outputs;
        }

        public void Reset()
        {
        }
    }
}
