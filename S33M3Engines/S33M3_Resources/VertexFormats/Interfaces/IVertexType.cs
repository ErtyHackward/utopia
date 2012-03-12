using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_DXEngine.VertexFormat;

namespace S33M3_Resources.VertexFormats.Interfaces
{
    public interface IVertexType
    {
        // Properties
        VertexDeclaration VertexDeclaration { get; }
    }
}
