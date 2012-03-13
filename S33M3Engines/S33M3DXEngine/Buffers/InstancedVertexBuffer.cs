using System;
using S33M3DXEngine.VertexFormat;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace S33M3DXEngine.Buffers
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

        Device _device;

        public int VertexCount { get { return _vertexCountFixedData; } set { _vertexCountFixedData = value; } }
        #endregion

        public InstancedVertexBuffer(Device device, VertexDeclaration vertexDeclatation, PrimitiveTopology primitiveTopology)
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
            _device = device;
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
            _vertexBufferFixedData = new Buffer(_device, verticesFixedData, _descriptionFixedData);

            verticesFixedData.Dispose();

            _bindingFixed = new VertexBufferBinding(_vertexBufferFixedData, _vertexDeclatation.PerVertex_vertexStride, 0);
        }

        public void SetInstancedData(DeviceContext context, InstancedDataType[] data)
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
                _vertexBufferInstancedData = new Buffer(_device, verticesInstancedData, _descriptionInstancedData);

                verticesInstancedData.Dispose();
            }
            else
            {
                //Update the buffer
                DataStream dataStream;
                DataBox databox = context.MapSubresource(_vertexBufferInstancedData, 0, MapMode.WriteDiscard, MapFlags.None, out dataStream);
                dataStream.Position = 0;
                dataStream.WriteRange(data);
                dataStream.Position = 0;
                context.UnmapSubresource(_vertexBufferInstancedData, 0);
                dataStream.Dispose();
            }

            _bindingInstanced = new VertexBufferBinding(_vertexBufferInstancedData, _vertexDeclatation.PerInstance_vertexStride, 0);

        }

        public void SetToDevice(DeviceContext context, int Offset)
        {
            if (D3DEngine.SingleThreadRenderingOptimization)
            {
                if (VertexBuffer.LastPrimitiveTopology != _primitiveTopology)
                {
                    context.InputAssembler.PrimitiveTopology = _primitiveTopology;
                    VertexBuffer.LastPrimitiveTopology = _primitiveTopology;
                }
            }
            else
            {
                context.InputAssembler.PrimitiveTopology = _primitiveTopology;
            }

            _bindingInstanced.Offset = Offset;
            context.InputAssembler.SetVertexBuffers(0, _bindingFixed, _bindingInstanced);
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
