using OpenTKExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTKExtensions.Framework;

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

        public GlobalTextures()
        {
            SpectrumTex = new Texture("spectrum", SPECTRUMRES, SAMPLEHISTORY, TextureTarget.Texture2D, PixelInternalFormat.R32f, PixelFormat.Red, PixelType.Float);

            Loading += GlobalTextures_Loading;
            Unloading += GlobalTextures_Unloading;
        }

        private void GlobalTextures_Loading(object sender, EventArgs e)
        {
            SpectrumTex.SetParameter(new TextureParameterInt(TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest));
            SpectrumTex.SetParameter(new TextureParameterInt(TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest));
            SpectrumTex.SetParameter(new TextureParameterInt(TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge));
            SpectrumTex.SetParameter(new TextureParameterInt(TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge));
            SpectrumTex.UploadEmpty();
        }
        private void GlobalTextures_Unloading(object sender, EventArgs e)
        {
            SpectrumTex.Unload();
        }

        public void PushSample(float[] spectrumData)
        {
            if (spectrumData.Length < SPECTRUMRES)
                throw new IndexOutOfRangeException("spectrumData not large enough");

            SamplePosition++;
            SamplePosition %= SAMPLEHISTORY;

            SpectrumTex.RefreshImage(spectrumData, 0, SamplePosition, SPECTRUMRES, 1);
        }
    }
}
