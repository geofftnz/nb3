﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player
{

    /// <summary>
    /// Generates and caches a frequency-dependent loudness weighting
    /// </summary>
    public class LoudnessWeighting
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

        private double[] coeff;

        public LoudnessWeighting(int size)
        {
            this.Size = size;
            coeff = new double[size];

            for (int i = 0; i < size; i++)
            {
                coeff[i] = ITU_T_468((double)i);
            }
        }


        /// <summary>
        /// Calculates the ITU-T-468 weighting
        /// </summary>
        /// <param name="f">Frequency in Hz</param>
        /// <returns></returns>
        private static double ITU_T_468(double f)
        {
            if (f < 0.1)
                f = 0.1;

            double f6 = Math.Pow(f, 6.0) * -4.73733898137838E-24;
            double f5 = Math.Pow(f, 5.0) * 1.30661225741282E-19;
            double f4 = Math.Pow(f, 4.0) * 2.04382833360612E-15;
            double f3 = Math.Pow(f, 3.0) * 2.11815088751865E-11;
            double f2 = f * f * 0.000000136389479546262;
            double h1 = f6 + f4 - f2 + 1.0;
            double h2 = f5 - f3 + f * 0.000555948802349864;
            return (f * 0.000124633263753214) / Math.Sqrt(h1 * h1 + h2 * h2);
        }

    }
}