﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3_DXEngine.Debug.Interfaces
{
    public interface IDebugInfo
    {
        bool ShowDebugInfo { get; set; }
        string GetDebugInfo();
    }
}
