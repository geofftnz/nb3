using nb3.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis.Filter.Nodes
{
    public class PeakExtract : IFilterNode
    {
        public float Lowpass { get; set; }
        public float Amount { get; set; }

        private float _avg = 0f;
        

        public PeakExtract(float lowpass = 0.99f, float amount = 1f)
        {
            Lowpass = lowpass;
            Amount = amount;
        }

        public float Get(float input)
        {
            _avg = Lowpass.Mix(input, _avg);
            return Math.Max(0f, input - _avg * Amount) * (1f+Amount);
        }
    }
}
