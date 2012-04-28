using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noises.Interfaces
{
    public interface INoise3 : INoise
    {
        float GetValue(float x, float y, float z);
    }
}
