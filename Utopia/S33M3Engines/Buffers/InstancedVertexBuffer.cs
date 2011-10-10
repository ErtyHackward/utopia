using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.Struct.Vertex.Helper;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX.Direct3D11;
using SharpDX;
using SharpDX.Direct3D;

namespace S33M3Engines.Buffers
{
    public class InstancedVertexBuffer<FixedDataType, InstancedDataType> : IDisposable
        where FixedDataType : struct
        where InstancedDataType : struct
    {
        #region Private variables
        BufferDescription _descriptionFixedData;
        Buffer _vertexBufferFixedData;
        VertexBufferBinding _bindingFixed;
        int _vertexCountFixedData;


        BufferDescription _descriptionInstancedData;
        Buffer _vertexBufferInstancedData;
        int _vertexCountInstancedData;
        VertexBufferBinding _bindingInstanced;
        int _InstancedDataMaxBufferCount;



        VertexDeclaration _vertexDeclatation;
        PrimitiveTopology _primitiveTopology;

        D3DEngine _d3dEngine;

        public int VertexCount { get { return _vertexCountFixedData; } set { _vertexCountFixedData = value; } }
        #endregion

        public InstancedVertexBuffer(D3DEngine d3dEngine, VertexDeclaration vertexDeclatation, PrimitiveTopology primitiveTopology)
        {
            //Fixed Data part Buffer ========================
            //Create the buffer for the fixed data
            _descriptionFixedData = new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Immutable
            };

            //instanced Data part Buffer ====================
            //Create the buffer Instanced data
            _descriptionInstancedData = new BufferDescription()
            {
                BindFlags = BindFlags.VertexBuffer,
                CpuAccessFlags = CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                Usage = ResourceUsage.Dynamic
            };

            _vertexDeclatation = vertexDeclatation;
            _primitiveTopology = primitiveTopology;
            _d3dEngine = d3dEngine;
        }

        public void SetFixedData(FixedDataType[] data)
        {
            _vertexCountFixedData = data.Length;

            if (_vertexBufferFixedData != null)
            {
                throw new System.ApplicationException("Error trying to update an Immutable Vertex buffer !!!");
            }

            //Create new DataStream
            DataStream verticesFixedData = new DataStream(_vertexCountFixedData * _vertexDeclatation.PerVertex_vertexStride, false, true);
            verticesFixedData.WriteRange(data);
            verticesFixedData.Position = 0; //Set the pointer to the beggining of the datastream

            //Create new Buffer
            _descriptionFixedData.SizeInBytes = _vertexCountFixedData * _vertexDeclatation.PerVertex_vertexStride;
            _vertexBufferFixedData = new Buffer(_d3dEngine.Device, verticesFixedData, _descriptionFixedData);

            verticesFixedData.Dispose();

            _bindingFixed = new VertexBufferBinding(_vertexBufferFixedData, _vertexDeclatation.PerVertex_vertexStride, 0);
        }

        public void SetInstancedData(InstancedDataType[] data)
        {
            _vertexCountInstancedData = data.Length;

            if (_vertexBufferInstancedData == null || _vertexCountInstancedData > _InstancedDataMaxBufferCount)
            {
                _InstancedDataMaxBufferCount = _vertexCountInstancedData;

                if (_vertexBufferInstancedData != null) _vertexBufferInstancedData.Dispose();

                //Create the buffer
                //Create new DataStream
                DataStream verticesInstancedData = new DataStream(_vertexCountInstancedData * _vertexDeclatation.PerInstance_vertexStride, false, true);
                verticesInstancedData.WriteRange(data);
                verticesInstancedData.Position = 0; //Set the pointer to the beggining of the datastream

                //Create new Buffer
                _descriptionInstancedData.SizeInBytes = _vertexCountInstancedData * _vertexDeclatation.PerInstance_vertexStride;
                _vertexBufferInstancedData = new Buffer(_d3dEngine.Device, verticesInstancedData, _descriptionInstancedData);

                verticesInstancedData.Dispose();
            }
            else
            {
                //Update the buffer
                DataBox databox = _d3dEngine.Context.MapSubresource(_vertexBufferInstancedData, 0, MapMode.WriteDiscard, MapFlags.None);
                databox.Data.Position = 0;
                databox.Data.WriteRange(data);
                databox.Data.Position = 0;
                _d3dEngine.Context.UnmapSubresource(_vertexBufferInstancedData, 0);
            }

            _bindingInstanced = new VertexBufferBinding(_vertexBufferInstancedData, _vertexDeclatation.PerInstance_vertexStride, 0);

        }

        public void SetToDevice(int Offset)
        {
            if (VertexBuffer.LastPrimitiveTopology != _primitiveTopology)
            {
                _d3dEngine.Context.InputAssembler.PrimitiveTopology = _primitiveTopology;
                VertexBuffer.LastPrimitiveTopology = _primitiveTopology;
            }

            _bindingInstanced.Offset = Offset;
            _d3dEngine.Context.InputAssembler.SetVertexBuffers(0, _bindingFixed, _bindingInstanced);
        }


        #region IDisposable Members
        public void Dispose()
        {
            if (_vertexBufferFixedData != null) _vertexBufferFixedData.Dispose();
            if (_vertexBufferInstancedData != null) _vertexBufferInstancedData.Dispose();
        }
        #endregion
    }
}
