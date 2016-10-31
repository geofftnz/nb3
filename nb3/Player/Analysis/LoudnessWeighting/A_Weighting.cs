using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis.LoudnessWeighting
{
    public class A_Weighting : LoudnessWeightingBase, ILoudnessWeighting
    {

        public A_Weighting(int size) : base(size)
        {
            for (int i = 0; i < size; i++)
            {
                coeff[i] = A((double)i);
            }

        }

        /// <summary>
        /// Calculates the ITU-T-468 weighting
        /// </summary>
        /// <param name="f">Frequency in Hz</param>
        /// <returns></returns>
        private static double A(double f)
        {
            if (f < 0.1)
                f = 0.1;

            double f2 = f * f;

            return ((12200.0 * 12200.0 * f2 * f2) /
                ((f2 + 20.6 * 20.6) * Math.Sqrt((f2 + 107.7 * 107.7) * (f2 + 737.9 * 737.9)) * (f2 + 12200.0 * 12200.0))) / 0.7943463958;

        }

    }
}
