using System;
using S33M3DXEngine.VertexFormat;
using S33M3Resources.VertexFormats.Interfaces;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace S33M3DXEngine.Buffers
{
    public class InstancedVertexBuffer<FixedDataType, InstancedDataType> : IDisposable
        where FixedDataType : struct
        where InstancedDataType : struct, IVertexType
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

        DataStream _verticesInstancedData;
        DataBox _databoxInstanced;
        bool _withDynamicInstanceBuffer;

        string _bufferName;

        Device _device;

        public int VertexCount { get { return _vertexCountFixedData; } set { _vertexCountFixedData = value; } }
        public int VertexCountInstancedData { get { return _vertexCountInstancedData; } }
        #endregion

        public InstancedVertexBuffer(Device device, PrimitiveTopology primitiveTopology, string bufferName, bool withDynamicInstanceBuffer = false)
        {
            _withDynamicInstanceBuffer = withDynamicInstanceBuffer;
            _bufferName = bufferName;

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
                CpuAccessFlags = _withDynamicInstanceBuffer ? CpuAccessFlags.Write : CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                Usage = _withDynamicInstanceBuffer ? ResourceUsage.Dynamic : ResourceUsage.Default
            };

            _vertexDeclatation = ((IVertexType)new InstancedDataType()).VertexDeclaration;
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
                if (_verticesInstancedData != null) _verticesInstancedData.Dispose();

                //Create the buffer
                //Create new DataStream
                _verticesInstancedData = new DataStream(_vertexCountInstancedData * _vertexDeclatation.PerInstance_vertexStride, false, true);
                _verticesInstancedData.WriteRange(data);
                _verticesInstancedData.Position = 0; //Set the pointer to the beggining of the datastream

                _databoxInstanced = new DataBox(_verticesInstancedData.DataPointer, _vertexDeclatation.PerInstance_vertexStride, _InstancedDataMaxBufferCount * _vertexDeclatation.PerInstance_vertexStride);

                //Create new Buffer
                _descriptionInstancedData.SizeInBytes = _vertexCountInstancedData * _vertexDeclatation.PerInstance_vertexStride;
                _vertexBufferInstancedData = new Buffer(_device, _verticesInstancedData, _descriptionInstancedData);

#if DEBUG
                //Set resource Name, will only be done at debug time.
                _vertexBufferInstancedData.DebugName = "Instanced Buffer : " + _bufferName;
#endif
            }
            else
            {
                //A dynamic buffer can only be update by MAP/UNMAP operation
                if (_withDynamicInstanceBuffer)
                {
                    //MAP
                    DataBox databox = context.MapSubresource(_vertexBufferInstancedData, 0, MapMode.WriteDiscard, MapFlags.None);
                    //Write Data to Pointer without changing the Position value (Fastest way)
                    Utilities.Write(databox.DataPointer, data, 0, _vertexCountInstancedData);
                    //UNMAP
                    context.UnmapSubresource(_vertexBufferInstancedData, 0);
                }
                else
                {
                    Utilities.Write(_verticesInstancedData.DataPointer, data, 0, _vertexCountInstancedData);   //Write data to the buffer stream
                    context.UpdateSubresource(_databoxInstanced, _vertexBufferInstancedData, 0);                  //Push the data to the GPU
                }
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
            if (_verticesInstancedData != null) _verticesInstancedData.Dispose();
        }
        #endregion
    }
}
