using OpenTKExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTKExtensions.Framework;
using nb3.Player;
using System.Diagnostics;

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
        public const int SPECTRUMRES = 1024;

        public Texture SpectrumTex { get; private set; }

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
        private int sampleRate = 44100;  // TODO: supply this from player, or move this calc out
        private int samplesPerFrame = 100;
        private long estimatedSamples = 0;
        private long sampleCorrection = 0;

        public long EstimatedSamples
        {
            get
            {
                long correctionShift = 0;
                estimatedSamples = (long)(timer.Elapsed.TotalSeconds * (double)(sampleRate)) + sampleCorrection;


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
            SpectrumTex = new Texture("spectrum", SPECTRUMRES, SAMPLEHISTORY, TextureTarget.Texture2D, PixelInternalFormat.Rg32f, PixelFormat.Rg, PixelType.Float);

            Loading += GlobalTextures_Loading;
            Unloading += GlobalTextures_Unloading;
        }

        private void GlobalTextures_Loading(object sender, EventArgs e)
        {
            SpectrumTex.SetParameter(new TextureParameterInt(TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear));
            SpectrumTex.SetParameter(new TextureParameterInt(TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear));
            SpectrumTex.SetParameter(new TextureParameterInt(TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge));
            SpectrumTex.SetParameter(new TextureParameterInt(TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat));
            SpectrumTex.UploadEmpty();
        }
        private void GlobalTextures_Unloading(object sender, EventArgs e)
        {
            SpectrumTex.Unload();
        }

        public void PushSample(AudioAnalysisSample sample)
        {
            if (sample.Spectrum.Length < SPECTRUMRES * 2)
                throw new IndexOutOfRangeException("spectrumData not large enough");

            samplesPerFrame = sample.Samples;
            TotalSamples += sample.Samples;
            SamplePosition++;
            SamplePosition %= SAMPLEHISTORY;

            SpectrumTex.RefreshImage(sample.Spectrum, 0, SamplePosition, SPECTRUMRES, 1);
        }
    }
}
