﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noises.Interfaces
{
    public interface INoise2 : INoise
    {
        float GetValue(float x, float z);
    }
}
