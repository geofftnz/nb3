using nb3.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis.Filter.Nodes
{
    public class GainControl : IFilterNode
    {
        public float MaxGain { get; set; } = 2.0f;
        public float PeakMix { get; set; } = 1.0f;
        public float Decay { get; set; } = 0.999f;

        private float _max = 0f;

        public GainControl(float maxGain, float peakMix, float decay)
        {
            MaxGain = maxGain;
            PeakMix = peakMix;
            Decay = decay;
        }

        public float Get(float input)
        {
            _max = PeakMix.Mix(Math.Max(_max, input), _max);

            float gain = Math.Min(MaxGain, 1f / Math.Max(0.00001f, _max));

            _max *= Decay;

            return input * gain;
        }
    }
}
