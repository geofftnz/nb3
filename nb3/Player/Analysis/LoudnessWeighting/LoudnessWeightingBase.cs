using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis.LoudnessWeighting
{

    /// <summary>
    /// Generates and caches a frequency-dependent loudness weighting
    /// </summary>
    public class LoudnessWeightingBase : ILoudnessWeighting
    {
        public int Size { get; private set; }

        public double this[int n]
        {
            get
            {
                if (n < 0)
                    n = 0;
                if (n >= Size)
                    n = Size - 1;

                return coeff[n];
            }
        }
        public double this[float n]
        {
            get
            {
                if (n < 0f)
                    n = 0f;
                if (n > 1f)
                    n = 1f;

                return coeff[(int)(n * (Size - 1))];
            }
        }

        protected double[] coeff;

        protected LoudnessWeightingBase(int size)
        {
            this.Size = size;
            coeff = new double[size];
        }



    }
}
