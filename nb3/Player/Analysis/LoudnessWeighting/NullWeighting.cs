using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis.LoudnessWeighting
{
    public class NullWeighting : LoudnessWeightingBase, ILoudnessWeighting
    {

        public NullWeighting(int size) : base(size)
        {
            for (int i = 0; i < size; i++)
            {
                coeff[i] = 1f;
            }

        }

    }
}
