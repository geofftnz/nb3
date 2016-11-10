using NAudio.Utils;
using nb3.Common;
using nb3.Player.Analysis.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nb3.Common;

namespace nb3.Player.Analysis
{
    /// <summary>
    /// SpectrumAnalyser
    /// 
    /// Takes an AudioAnalysisSample containing a spectrum and populates the AudioData array
    /// </summary>
    public class SpectrumAnalyser
    {
        private const int SPECTRUMHISTORY = 16;
        private RingBuffer<float[]> SpectrumHistory = new RingBuffer<float[]>(SPECTRUMHISTORY);
        private FilterParameters filterParams = new FilterParameters();
        public List<ISpectrumFilter> Filters { get; private set; } = new List<ISpectrumFilter>();
        private int maxFilterOutputIndex = 0;

        public SpectrumAnalyser()
        {
            filterParams.Spectrum = new float[Globals.SPECTRUMRES];
            filterParams.SpectrumDB = new float[Globals.SPECTRUMRES];
            filterParams.SpectrumHistory = SpectrumHistory;

            // The order of these is important
            AddFilter(new KickDrumFilter3(0, 6));
            AddFilter(new KickDrumFilter2(0, 4));
            AddFilter(new KickDrumFilter2(0, 8));
            AddFilter(new DistributionFilter());
            AddFilter(new KickDrumFilter());
        }

        public void Process(AudioAnalysisSample frame)
        {
            SpectrumHistory.Add(frame.Spectrum);
            filterParams.Frame = frame;

            // generate downmixed spectrums
            for (int i = 0; i < Globals.SPECTRUMRES; i++)
            {
                filterParams.Spectrum[i] = (frame.Spectrum[i * 2] + frame.Spectrum[i * 2 + 1]) * 0.5f;
                //filterParams.SpectrumDB[i] = (float)Decibels.LinearToDecibels(filterParams.Spectrum[i]);

                filterParams.SpectrumDB[i] = filterParams.Spectrum[i].NormDB();
                //s.rgb = 20.0 * log(s.rgb);
                //s.rgb = max(vec3(0.0), vec3(1.0) + ((s.rgb + vec3(20.0)) / vec3(200.0)));

            }

            // run filters
            foreach (var filter in Filters)
            {
                var results = filter.GetValues(filterParams);
                for (int i = 0; i < filter.OutputSlots; i++)
                {
                    frame.AudioData[filter.OutputOffset + i] = results[i];
                }
            }
        }

        private void AddFilter(ISpectrumFilter filter)
        {
            if (maxFilterOutputIndex + filter.OutputSlots >= Globals.AUDIODATASIZE)
                throw new InvalidOperationException("SpectrumAnalyser out of slots for filter.");

            filter.OutputOffset = maxFilterOutputIndex;
            maxFilterOutputIndex += filter.OutputSlots;
            Filters.Add(filter);
        }
    }
}
