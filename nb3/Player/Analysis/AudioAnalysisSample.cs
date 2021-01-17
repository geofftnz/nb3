using NAudio.Dsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis
{
    /// <summary>
    /// Represents a single sample of audio analysis to be passed to the renderer.
    /// </summary>
    public class AudioAnalysisSample
    {
        public int Samples { get; set; } = 1;
        public float[] Spectrum { get; set; } = null;
        public float[] Spectrum2 { get; set; } = null;
        public float[] AudioData { get; set; } = null;

        //public List<string> AudioDataLabels { get; set; } = null;  // TODO: this should probably not be passed every frame.

        public AudioAnalysisSample(float[] spectrum, float[] spectrum2, float[] audioData, int samples/*, List<string> audioDataLabels*/)
        {
            Spectrum = spectrum;
            Spectrum2 = spectrum2;
            AudioData = audioData;
            Samples = samples;
            //AudioDataLabels = audioDataLabels;
        }
    }
}
