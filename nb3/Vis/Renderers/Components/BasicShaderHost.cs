using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTKExtensions;
using OpenTKExtensions.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Vis.Renderers.Components
{
    public class BasicShaderHost : GameComponentBase, IRenderable, IUpdateable, IReloadable, ITransformable
    {
        public int DrawOrder { get; set; } = 0;
        public bool Visible { get; set; } = true;
        public Matrix4 ViewMatrix { get; set; } = Matrix4.Identity;
        public Matrix4 ModelMatrix { get; set; } = Matrix4.Identity;
        public Matrix4 ProjectionMatrix { get; set; } = Matrix4.Identity;

        // VBOs for quad
        protected VBO vertexVBO = new VBO("basicshader_q_v");
        protected VBO indexVBO = new VBO("basicshader_q_i", BufferTarget.ElementArrayBuffer);
        protected ShaderProgram program = new ShaderProgram("basicshader_sp");

        public string VertexShaderName { get; set; }
        public string FragmentShaderName { get; set; }


        public BasicShaderHost(string vertexName, string fragmentName)
        {
            VertexShaderName = vertexName;
            FragmentShaderName = fragmentName;

            Loading += BasicShaderHost_Loading;
            Unloading += BasicShaderHost_Unloading;
        }

        private void BasicShaderHost_Unloading(object sender, EventArgs e)
        {
            program?.Unload();
        }

        private void BasicShaderHost_Loading(object sender, EventArgs e)
        {
            InitVBOsq();
            Reload();
        }

        private void InitVBOsq()
        {
            Vector3[] vertex = {
                                    //new Vector3(0f,1f,0f),
                                    //new Vector3(0f,0f,0f),
                                    //new Vector3(1f,1f,0f),
                                    //new Vector3(1f,0f,0f)
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
                VertexShaderName,
                FragmentShaderName,
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



        public void Render(IFrameRenderData _frameData)
        {
            FrameData frameData = (_frameData as FrameData);
            if (frameData == null)
                throw new InvalidOperationException("Unrecognised FrameData: " + (_frameData?.GetType()?.Name ?? "null"));

            //GL.ClearColor(0.8f, 0.1f, 0.4f, 1.0f);
            //GL.ClearDepth(1.0);
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            frameData.GlobalTextures.SpectrumTex.Bind(TextureUnit.Texture0);
            frameData.GlobalTextures.Spectrum2Tex.Bind(TextureUnit.Texture1);
            frameData.GlobalTextures.AudioDataTex.Bind(TextureUnit.Texture2);

            // render quad
            program.UseProgram()
                .SetUniform("spectrumTex", 0)
                .SetUniform("spectrum2Tex", 1)
                .SetUniform("audioDataTex", 2)
                .SetUniform("projectionMatrix", ProjectionMatrix)
                .SetUniform("modelMatrix", ModelMatrix)
                .SetUniform("viewMatrix", ViewMatrix)
                .SetUniform("currentPosition", frameData.GlobalTextures.SamplePositionRelative)
                .SetUniform("currentPositionEst", frameData.GlobalTextures.EstimatedSamplePositionRelative);
            vertexVBO.Bind(this.program.VariableLocation("vertex"));
            indexVBO.Bind();
            GL.DrawElements(BeginMode.Triangles, indexVBO.Length, DrawElementsType.UnsignedInt, 0);

        }

        public void Update(IFrameUpdateData frameData)
        {
        }
    }
}
