using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis.Filter.Nodes
{
    /// <summary>
    /// Applies convolution to the incoming stream.
    /// </summary>
    public class Convolution : IFilterNode
    {
        private float[] _coefficients;
        private RingBuffer<float> _ringBuffer;

        public Convolution(params float[] coefficients)
        {
            _coefficients = new float[coefficients.Length];
            for(int i = 0; i < coefficients.Length;i++)
            {
                _coefficients[i] = coefficients[i];
            }
            _ringBuffer = new RingBuffer<float>(coefficients.Length);
        }


        public float Get(float input)
        {
            throw new NotImplementedException();
        }
    }
}
