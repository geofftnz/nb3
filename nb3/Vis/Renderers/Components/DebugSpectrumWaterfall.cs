using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTKExtensions;
using OpenTKExtensions.Framework;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using NLog;

namespace nb3.Vis.Renderers.Components
{

    /// <summary>
    /// Renders the current state of the audio spectrum buffer. This will be a 1024x1024 1-channel fp32 texture.
    /// 
    /// The texture will be supplied from the common texture set.
    /// 
    /// </summary>
    public class DebugSpectrumWaterfall : GameComponentBase, IRenderable, IUpdateable, IReloadable, ITransformable
    {
        public int DrawOrder { get; set; } = 1;
        public bool Visible { get; set; } = true;

        public Matrix4 ModelView { get; set; } = Matrix4.Identity;
        public Matrix4 Projection { get; set; } = Matrix4.Identity;

        // VBOs for quad
        protected VBO vertexVBO = new VBO("DebugSpectrumWaterfall_q_v");
        protected VBO indexVBO = new VBO("DebugSpectrumWaterfall_q_i", BufferTarget.ElementArrayBuffer);
        protected ShaderProgram program = new ShaderProgram("DebugSpectrumWaterfall_sp");

        public DebugSpectrumWaterfall()
        {
            Loading += DebugSpectrumWaterfall_Loading;
        }

        private void DebugSpectrumWaterfall_Loading(object sender, EventArgs e)
        {
            InitVBOsq();
            Reload();
        }


        public void Render(IFrameRenderData _frameData)
        {
            FrameData frameData = (_frameData as FrameData);
            if (frameData == null)
                throw new InvalidOperationException("Unrecognised FrameData: " + (_frameData?.GetType()?.Name ?? "null"));

            //GL.ClearColor(0.8f, 0.1f, 0.4f, 1.0f);
            //GL.ClearDepth(1.0);
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            frameData.GlobalTextures.SpectrumTex.Bind(TextureUnit.Texture0);

            // render quad
            program.UseProgram()
                .SetUniform("spectrumTex", 0)
                .SetUniform("projection", Projection)
                .SetUniform("modelview", ModelView)
                .SetUniform("currentPosition", frameData.GlobalTextures.SamplePositionRelative);
            vertexVBO.Bind(this.program.VariableLocation("vertex"));
            indexVBO.Bind();
            GL.DrawElements(BeginMode.Triangles, indexVBO.Length, DrawElementsType.UnsignedInt, 0);

        }

        public void Update(IFrameUpdateData frameData)
        {

        }


        private void InitVBOsq()
        {
            Vector3[] vertex = {
                                    new Vector3(-1f,1f,0f),
                                    new Vector3(-1f,-1f,0f),
                                    new Vector3(1f,1f,0f),
                                    new Vector3(1f,-1f,0f)
                                };
            uint[] index = {
                                0,1,2,
                                1,3,2
                            };

            this.vertexVBO.SetData(vertex);
            this.indexVBO.SetData(index);
        }

        public void Reload()
        {
            this.ReloadShader(this.LoadShader, this.SetShader, log);
        }


        private ShaderProgram LoadShader()
        {
            var program = new ShaderProgram(this.GetType().Name);
            program.Init(
                @"DebugSpectrum.glsl|vert",
                @"DebugSpectrum.glsl|waterfall_frag",
                new List<Variable>
                {
                    new Variable(0, "vertex")
                });
            return program;
        }

        private void SetShader(ShaderProgram newprogram)
        {
            if (this.program != null)
            {
                this.program.Unload();
            }
            this.program = newprogram;
        }
    }
}
