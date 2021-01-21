using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis.Filter.Nodes
{
    /// <summary>
    /// Applies convolution to the incoming stream.
    /// 
    /// Coefficient array starts with 0 = current sample, then goes back in time.
    /// </summary>
    public class Convolution : IFilterNode
    {
        private float[] _coefficients;
        private RingBuffer<float> _ringBuffer;

        public Convolution(params float[] coefficients)
        {
            _coefficients = new float[coefficients.Length];

            for (int i = 0; i < coefficients.Length; i++)
            {
                _coefficients[i] = coefficients[i];
            }
            _ringBuffer = new RingBuffer<float>(coefficients.Length);
        }


        public float Get(float input)
        {
            float ret = 0f;

            // add sample to ring buffer
            _ringBuffer.Add(input);

            int i = 0;
            foreach (float v in _ringBuffer.Last().Take(_coefficients.Length))
            {
                ret += v * _coefficients[i++];                  
            }
            return ret;
        }
    }
}
