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
using OpenTK.Input;
using OpenTKExtensions.Input;

namespace nb3.Vis.Renderers
{
    public class AnalysisDebugRenderer : CompositeGameComponent, IRenderable, IUpdateable, IReloadable
    {
        private DebugSpectrumWaterfall waterfall;
        private DebugSpectrum spectrum;
        private DebugAudioData datagraphs;

        private Matrix4 projection = Matrix4.Identity;
        private Matrix4 view = Matrix4.Identity;
        private Matrix4 waterfallModel = Matrix4.Identity;
        private Matrix4 spectrumModel = Matrix4.Identity;
        private Matrix4 audioDataModel = Matrix4.Identity;

        private KeyboardActionManager keyboardActions;

        private float ypos = 0f;
        private float ypostarget = 0f;
        private const float yshift = 0.05f;

        public AnalysisDebugRenderer()
        {
            projection = Matrix4.CreateOrthographicOffCenter(0f, 1f, 0f, 1f, 0.0f, 10f);
            waterfallModel = Matrix4.CreateScale(1.0f, 0.8f, 1.0f) * Matrix4.CreateTranslation(0.0f, 0.2f, 0.0f);
            spectrumModel = Matrix4.CreateScale(1.0f, 0.2f, 1.0f) * Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f);
            audioDataModel = Matrix4.CreateScale(1.0f, 20.0f, 1.0f) * Matrix4.CreateTranslation(0.0f, -20.0f, 0.0f);

            components.Add(waterfall = new DebugSpectrumWaterfall() { DrawOrder = 1, ModelMatrix = waterfallModel, ProjectionMatrix = projection });
            components.Add(spectrum = new DebugSpectrum() { DrawOrder = 2, ModelMatrix = spectrumModel, ProjectionMatrix = projection });
            components.Add(datagraphs = new DebugAudioData() { DrawOrder = 3, ModelMatrix = audioDataModel, ProjectionMatrix = projection });
            components.Add(keyboardActions = new KeyboardActionManager(), 1);

            keyboardActions.Add(Key.Up, 0, () => { ypostarget -= yshift;  });
            keyboardActions.Add(Key.Down, 0, () => { ypostarget += yshift;  });
            
        }

        public override void Update(IFrameUpdateData frameData)
        {
            ypos = ypos * 0.8f + 0.2f * ypostarget;
            if (Math.Abs(ypos - ypostarget) < 0.0005)
                ypos = ypostarget;

            UpdateView();
            base.Update(frameData);
        }

        private void UpdateView()
        {
            view = Matrix4.CreateTranslation(0f, ypos, 0f);

            components.Do<ITransformable>(c => c.ViewMatrix = view);
        }

    }
}
