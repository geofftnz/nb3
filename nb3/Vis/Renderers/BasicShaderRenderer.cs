using nb3.Vis.Renderers.Components;
using OpenTKExtensions.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Vis.Renderers
{
    public class BasicShaderRenderer : CompositeGameComponent, IRenderable, IUpdateable, IReloadable, IKeyboardControllable
    {

        private BasicShaderHost shaderHost;

        public BasicShaderRenderer()
        {
            components.Add(shaderHost = new BasicShaderHost(@"shaderhost.glsl|vert", @"effects/pulse.glsl|effect"));
        }


    }
}
