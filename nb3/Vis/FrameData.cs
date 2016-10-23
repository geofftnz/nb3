using OpenTKExtensions.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Vis
{
    public class FrameData : IFrameRenderData, IFrameUpdateData
    {
        public double Time { get; set; }
        public GlobalTextures GlobalTextures { get; set; }
    }
}
