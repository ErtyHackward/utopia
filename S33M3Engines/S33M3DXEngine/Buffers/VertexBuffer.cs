using System;
using S33M3DXEngine.VertexFormat;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace S33M3DXEngine.Buffers
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
        Device _device;
        DataStream _vertices;
        VertexDeclaration _vertexDeclatation;
        DataBox _databox;
        BufferDescription _description;
        string _bufferName;
        int _vertexCount;
        int _bufferCount;
        int _autoResizePerc;

        public int BufferCount { get { return _bufferCount; } }
        public int VertexCount { get { return _vertexCount; } set { _vertexCount = value; } }
        #endregion

        #region Public Properties
        #endregion
        public VertexBuffer(Device device, int vertexCount, VertexDeclaration vertexDeclatation, PrimitiveTopology primitiveTopology, string bufferName, ResourceUsage usage = ResourceUsage.Default, int AutoResizePerc = 0)            
        {                    
            _autoResizePerc = AutoResizePerc;
            _vertexCount = 0;
            _bufferCount = vertexCount + ((int)(vertexCount * AutoResizePerc / 100));
            _bufferName = bufferName;
            _vertexDeclatation = vertexDeclatation;
            _primitiveTopology = primitiveTopology;
            _device = device;

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

        public void SetData(DeviceContext context, dataType[] data, bool MapUpdate = false)
        {
            SetData(context, data, 0, data.Length, MapUpdate);
        }

        public void SetData(DeviceContext context, dataType[] data, int offset, int vertexCount, bool MapUpdate = false)
        {
            _vertexCount = vertexCount;
            //Do I need to create my Buffer or re-create it because its size is not enough (autoresize) ?
            if (_vertexBuffer == null || (_vertexCount > _bufferCount))
            {
                //Compute VB new size
                _bufferCount = _vertexCount + (_vertexCount * _autoResizePerc / 100);

                //Dispose resources as they will be recreated with larget size !
                if (_vertexBuffer != null) { _vertexBuffer.Dispose(); }
                if (_vertices != null) { _vertices.Dispose(); }

                //Create new DataStream
                _vertices = new DataStream(_bufferCount * _vertexDeclatation.VertexStride, false, true);
                _vertices.WriteRange(data, offset, _vertexCount);
                _vertices.Position = 0; //Set the pointer to the beggining of the datastream

                //Create the new Databox
                _databox = new DataBox(_vertices.DataPointer, _vertexDeclatation.VertexStride, _bufferCount * _vertexDeclatation.VertexStride);

                //Create new Buffer
                _description.SizeInBytes = _bufferCount * _vertexDeclatation.VertexStride;
                _vertexBuffer = new Buffer(_device, _vertices, _description);
#if DEBUG
                //Set resource Name, will only be done at debug time.
                _vertexBuffer.DebugName = "VertexBuffer : " + _bufferName;
#endif
            }
            else
            {
                //Using MapSubresource
                if (MapUpdate || _vertexBuffer.Description.Usage == ResourceUsage.Dynamic)
                {
                    DataBox databox = context.MapSubresource(_vertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
                    //Write Data to Pointer without changing the Position value (Fastest way)
                    Utilities.Write(databox.DataPointer, data, offset, _vertexCount);                 
                    context.UnmapSubresource(_vertexBuffer, 0);
                }
                else
                {
                    //Write Data to Pointer without changing the Position value (Fastest way)
                    //Using UpdateSubresource
                    Utilities.Write(_vertices.DataPointer, data, offset, _vertexCount);   //Write data to the buffer stream
                    context.UpdateSubresource(_databox, _vertexBuffer, 0);                  //Push the data to the GPU
                }
            }
            _binding = new VertexBufferBinding(_vertexBuffer, _vertexDeclatation.VertexStride, 0);
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

            _binding.Offset = Offset;
            context.InputAssembler.SetVertexBuffers(0, _binding);
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
