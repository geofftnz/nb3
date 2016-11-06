using NAudio.Dsp;
using NAudio.Wave;
using nb3.Common;
using nb3.Player.Analysis;
using nb3.Player.Analysis.LoudnessWeighting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis
{
    /// <summary>
    /// Generates FFT spectrums from audio.
    /// Based heavily on the SampleAggregator from the NAudio samples.
    /// 
    /// Main difference is that it generates FFTs more frequently, using a ring buffer.
    /// 
    /// Target is some sort of hybrid spectrum calculated from overlaid FFTs of different sizes, in order to get good time
    /// resolution for high frequencies and good frequency resolution for low frequencies.
    /// </summary>
    public class SpectrumGenerator : ISampleProvider
    {
        /// <summary>
        /// 
        /// </summary>
        private ISampleProvider source;
        private int channels;

        private const int fftSize = Globals.SPECTRUMRES * 2;
        private const int targetFrameRate = 120;

        private int frameInterval = 100;
        private int sampleCounter = 0;
        private const int outputResolution = Globals.SPECTRUMRES;
        private const int outputResolution2 = Globals.SPECTRUMRES/2;

        private const int MAXCHANNELS = 2;
        private const int BUFFERLEN = 8192;

        private BufferedFFT[] fft = new BufferedFFT[MAXCHANNELS];
        private BufferedFFT fft2;

        private ILoudnessWeighting loudnessWeighting;

        private SpectrumAnalyser analyser = new SpectrumAnalyser();

        public event EventHandler<FftEventArgs> SpectrumReady;
        public WaveFormat WaveFormat => source.WaveFormat;

        public int FrameInterval { get { return frameInterval; } }

        public SpectrumGenerator(ISampleProvider source)
        {
            this.source = source;
            this.channels = source.WaveFormat.Channels;
            this.frameInterval = source.WaveFormat.SampleRate / targetFrameRate;
            this.loudnessWeighting = new ITU_T_468_Weighting(source.WaveFormat.SampleRate/2);
            //this.loudnessWeighting = new A_Weighting(source.WaveFormat.SampleRate);

            for (int i = 0; i < MAXCHANNELS; i++)
                fft[i] = new BufferedFFT(BUFFERLEN, fftSize, loudnessWeighting);

            fft2 = new BufferedFFT(outputResolution * 4, outputResolution, new NullWeighting(outputResolution / 2));
        }


        public int Read(float[] buffer, int offset, int count)
        {
            if (source == null)
                return 0;


            int samplesRead = 0;

            try
            {
                samplesRead = source.Read(buffer, offset, count);
            }
            catch (NullReferenceException)  // TODO: FIX FILTHY HACK
            {
                return 0;
            }

            for (int i = 0; i < samplesRead; i += channels)
            {
                // add to ring buffer
                AddSample(buffer, i, channels);
            }

            return samplesRead;
        }

        private void AddSample(float[] samples, int offset, int channels)
        {
            // mono source - copy to both channels
            if (channels == 1)
            {
                fft[0].Add(samples[offset]);
                fft[1].Add(samples[offset]);
            }
            else
            {
                fft[0].Add(samples[offset]);
                fft[1].Add(samples[offset + 1]);
            }

            sampleCounter++;
            if (sampleCounter > frameInterval)
            {
                float[] f = new float[outputResolution * MAXCHANNELS];

                for (int i = 0; i < MAXCHANNELS; i++)
                {
                    fft[i].GenerateTo(f, i, outputResolution, MAXCHANNELS);
                }

                float[] f2 = new float[outputResolution2];
                fft2.Add(MixChannels(f, outputResolution * MAXCHANNELS, MAXCHANNELS).Select(x => x / (float)MAXCHANNELS));
                fft2.GenerateTo(f2, 0, outputResolution2);

                var analysisSample = new AudioAnalysisSample(f, f2, new float[Globals.AUDIODATASIZE], frameInterval);

                analyser.Process(analysisSample);

                SpectrumReady?.Invoke(this, new FftEventArgs(analysisSample));

                sampleCounter = 0;
            }
        }

        private IEnumerable<float> MixChannels(float[] samples, int count, int MAXCHANNELS)
        {
            float total = 0f;
            int c = MAXCHANNELS;
            for (int i=0;i<count;i++)
            {
                total += samples[i];
                if (--c == 0)
                {
                    yield return total;
                    c = MAXCHANNELS;
                    total = 0f;
                }
            }
        }

    }


    public class FftEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public FftEventArgs(AudioAnalysisSample sample)
        {
            Sample = sample;
        }

        public AudioAnalysisSample Sample { get; set; }
    }

}
