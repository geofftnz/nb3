using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis
{
    public class FilterParameters
    {
        /// <summary>
        /// Frame of analysis data from the spectrum analyser.
        /// Contains the linear stereo spectrum as interleaved floats.
        /// </summary>
        public AudioAnalysisSample Frame { get; set; }

        /// <summary>
        /// A mono-mixed linear-frequency spectrum
        /// </summary>
        public float[] Spectrum { get; set; }

        /// <summary>
        /// A mono-mixed linear spectrum, in dB
        /// </summary>
        public float[] SpectrumDB { get; set; }

        /// <summary>
        /// Auxiliary linear spectrum, linear values
        /// </summary>
        //public float[] Spectrum2 { get; set; }

        /// <summary>
        /// Auxiliary linear spectrum, in dB
        /// </summary>
        public float[] Spectrum2DB { get; set; }

        /// <summary>
        /// History of mono mixed linear spectrum
        /// </summary>
        public RingBuffer<float[]> SpectrumHistory { get; set; }
    }
}
