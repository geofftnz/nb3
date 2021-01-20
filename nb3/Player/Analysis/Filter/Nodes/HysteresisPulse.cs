using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis.Filter.Nodes
{
    /// <summary>
    /// Applies hysteresis to the supplied waveform and outputs either 0.0 or 1.0
    /// 
    /// </summary>
    public class HysteresisPulse : IFilterNode
    {

        /// <summary>
        /// When the output is currently low, an input over this threshold
        /// will cause the output to go high
        /// </summary>
        public float RisingThreshold { get; set; } = 0.6f;

        /// <summary>
        /// When the output is currently high, an input under this threshold
        /// will cause the output to go low
        /// </summary>
        public float FallingThreshold { get; set; } = 0.4f;

        private bool _state = false;

        public HysteresisPulse()
        {
        }

        public HysteresisPulse(float risingThreshold, float fallingThreshold, bool initiallyHigh = false)
        {
            RisingThreshold = risingThreshold;
            FallingThreshold = fallingThreshold;
            _state = initiallyHigh;
        }

        public float Get(float input)
        {
            if (_state) // we are high
            {
                // should we go low?
                if (input < FallingThreshold)
                {
                    _state = false;
                }
            }
            else // we are low
            {
                // should we go high?
                if (input > RisingThreshold)
                {
                    _state = true;
                }
            }

            return _state ? 1f : 0f;
        }
    }
}
