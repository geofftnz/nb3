using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player
{
    /// <summary>
    /// Represents a single sample of audio analysis to be passed to the renderer.
    /// </summary>
    public class AudioAnalysisSample
    {
        public int Samples { get; set; } = 1;
        public float[] Spectrum { get; set; } = null;

        public AudioAnalysisSample(float[] spectrum, int samples)
        {
            Spectrum = spectrum;
            Samples = samples;
        }
    }
}
