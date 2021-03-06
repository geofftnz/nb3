﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nb3.Player.Analysis.Filter
{
    public interface ISpectrumFilter
    {
        int OutputSlotCount { get; }
        int OutputOffset { get; set; }
        float[] GetValues(FilterParameters frame);
        void Reset();
        string GetOutputName(int slot);
    }
}
