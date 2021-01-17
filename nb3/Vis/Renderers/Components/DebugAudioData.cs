using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTKExtensions;
using OpenTKExtensions.Framework;
using OpenTK.Graphics.OpenGL4;
using OpenTK;
using NLog;
using OpenTKExtensions.Text;
using nb3.Common;
using NLog.Layouts;

namespace nb3.Vis.Renderers.Components
{

    /// <summary>
    /// Renders the current state of the audio spectrum buffer. This will be a 1024x1024 1-channel fp32 texture.
    /// 
    /// The texture will be supplied from the common texture set.
    /// 
    /// </summary>
    public class DebugAudioData : OperatorComponentBase, IRenderable, IReloadable, ITransformable
    {
        public Matrix4 ViewMatrix { get; set; } = Matrix4.Identity;
        public Matrix4 ModelMatrix { get; set; } = Matrix4.Identity;
        public Matrix4 ProjectionMatrix { get; set; } = Matrix4.Identity;

        private FrameData frameData = null;

        // temporary component collection support until OperatorComponentBase can be changed to derive from a composite component.
        protected GameComponentCollection components = new GameComponentCollection();

        private List<string> filterOutputNames = new List<string>();
        protected TextManager textManager;

        public DebugAudioData(Font font, List<string> outputNames) : base(@"DebugAudioData.glsl|vert", @"DebugAudioData.glsl|frag")
        {
            TextureBinds = () =>
            {
                if (frameData != null)
                {
                    frameData.GlobalTextures.AudioDataTex.Bind(TextureUnit.Texture0);
                }
            };

            SetShaderUniforms = (sp) =>
            {
                if (frameData != null && sp != null)
                {
                    sp
                    .SetUniform("audioDataTex", 0)
                    .SetUniform("projectionMatrix", ProjectionMatrix)
                    .SetUniform("modelMatrix", ModelMatrix)
                    .SetUniform("viewMatrix", ViewMatrix)
                    .SetUniform("currentPosition", frameData.GlobalTextures.SamplePositionRelative)
                    .SetUniform("currentPositionEst", frameData.GlobalTextures.EstimatedSamplePositionRelative);
                }
            };

            Loading += DebugAudioData_Loading;
            Unloading += DebugAudioData_Unloading;
            

            if (outputNames != null)
            {
                filterOutputNames.AddRange(outputNames);
            }

            components.Add(textManager = new TextManager("tm", font));
        }

        private void DebugAudioData_Unloading(object sender, EventArgs e)
        {
            components.Unload();
        }

        private void DebugAudioData_Loading(object sender, EventArgs e)
        {
            components.Load();

            float rowsize = 2f / Globals.AUDIODATASIZE;
            int i = 0;
            
            foreach(var s in filterOutputNames)
            {
                var tb = new TextBlock($"F{i:000}", $"{i:000} {s}", new Vector3(0.0f, 0.0f + (i+1) * rowsize, 0.0f), .1f / 1024f, new Vector4(1f, 1f, 1f, .3f));
                textManager.AddOrUpdate(tb);
                i++;
            }
            //TextBlock tb1 = new TextBlock("1", "Line 1", new Vector3(0.0f, 0.0f + rowsize, 0.0f), .1f / 1024f, new Vector4(1f, 1f, 1f, 1f));
            //TextBlock tb2 = new TextBlock("2", "Line 2", new Vector3(0.0f, 0.0f + rowsize*2f, 0.0f), .1f / 1024f, new Vector4(1f, 1f, 1f, 1f));
            //textManager.AddOrUpdate(tb2);
        }

        public override void Render(IFrameRenderData renderData)
        {
            frameData = renderData as FrameData;
            base.Render(renderData);

            LayoutLabels();

            //components.Do<ITransformable>(c => { c.ViewMatrix = ViewMatrix; c.ProjectionMatrix = ProjectionMatrix; }); // TODO: temp hack until operatorcomponentbase is derived from compositecomponent

            textManager.Refresh();
            components.Render(renderData);
        }

        private bool _doneTextLayout = false;
        private void LayoutLabels()
        {
            //if (_doneTextLayout) return;


            textManager.Modelview = Matrix4.CreateScale(2f, ModelMatrix.Row1.Y, 1f) * ViewMatrix;
            textManager.Projection = ProjectionMatrix;


            _doneTextLayout = true;
        }
    }
}
