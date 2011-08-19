using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using S33M3Engines.Struct.Vertex.Helper;
using Buffer = SharpDX.Direct3D11.Buffer;
using S33M3Engines.D3D;
using SharpDX.Direct3D;

namespace S33M3Engines.Buffers
{
    public static class VertexBuffer
    {
        public static PrimitiveTopology LastPrimitiveTopology = PrimitiveTopology.Undefined;
        public static void ResetVertexStateTracker()
        {
            LastPrimitiveTopology = PrimitiveTopology.Undefined;
        }
    }

    public class VertexBuffer<dataType> : IDisposable where dataType : struct
    {
        #region Private variables
        VertexBufferBinding _binding;
        Buffer _vertexBuffer;
        PrimitiveTopology _primitiveTopology;
        //VertexBufferBinding _defaultBinding;
        D3DEngine _d3dEngine;
        DataStream _vertices;
        VertexDeclaration _vertexDeclatation;
        DataBox _databox;
        BufferDescription _description;
        int _vertexCount;
        int _bufferCount;
        int _autoResizePerc;

        public int BufferCount { get { return _bufferCount; } }
        public int VertexCount { get { return _vertexCount; } set { _vertexCount = value; } }
        #endregion

        #region Public Properties
        #endregion
        public VertexBuffer(D3DEngine d3dEngine, int vertexCount, VertexDeclaration vertexDeclatation, PrimitiveTopology primitiveTopology, ResourceUsage usage = ResourceUsage.Default, int AutoResizePerc = 0)
        {
            _autoResizePerc = AutoResizePerc;
            _vertexCount = vertexCount;
            _bufferCount = _vertexCount + ((int)(_vertexCount * AutoResizePerc / 100));

            _vertexDeclatation = vertexDeclatation;
            _primitiveTopology = primitiveTopology;
            _d3dEngine = d3dEngine;

            //Create the buffer description object
            _description = new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = usage == ResourceUsage.Default || usage == ResourceUsage.Immutable ? CpuAccessFlags.None : CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = _bufferCount * vertexDeclatation.VertexStride,
                Usage = usage
            };
        }

        public void SetData(dataType[] data, bool MapUpdate = false)
        {
            _vertexCount = data.Length;

            //Autoresize ??
            if (_vertexBuffer == null || (data.Length > _bufferCount))
            {
                //Compute VB new size
                _bufferCount = data.Length + (data.Length * _autoResizePerc / 100);

                if (_vertexBuffer != null) { _vertexBuffer.Dispose(); }
                if (_vertices != null) { _vertices.Dispose(); }

                //Create new DataStream
                _vertices = new DataStream(_bufferCount * _vertexDeclatation.VertexStride, false, true);

                _vertices.WriteRange(data);
                _vertices.Position = 0; //Set the pointer to the beggining of the datastream

                //Create the new Databox
                _databox = new DataBox(_vertexDeclatation.VertexStride, _bufferCount * _vertexDeclatation.VertexStride, _vertices);

                //Create new Buffer
                _description.SizeInBytes = _bufferCount * _vertexDeclatation.VertexStride;
                _vertexBuffer = new Buffer(_d3dEngine.Device, _vertices, _description);
            }
            else
            {
                if (MapUpdate || _vertexBuffer.Description.Usage == ResourceUsage.Dynamic)
                {
                    DataBox databox = _d3dEngine.Context.MapSubresource(_vertexBuffer, 0, _vertexCount * _vertexDeclatation.VertexStride, MapMode.WriteDiscard, MapFlags.None);
                    databox.Data.Position = 0;
                    databox.Data.WriteRange(data);
                    databox.Data.Position = 0;
                    _d3dEngine.Context.UnmapSubresource(_vertexBuffer, 0);
                }
                else
                {
                    _databox.Data.Position = 0;
                    _databox.Data.WriteRange(data);
                    _databox.Data.Position = 0;
                    _d3dEngine.Context.UpdateSubresource(_databox, _vertexBuffer, 0);
                }
            }

            _binding = new VertexBufferBinding(_vertexBuffer, _vertexDeclatation.VertexStride, 0);

        }

        public void SetToDevice(int Offset)
        {
            if (VertexBuffer.LastPrimitiveTopology != _primitiveTopology)
            {
                _d3dEngine.Context.InputAssembler.SetPrimitiveTopology(_primitiveTopology);
                VertexBuffer.LastPrimitiveTopology = _primitiveTopology;
            }

            _binding.Offset = Offset;

            _d3dEngine.Context.InputAssembler.SetVertexBuffers(0, _binding);

        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_vertexBuffer != null) _vertexBuffer.Dispose();
            if (_vertices != null) _vertices.Dispose();
        }

        #endregion
    }
}
