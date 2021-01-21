using nb3.Common;
using nb3.Player.Analysis.Filter.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis.Filter
{
    /// <summary>
    /// Filter for broadband transients, for example:
    /// 
    /// - HiHats
    /// - Snares
    /// - Claps
    /// - Cymbals
    /// 
    /// This uses the secondary spectrum which has a better time resolution at the expense of frequency resolution.
    /// 
    /// </summary>
    public class BroadbandTransientFilter : SpectrumFilterBase, ISpectrumFilter
    {
        private const int NUMOUTPUTS = 6;

        public enum Outputs
        {
            Current,
            MovingAverage,
            HighPass,
            PeakTracker,
            Edge,
            Level
        }

        public int OutputOffset { get; set; }
        public int OutputSlotCount { get { return NUMOUTPUTS; } }

        private float[] output = new float[NUMOUTPUTS];

        public int FreqStart { get; private set; }
        public int FreqCount { get; private set; }
        public float Threshold { get; set; } = 0.05f;  // lowest value able to trigger a rising edge
        public float TriggerHigh { get => hyst.RisingThreshold; set => hyst.RisingThreshold = value; }
        public float TriggerLow { get => hyst.FallingThreshold; set => hyst.FallingThreshold = value; }

        public float Decay { get; set; } = 0.999f;  // decay for the peak tracker

        public float AvgLowpass { get; set; } = 0.99f;  // lowpass value used for long-term average
        //public float AvgLowpassHigh { get; set; } = 0.98f;
        public float ActivationLinearDecay { get; set; } = 0.02f;
        public float HighPass { get; set; } = 1.0f;

        private const int RINGLEN = 128;
        private RingBuffer<float> buffer = new RingBuffer<float>(RINGLEN);
        private float avg_long = 0f;
        private float max = 0f;
        private bool state = false;
        private float activation_rising_edge = 0f;
        private float activation_high = 0f;
        private float last_pulse = 0f;

        private Convolution conv;
        private HysteresisPulse hyst;
        private PeakExtract peak;
        private GainControl agc;

        Func<FilterParameters, int, float> Spectrum;


        public BroadbandTransientFilter(string name, Func<FilterParameters, int, float> spectrum, int freq_start, int freq_count, IList<float> convolutionCoefficients) : base(name, "current", "movavg", "highpass", "peak", "edge", "level")
        {
            Spectrum = spectrum;
            FreqStart = freq_start;
            FreqCount = freq_count;

            /*
            if (FreqStart > Globals.SPECTRUM2RES - 2)
            {
                FreqStart = Globals.SPECTRUM2RES - 2;
            }
            if (FreqStart + FreqCount > Globals.SPECTRUM2RES)
            {
                FreqCount = Globals.SPECTRUM2RES - FreqStart;
            }*/

            conv = new Convolution(convolutionCoefficients.ToArray());
            hyst = new HysteresisPulse(0.5f, 0.4f);
            peak = new PeakExtract(AvgLowpass,0.5f);
            agc = new GainControl(1.5f, 0.0f, 0.999f);
        }

        public float[] GetValues(FilterParameters frame)
        {
            float current = 0f;
            for (int i = FreqStart; i < FreqStart + FreqCount; i++)
            {
                current += Spectrum(frame, i);
            }
            current /= (float)FreqCount;

            output[(int)Outputs.Current] = current;

            // convolution filter 
            current = conv.Get(current);

            output[(int)Outputs.MovingAverage] = current;

            buffer.Add(current);

            current = peak.Get(current);
            output[(int)Outputs.HighPass] = current;

            current = agc.Get(current);
            output[(int)Outputs.PeakTracker] = current;

            /*
            // generate long-term average
            avg_long = AvgLowpass.Mix(current, avg_long);


            //output[(int)Outputs.MovingAverage] = avg_long;
            // get a recent moving average of previous samples
            float movingAverage = buffer.Last().Skip(1).Take(64).Average();
            //output[(int)Outputs.MovingAverage] = movingAverage;


            // subtract a portion of the moving average from our current (basic highpass)
            float highpass = Math.Max(0f, current - movingAverage * HighPass);
            //float highpass = Math.Max(0f, current - avg_long * HighPass);
            //output[(int)Outputs.HighPass] = highpass;

            // peak tracker
            max = Math.Max(Threshold, Math.Max(max, highpass));
            output[(int)Outputs.PeakTracker] = max;

            // highpass as proportion of max
            float highpass_rel = highpass / max;
            output[(int)Outputs.HighPass] = highpass_rel;

            if (!state) // we are off
            {
                // should we turn on?
                if (highpass_rel > TriggerHigh)
                {
                    state = true;
                    activation_rising_edge = 1f;
                }

            }
            else // we are on
            {
                activation_high = 1f;

                // should we turn off?
                if (highpass_rel < TriggerLow)
                {
                    state = false;
                }
            }*/


            float pulse = hyst.Get(current);

            //output[(int)Outputs.HighPass] = pulse * 0.9f;

            if (last_pulse < 0.5f && pulse > 0.5f)
            {
                activation_rising_edge = pulse;
            }
            if (pulse > 0.5f)
            {
                activation_high = pulse;
            }

            

            output[(int)Outputs.Edge] = activation_rising_edge;
            output[(int)Outputs.Level] = activation_high;
            activation_rising_edge = Math.Max(0f, activation_rising_edge - ActivationLinearDecay);
            activation_high = Math.Max(0f, activation_high - ActivationLinearDecay);
            max *= Decay;

            last_pulse = pulse;
            return output;
        }

        public void Reset()
        {
        }
    }
}
