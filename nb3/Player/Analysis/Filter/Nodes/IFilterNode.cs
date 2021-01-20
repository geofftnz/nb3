using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis.Filter.Nodes
{
    /// <summary>
    /// Simple filter interface. 
    /// </summary>
    public interface IFilterNode
    {
        float Get(float input);
    }
}
