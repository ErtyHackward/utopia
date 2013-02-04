using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise
{
    public interface INoise : INoise1, INoise2, INoise3, INoise4
    {
    }

    public interface INoise1
    {
        double Get(double x);
    }

    public interface INoise2
    {
        double Get(double x, double y);
    }

    public interface INoise3
    {
        double Get(double x, double y, double z);
    }

    public interface INoise4
    {
        double Get(double x, double y, double z, double w);
    }
}
