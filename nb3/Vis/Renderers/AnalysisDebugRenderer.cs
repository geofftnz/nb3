using OpenTKExtensions.Framework;
using OpenTKExtensions;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nb3.Vis.Renderers.Components;

namespace nb3.Vis.Renderers
{
    public class AnalysisDebugRenderer : CompositeGameComponent, IRenderable, IUpdateable, IReloadable
    {
        private DebugSpectrumWaterfall waterfall;
        private DebugSpectrum spectrum;

        private Matrix4 projection = Matrix4.Identity;
        private Matrix4 waterfallModelView = Matrix4.Identity;
        private Matrix4 spectrumModelView = Matrix4.Identity;

        public AnalysisDebugRenderer()
        {
            projection = Matrix4.CreateOrthographicOffCenter(0f, 1f, 0f, 1f, 0.0f, 10f);
            waterfallModelView = Matrix4.CreateScale(1.0f, 0.8f, 1.0f) * Matrix4.CreateTranslation(0.0f, 0.2f, 0.0f);
            spectrumModelView = Matrix4.CreateScale(1.0f, 0.2f, 1.0f) * Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);

            components.Add(waterfall = new DebugSpectrumWaterfall() { DrawOrder = 1, ModelMatrix = waterfallModelView, ProjectionMatrix = projection });
            components.Add(spectrum = new DebugSpectrum() { DrawOrder = 2, ModelMatrix = spectrumModelView, ProjectionMatrix = projection });
        }

    }
}
