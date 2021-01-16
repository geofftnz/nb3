using OpenTKExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTKExtensions.Framework;
using nb3.Player;
using System.Diagnostics;
using nb3.Player.Analysis;
using nb3.Common;
using OpenTKExtensions.Resources;

namespace nb3.Vis
{

    /// <summary>
    /// Holds the textures that store the spectrum and other audio-derived data.
    /// These are incrementally updated from the audio analysis
    /// </summary>
    public class GlobalTextures : GameComponentBase
    {
        /// <summary>
        /// Number of samples that are kept in the texture ring-buffer
        /// This is the V-coordinate dimension of the textures.
        /// </summary>
        public const int SAMPLEHISTORY = 1024;

        /// <summary>
        /// Resolution of the audio spectrum.
        /// This is the U-coordinate dimension of the textures.
        /// </summary>
        //public const int SPECTRUMRES = Globals.SPECTRUMRES;

        /// <summary>
        /// FFT of FFT, so half the size
        /// </summary>
        //public const int SPECTRUM2RES = Globals.SPECTRUM2RES;


        public Texture SpectrumTex { get; private set; }
        public Texture Spectrum2Tex { get; private set; }
        public Texture AudioDataTex { get; private set; }

        /// <summary>
        /// Texture V coordinate of last sample written.
        /// </summary>
        public int SamplePosition { get; private set; } = 0;

        /// <summary>
        /// Relative position of buffer sample within texture
        /// </summary>
        public float SamplePositionRelative
        {
            get
            {
                return (float)SamplePosition / (float)SAMPLEHISTORY;
            }
        }

        /// <summary>
        /// Total frames received. Used to synchronize playback cursor.
        /// </summary>
        public long TotalSamples { get; private set; } = 0;

        private Stopwatch timer = Stopwatch.StartNew();
        public int SampleRate { get; set; } = 44100;
        private int samplesPerFrame = 100;
        private long estimatedSamples = 0;
        private long sampleCorrection = 0;

        public void Reset()
        {
            TotalSamples = 0;
            SamplePosition = 0;
            sampleCorrection = 0;
            timer = Stopwatch.StartNew();
        }

        public long EstimatedSamples
        {
            get
            {
                long correctionShift = 0;
                estimatedSamples = (long)(timer.Elapsed.TotalSeconds * (double)(SampleRate)) + sampleCorrection;


                if (estimatedSamples < TotalSamples - samplesPerFrame)
                    correctionShift += 2;

                if (estimatedSamples + correctionShift > TotalSamples)
                    correctionShift += TotalSamples - (estimatedSamples + correctionShift);

                if (estimatedSamples + correctionShift < TotalSamples - 22000)
                    correctionShift += TotalSamples - (estimatedSamples + correctionShift);

                sampleCorrection += correctionShift;

                return estimatedSamples;
            }
        }
        public float EstimatedSamplePositionRelative
        {
            get
            {
                return (float)((EstimatedSamples / samplesPerFrame) % SAMPLEHISTORY) / (float)SAMPLEHISTORY;
            }
        }

        public GlobalTextures()
        {
            Resources.Add(SpectrumTex =
                Texture.RG32f("spectrum", Globals.SPECTRUMRES, SAMPLEHISTORY,
                    TextureParameterName.TextureMagFilter.SetTo(TextureMagFilter.Linear),
                    TextureParameterName.TextureMinFilter.SetTo(TextureMinFilter.Linear),
                    TextureParameterName.TextureWrapS.SetTo(TextureWrapMode.ClampToEdge),
                    TextureParameterName.TextureWrapT.SetTo(TextureWrapMode.Repeat)
                ));
            Resources.Add(Spectrum2Tex =
                Texture.R32f("spectrum2", Globals.SPECTRUM2RES, SAMPLEHISTORY,
                    TextureParameterName.TextureMagFilter.SetTo(TextureMagFilter.Linear),
                    TextureParameterName.TextureMinFilter.SetTo(TextureMinFilter.Linear),
                    TextureParameterName.TextureWrapS.SetTo(TextureWrapMode.ClampToEdge),
                    TextureParameterName.TextureWrapT.SetTo(TextureWrapMode.Repeat)
                ));
            Resources.Add(AudioDataTex =
                Texture.R32f("audiodata", Globals.AUDIODATASIZE, SAMPLEHISTORY,
                    TextureParameterName.TextureMagFilter.SetTo(TextureMagFilter.Linear),
                    TextureParameterName.TextureMinFilter.SetTo(TextureMinFilter.Linear),
                    TextureParameterName.TextureWrapS.SetTo(TextureWrapMode.ClampToEdge),
                    TextureParameterName.TextureWrapT.SetTo(TextureWrapMode.Repeat)
                ));

            SpectrumTex.LoadEmpty = true;
            Spectrum2Tex.LoadEmpty = true;
            AudioDataTex.LoadEmpty = true;

        }

        public void PushSample(AudioAnalysisSample sample)
        {
            if (sample.Spectrum.Length < Globals.SPECTRUMRES * 2)
                throw new IndexOutOfRangeException("spectrumData not large enough");

            samplesPerFrame = sample.Samples;
            TotalSamples += sample.Samples;
            SamplePosition++;
            SamplePosition %= SAMPLEHISTORY;

            SpectrumTex.RefreshImage(sample.Spectrum, 0, SamplePosition, Globals.SPECTRUMRES, 1);
            Spectrum2Tex.RefreshImage(sample.Spectrum2, 0, SamplePosition, Globals.SPECTRUM2RES, 1);
            AudioDataTex.RefreshImage(sample.AudioData, 0, SamplePosition, Globals.AUDIODATASIZE, 1);
        }

    }
}
