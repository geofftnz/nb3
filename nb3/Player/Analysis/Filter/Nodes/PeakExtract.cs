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
        private float _lowpass;
        private float _amount;

        private float _avg = 0f;
        

        public PeakExtract(float lowpass = 0.99f, float amount = 1f)
        {
            _lowpass = lowpass;
            _amount = amount;
        }

        public float Get(float input)
        {
            _avg = _lowpass.Mix(input, _avg);
            return Math.Max(0f, input - _avg * _amount) * (1f+_amount);
        }
    }
}
