using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis.Filter
{
    /// <summary>
    /// Simple filter interface. 
    /// </summary>
    public interface IFilter
    {
        float Get(float input);
    }
}
