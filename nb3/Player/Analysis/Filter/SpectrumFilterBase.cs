using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis.Filter
{
    public abstract class SpectrumFilterBase
    {
        /// <summary>
        /// Name of this filter instance, prepended to output slot names.
        /// </summary>
        public string Name { get; set; }
        protected string[] SlotNames;

        public SpectrumFilterBase(string name, params string[] slots)
        {
            Name = name;
            SlotNames = slots;
        }

        public string GetOutputName(int slot)
        {
            if (slot<0 || slot>= SlotNames.Length)
            {
                throw new ArgumentOutOfRangeException("slot");
            }
            return $"{Name}_{SlotNames[slot]}";
        }
    }
}
