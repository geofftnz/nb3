using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTKExtensions;
using OpenTKExtensions.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Vis.Renderers.Components
{
    public class BasicShaderHost : OperatorComponentBase, IRenderable, IReloadable, ITransformable
    {
        public Matrix4 ViewMatrix { get; set; } = Matrix4.Identity;
        public Matrix4 ModelMatrix { get; set; } = Matrix4.Identity;
        public Matrix4 ProjectionMatrix { get; set; } = Matrix4.Identity;

        private FrameData frameData = null;

        public BasicShaderHost(string vertexName, string fragmentName) : base(vertexName, fragmentName)
        {
            TextureBinds = () =>
            {
                if (frameData != null)
                {
                    frameData.GlobalTextures.SpectrumTex.Bind(TextureUnit.Texture0);
                    frameData.GlobalTextures.Spectrum2Tex.Bind(TextureUnit.Texture1);
                    frameData.GlobalTextures.AudioDataTex.Bind(TextureUnit.Texture2);
                }
            };

            SetShaderUniforms = (sp) =>
            {
                if (frameData != null && sp != null)
                {
                    sp
                    .SetUniform("spectrumTex", 0)
                    .SetUniform("spectrum2Tex", 1)
                    .SetUniform("audioDataTex", 2)
                    .SetUniform("projectionMatrix", ProjectionMatrix)
                    .SetUniform("modelMatrix", ModelMatrix)
                    .SetUniform("viewMatrix", ViewMatrix)
                    .SetUniform("currentPosition", frameData.GlobalTextures.SamplePositionRelative)
                    .SetUniform("currentPositionEst", frameData.GlobalTextures.EstimatedSamplePositionRelative);
                }
            };
        }

        public override void Render(IFrameRenderData renderData)
        {
            frameData = renderData as FrameData;
            base.Render(renderData);
        }

    }
}
