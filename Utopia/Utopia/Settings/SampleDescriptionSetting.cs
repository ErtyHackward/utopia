using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.DXGI;

namespace Utopia.Settings
{
    public class SampleDescriptionSetting
    {
        public SampleDescription SampleDescription;

        public override string ToString()
        {
            return SampleDescription.Count + "x";
        }
    }
}
