using System;
using S33M3DXEngine.VertexFormat;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace S33M3DXEngine.Buffers
{
    public class IndexBuffer<dataType> : IDisposable where dataType : struct
    {
        #region Private variables
        private Buffer _indexBuffer;
        private Device _device;
        private Format _indexFormat;
        private BufferDescription _description;
        private DataStream _indices;
        private DataBox _databox;
        private int _indexStride;
        private int _indicesCount;
        private int _autoResizePerc;
        private int _bufferCount;
        private string _bufferName;
        #endregion

        #region Public Properties
        public int IndicesCount { get { return _indicesCount; } }
        #endregion
        public IndexBuffer(Device device, int IndicesCount, Format indexFormat, string bufferName, int AutoResizePerc = 0, ResourceUsage usage = ResourceUsage.Default)
        {
            _indicesCount = 0;
            _bufferCount = IndicesCount + (IndicesCount * _autoResizePerc / 100);

            _autoResizePerc = AutoResizePerc;
            _bufferName = bufferName;
            _device = device;
            _indexFormat = indexFormat;
            _indexStride = FormatSize.GetFormatSize(indexFormat);

            //Create the buffer description object
            _description = new BufferDescription()
            {
                BindFlags = BindFlags.IndexBuffer,
                CpuAccessFlags = usage == ResourceUsage.Default || usage == ResourceUsage.Immutable ? CpuAccessFlags.None : CpuAccessFlags.Write,
                OptionFlags = ResourceOptionFlags.None,
                SizeInBytes = _bufferCount * _indexStride,
                Usage = usage
            };
        }

        public void SetData(DeviceContext context, dataType[] data, bool MapUpdate = false)
        {
            SetData(context, data, 0, data.Length, MapUpdate);
        }

        public void SetData(DeviceContext context,dataType[] data, int offset, int _indiceCount, bool MapUpdate = false)
        {
            _indicesCount = _indiceCount;

            //Autoresize ??
            if (_indexBuffer == null || (_indicesCount > _bufferCount))
            {
                //Compute IB new size
                _bufferCount = _indicesCount + (_indicesCount * _autoResizePerc / 100);

                if (_indexBuffer != null) { _indexBuffer.Dispose(); }
                if (_indices != null) { _indices.Dispose(); }

                //Create new DataStream
                _indices = new DataStream(_bufferCount * _indexStride, false, true);
                _indices.WriteRange(data, offset, _indiceCount);
                _indices.Position = 0; //Set the pointer to the beggining of the datastream

                //Create the new Databox
                _databox = new DataBox(_indices.DataPointer, _indexStride, _bufferCount * _indexStride);

                //Create new Buffer
                _description.SizeInBytes = _bufferCount * _indexStride;
                _indexBuffer = new Buffer(_device, _indices, _description);
#if DEBUG
                //Set resource Name, will only be done at debug time.
                _indexBuffer.DebugName = "indexBuffer : " + _bufferName;
#endif
            }
            else
            {
                if (MapUpdate || _indexBuffer.Description.Usage == ResourceUsage.Dynamic)
                {
                    DataBox databox = context.MapSubresource(_indexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
                    //Write Data to Pointer without changing the Position value (Fastest way)
                    Utilities.Write(databox.DataPointer, data, offset, _indiceCount);
                    context.UnmapSubresource(_indexBuffer, 0);
                }
                else
                {
                    Utilities.Write(_indices.DataPointer, data, offset, _indiceCount);   //Write data to the buffer stream
                    context.UpdateSubresource(_databox, _indexBuffer, 0);                  //Push the data to the GPU
                }
            }
        }

        public void SetToDevice(DeviceContext context, int Offset)
        {
            context.InputAssembler.SetIndexBuffer(_indexBuffer, _indexFormat, Offset);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_indexBuffer != null) _indexBuffer.Dispose();
            if (_indices != null) _indices.Dispose();
        }

        #endregion
    }
}
