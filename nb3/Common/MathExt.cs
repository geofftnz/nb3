using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Common
{
    public static class MathExt
    {
        /// <summary>
        /// Converts a linear value to dB, then scales it so that 0dB == 1.0 and -100dB == 0.0
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static double NormDB(this double a)
        {
            return Math.Max(0.0, 1.0 + (20.0 * Math.Log10(a)) / 100.0);
        }
        public static float NormDB(this float a)
        {
            return (float)((double)a).NormDB();
        }

        public static float Mix(this float x, float a, float b)
        {
            return a * (1f - x) + x * b;
        }

        public static float[] Flat(int n, float sum_to = 1f)
        {
            return Enumerable.Range(0, n).Select(i => sum_to / n).ToArray();
        }
        public static float[] LinearDecay(int n, float sum_to = 1f)
        {
            var x = Enumerable.Range(0, n).Select(i => (float)(n - i)).ToArray();
            var total = x.Sum();
            return x.Select(i => (i * sum_to) / total).ToArray();
        }
    }
}
