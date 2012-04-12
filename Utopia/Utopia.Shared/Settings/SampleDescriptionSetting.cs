using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.DXGI;
using System.Xml.Serialization;

namespace Utopia.Shared.Settings
{
    public class SampleDescriptionSetting
    {
        public SampleDescription SampleDescription;

        [XmlIgnore]
        public int QualityWeight
        {
            get
            {
                if (SampleDescription.Count == 1) return 0;
                if (SampleDescription.Quality == 0)
                {
                    return SampleDescription.Count;
                }

                if (SampleDescription.Quality > 0)
                {
                    if (SampleDescription.Count == 4 && SampleDescription.Quality == 8) return 9;
                    if (SampleDescription.Count == 8 && SampleDescription.Quality == 8) return 10;
                    if (SampleDescription.Count == 4 && SampleDescription.Quality == 16) return 17;
                    if (SampleDescription.Count == 8 && SampleDescription.Quality == 16) return 18;
                    if (SampleDescription.Count == 4 && SampleDescription.Quality == 32) return 33;
                    if (SampleDescription.Count == 8 && SampleDescription.Quality == 32) return 34;
                }
                return 0;
            }
        }

        public override string ToString()
        {
            if (SampleDescription.Count == 1) return "None";
            if (SampleDescription.Quality == 0)
            {
                return SampleDescription.Count + "x MSAA";
            }

            if (SampleDescription.Quality > 0)
            {
                if (SampleDescription.Count == 4 && SampleDescription.Quality == 8) return "8x CSAA";
                if (SampleDescription.Count == 8 && SampleDescription.Quality == 8) return "8xQ CSAA";
                if (SampleDescription.Count == 4 && SampleDescription.Quality == 16) return "16x CSAA";
                if (SampleDescription.Count == 8 && SampleDescription.Quality == 16) return "16xQ CSAA";
                if (SampleDescription.Count == 8 && SampleDescription.Quality == 32) return "32x CSAA";
            }

            return "Not Supported Name";

        }
    }
}
