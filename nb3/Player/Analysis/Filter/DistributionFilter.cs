using nb3.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis.Filter
{
    public class DistributionFilter : ISpectrumFilter
    {
        private const int NUMOUTPUTS = 4;
        public int OutputOffset { get; set; }
        public int OutputSlots { get { return NUMOUTPUTS; } }
        private float[] output = new float[NUMOUTPUTS];

        private float scale = 4f / (Globals.SPECTRUMRES * Globals.SPECTRUMRES);

        public float Lowpass { get; set; } = 0.95f;

        public DistributionFilter()
        {

        }


        public float[] GetValues(FilterParameters frame)
        {
            // get weighted average of spectrum
            float total = 0f;

            for (int i = 0; i < Globals.SPECTRUMRES; i++)
            {
                total += (float)Math.Sqrt(frame.SpectrumDB[i]) * i;
            }

            total *= scale;

            output[0] = total;
            output[1] = output[1] * Lowpass + (1f - Lowpass) * output[0];
            output[2] = output[2] * Lowpass + (1f - Lowpass) * output[1];
            output[3] = output[3] * Lowpass + (1f - Lowpass) * output[2];

            return output;
        }

        public void Reset()
        {

        }




    }
}
