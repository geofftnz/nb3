using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using System.Threading;
using OpenTKExtensions;
using OpenTKExtensions.Framework;
using OpenTKExtensions.Text;

namespace nb3.Vis
{
    public class VisHost : GameWindow
    {
        private GameComponentCollection components = new GameComponentCollection();
        private Font font;
        private TextManager text;

        private TextBlock title = new TextBlock("t0", "NeuralBeat3 ©2016 Geoff Thornburrow", new Vector3(0.0f, 0.05f, 0f), 0.0005f, new Vector4(1f, 0.8f, 0.2f, 1f));
        private Matrix4 overlayProjection;
        private Matrix4 overlayModelview;

        public VisHost()
            : base(800, 600,
                 new OpenTK.Graphics.GraphicsMode(
                     new OpenTK.Graphics.ColorFormat(8, 8, 8, 8), 24, 8),
                    "NB3",
                    GameWindowFlags.Default,
                    DisplayDevice.Default,
                    4,0,
                    OpenTK.Graphics.GraphicsContextFlags.ForwardCompatible
                 )
        {
            this.VSync = VSyncMode.Off;

            this.UpdateFrame += VisHost_UpdateFrame;
            this.RenderFrame += VisHost_RenderFrame;
            this.Load += VisHost_Load;
            this.Unload += VisHost_Unload;
            this.Resize += VisHost_Resize;
            this.Closed += VisHost_Closed;
            this.Closing += VisHost_Closing;

            // create components
            //components.Add(font = new Font(@"res\font\calibrib.ttf_sdf.2048.png", @"res\font\calibrib.ttf_sdf.2048.txt"), 1);
            components.Add(font = new Font(@"res\font\lucon.ttf_sdf.1024.png", @"res\font\lucon.ttf_sdf.1024.txt"), 1);
            components.Add(text = new TextManager(), 2);
            font.Loaded += (s, e) => { text.Font = font; };

        }

        private void VisHost_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
        }

        private void VisHost_Closed(object sender, EventArgs e)
        {

        }

        private void VisHost_Resize(object sender, EventArgs e)
        {
            SetProjection();
            components.Resize(ClientRectangle.Width, ClientRectangle.Height);
        }

        private void VisHost_Unload(object sender, EventArgs e)
        {
            components.Unload();
        }

        private void VisHost_Load(object sender, EventArgs e)
        {
            // create components

            components.Load();
            SetProjection();
        }

        private void VisHost_RenderFrame(object sender, FrameEventArgs e)
        {
            text.AddOrUpdate(title);

            GL.ClearColor(0.0f, 0.1f, 0.4f, 1.0f);
            GL.ClearDepth(1.0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);

            text.Render();

            SwapBuffers();
        }

        private void VisHost_UpdateFrame(object sender, FrameEventArgs e)
        {

            Thread.Sleep(0);
        }

        private void SetProjection()
        {
            GL.Viewport(this.ClientRectangle);
            SetOverlayProjection();

            //SetGBufferCombineProjection();
        }

        private void SetOverlayProjection()
        {
            overlayProjection = Matrix4.CreateOrthographicOffCenter(0.0f, (float)this.ClientRectangle.Width / (float)this.ClientRectangle.Height, 1.0f, 0.0f, 0.001f, 10.0f);
            overlayModelview = Matrix4.Identity * Matrix4.CreateTranslation(0.0f, 0.0f, -1.0f);

            this.text.Projection = this.overlayProjection;
            this.text.Modelview = this.overlayModelview;
        }
    }
}
