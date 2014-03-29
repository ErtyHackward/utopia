using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;

namespace S33M3DXEngine.VertexFormat
{
    public class VertexDeclaration
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        InputElement[] _elements;
        int _vertexStride = 0;
        int _perVertex_vertexStride = 0;
        int _perInstance_vertexStride = 0;

        public InputElement[] Elements { get { return _elements; } set { _elements = value; } }
        public int VertexStride { get { return _vertexStride; } }
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

            //Check Multiple of 4 !
            if (_vertexStride % 4 != 0)
            {
                logger.Error(string.Format("Vertex declaration stride ({0}) is not multiple of 4", _vertexStride));
                throw new Exception(string.Format("Vertex declaration stride ({0}) is not multiple of 4", _vertexStride));
            }

        }
    }
}
