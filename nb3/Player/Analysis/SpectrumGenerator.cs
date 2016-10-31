using NAudio.Dsp;
using NAudio.Wave;
using nb3.Player.Analysis;
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

        private const int fftSize = 2048;
        private const int targetFrameRate = 120;

        private int frameInterval = 100;
        private int sampleCounter = 0;
        private const int outputResolution = 1024;

        private const int MAXCHANNELS = 2;
        private const int BUFFERLEN = 8192;
        private RingBuffer<float>[] ringbuffer = new RingBuffer<float>[MAXCHANNELS];
        private LoudnessWeighting loudnessWeighting;


        private FFT fft = new FFT(fftSize);

        public event EventHandler<FftEventArgs> SpectrumReady;
        public WaveFormat WaveFormat => source.WaveFormat;

        public int FrameInterval { get { return frameInterval; } }

        public SpectrumGenerator(ISampleProvider source)
        {
            this.source = source;
            this.channels = source.WaveFormat.Channels;
            this.frameInterval = source.WaveFormat.SampleRate / targetFrameRate;
            this.loudnessWeighting = new LoudnessWeighting(source.WaveFormat.SampleRate);

            for (int i = 0; i < MAXCHANNELS; i++)
                ringbuffer[i] = new RingBuffer<float>(BUFFERLEN);
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
                ringbuffer[0].Add(samples[offset]);
                ringbuffer[1].Add(samples[offset]);
            }
            else
            {
                ringbuffer[0].Add(samples[offset]);
                ringbuffer[1].Add(samples[offset + 1]);
            }

            sampleCounter++;
            if (sampleCounter > frameInterval)
            {
                float[] f = new float[outputResolution * MAXCHANNELS];

                for (int i = 0; i < MAXCHANNELS; i++)
                {
                    fft.Generate(ringbuffer[i]);
                    fft.CopyTo(f, i, outputResolution, MAXCHANNELS);

                    int jj = i;
                    for(int j = 0; j < outputResolution; j++)
                    {
                        float freq = (float)j / (float)(outputResolution - 1);
                        f[jj] *= (float)loudnessWeighting[freq];
                        jj += MAXCHANNELS;
                    }
                }

                float[] audioData = new float[256];// TODO: Move sizes to global class

                // temporary audio data
                for (int i = 0; i < 256; i++)
                {
                    //audioData[i] = (f[i * 2] + f[i * 2 + 1]) * 0.5f;
                    audioData[i] = 0f;
                }
                for (int i = 0; i < 32; i++)
                {
                    audioData[1] += f[i];
                }
                audioData[1] /= 32f;

                // TODO: pass spectrum off for further processing into audioData[]

                var analysisSample = new AudioAnalysisSample(f, audioData, frameInterval);

                SpectrumReady?.Invoke(this, new FftEventArgs(analysisSample));

                sampleCounter = 0;
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
