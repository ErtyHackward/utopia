using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace S33M3Engines.Struct.Vertex.Helper
{
    public class VertexDeclaration
    {
        InputElement[] _elements;
        int _vertexStride = 0;
        int _perVertex_vertexStride = 0;
        int _perInstance_vertexStride = 0;

        public InputElement[] Elements { get { return _elements; } set { _elements = value; } }
        public int VertexStride { get  {  return _vertexStride; } }
        public int PerVertex_vertexStride { get { return _perVertex_vertexStride; } }
        public int PerInstance_vertexStride { get { return _perInstance_vertexStride; } }

        public VertexDeclaration(InputElement[] Elements)
        {
            _elements = Elements;
            foreach (InputElement ie in Elements)
            {
                _vertexStride += FormatSize.GetFormatSize(ie.Format);
                switch (ie.Classification)
                {
                    case InputClassification.PerInstanceData:
                        _perInstance_vertexStride += FormatSize.GetFormatSize(ie.Format);
                        break;
                    case InputClassification.PerVertexData:
                        _perVertex_vertexStride += FormatSize.GetFormatSize(ie.Format);
                        break;
                }
            }
        }

    }
}
