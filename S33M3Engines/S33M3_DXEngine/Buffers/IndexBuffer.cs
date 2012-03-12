using System;
using S33M3_DXEngine.VertexFormat;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace S33M3_DXEngine.Buffers
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
        private DataStream _dataStream;
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
            _indicesCount = IndicesCount;
            _bufferCount = _indicesCount + (_indicesCount * _autoResizePerc / 100);

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

        public void SetData(DeviceContext context,dataType[] data, int offset, int indiceCount, bool MapUpdate = false)
        {
            _indicesCount = indiceCount;

            //Autoresize ??
            if (_indexBuffer == null || (_indicesCount > _bufferCount))
            {
                //Compute IB new size
                _bufferCount = _indicesCount + (_indicesCount * _autoResizePerc / 100);

                if (_indexBuffer != null) { _indexBuffer.Dispose(); }
                if (_indices != null) { _indices.Dispose(); }

                //Create new DataStream
                _indices = new DataStream(_bufferCount * _indexStride, false, true);
                _indices.WriteRange(data, offset, indiceCount);
                _indices.Position = 0; //Set the pointer to the beggining of the datastream

                //Create the new Databox
                _databox = new DataBox(_indices.DataPointer, _indexStride, _bufferCount * _indexStride);
                if (_dataStream != null) _dataStream.Dispose();
                _dataStream = new DataStream(_databox.DataPointer, _bufferCount * _indexStride, false, true);

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
                    DataStream dataStream;
                    DataBox databox = context.MapSubresource(_indexBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out dataStream);
                    dataStream.Position = 0;
                    dataStream.WriteRange(data, offset, indiceCount);
                    dataStream.Position = 0;
                    context.UnmapSubresource(_indexBuffer, 0);
                    dataStream.Dispose();
                }
                else
                {
                    _dataStream.Position = 0;
                    _dataStream.WriteRange(data, offset, indiceCount);
                    _dataStream.Position = 0;
                    context.UpdateSubresource(_databox, _indexBuffer, 0);
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
