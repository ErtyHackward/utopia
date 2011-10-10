using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using SharpDX;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using S33M3Engines.Struct.Vertex.Helper;
using S33M3Engines.D3D;

namespace S33M3Engines.Buffers
{
    public class IndexBuffer<dataType> : IDisposable where dataType : struct
    {
        #region Private variables
        Buffer _indexBuffer;
        D3DEngine _d3dEngine;
        Format _indexFormat;
        BufferDescription _description;
        DataStream _indices;
        DataBox _databox;
        int _indexStride;
        int _indicesCount;
        int _autoResizePerc;
        int _bufferCount;
        string _bufferName;

        public int IndicesCount { get { return _indicesCount; } }
        #endregion

        #region Public Properties
        #endregion
        public IndexBuffer(D3DEngine d3dEngine, int IndicesCount, Format indexFormat, string bufferName, int AutoResizePerc = 0, ResourceUsage usage = ResourceUsage.Default)
        {
            _indicesCount = IndicesCount;
            _bufferCount = _indicesCount + (_indicesCount * _autoResizePerc / 100);

            _autoResizePerc = AutoResizePerc;
            _bufferName = bufferName;
            _d3dEngine = d3dEngine;
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

        public void SetData(dataType[] data, bool MapUpdate = false)
        {
            SetData(data, 0, data.Length, MapUpdate);
        }

        public void SetData(dataType[] data, int offset, int indiceCount, bool MapUpdate = false)
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
                _databox = new DataBox(_indices.DataPointer,_indexStride, _bufferCount * _indexStride);

                //Create new Buffer
                _description.SizeInBytes = _bufferCount * _indexStride;
                _indexBuffer = new Buffer(_d3dEngine.Device, _indices, _description);
                //Set resource Name, will only be done at debug time.
                S33M3Engines.D3D.Tools.Resource.SetName(_indexBuffer, _bufferName);
            }
            else
            {
                if (MapUpdate || _indexBuffer.Description.Usage == ResourceUsage.Dynamic)
                {
                    DataBox databox = _d3dEngine.Context.MapSubresource(_indexBuffer, 0, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None);
                    databox.Data.Position = 0;
                    databox.Data.WriteRange(data, offset, indiceCount);
                    databox.Data.Position = 0;
                    _d3dEngine.Context.UnmapSubresource(_indexBuffer, 0);
                }
                else
                {
                    _databox.Data.Position = 0;
                    _databox.Data.WriteRange(data, offset, indiceCount);
                    _databox.Data.Position = 0;
                    _d3dEngine.Context.UpdateSubresource(_databox, _indexBuffer, 0);
                }

            }

        }

        public void SetToDevice(int Offset)
        {
            _d3dEngine.Context.InputAssembler.SetIndexBuffer(_indexBuffer, _indexFormat, Offset);
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
