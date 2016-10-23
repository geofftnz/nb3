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
        public const int SAMPLEHISTORY = 1024;
        public const int SPECTRUMRES = 1024;
        public Texture SpectrumTex { get; private set; }


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

    }
}
