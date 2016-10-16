using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    4, 5,
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

        }

        private void VisHost_UpdateFrame(object sender, FrameEventArgs e)
        {

        }
    }
}
