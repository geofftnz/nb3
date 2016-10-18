using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using System.Threading;

namespace nb3.Vis
{
    public class VisHost : GameWindow
    {
        public VisHost()
            : base(800, 600,
                 new OpenTK.Graphics.GraphicsMode(
                     new OpenTK.Graphics.ColorFormat(8, 8, 8, 8), 24, 8),
                    "NB3",
                    GameWindowFlags.Default,
                    DisplayDevice.Default,
                    3,3,
                    OpenTK.Graphics.GraphicsContextFlags.Default
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
            
        }

        private void VisHost_Unload(object sender, EventArgs e)
        {
        }

        private void VisHost_Load(object sender, EventArgs e)
        {

        }

        private void VisHost_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.ClearColor(0.0f, 0.1f, 0.4f, 1.0f);
            GL.ClearDepth(1.0);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

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
            //this.overlayProjection = Matrix4.CreateOrthographicOffCenter(0.0f, (float)this.ClientRectangle.Width / (float)this.ClientRectangle.Height, 1.0f, 0.0f, 0.001f, 10.0f);
            //this.overlayModelview = Matrix4.Identity * Matrix4.CreateTranslation(0.0f, 0.0f, -1.0f);

            //this.textManager.Projection = this.overlayProjection;
            //this.textManager.Modelview = this.overlayModelview;
        }
    }
}
