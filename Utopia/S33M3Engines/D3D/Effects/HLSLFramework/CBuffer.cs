using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX;

namespace S33M3Engines.D3D.Effects
{
    public interface iCBuffer : IDisposable
    {
        bool IsDirty { get; set; }
        string Name { get; set; }
        Shaders ShadersImpacted { get; set; }
        int[] Slot { get; set; }
        void Set2Device(bool forced = false);
        void Update();
    }

    public class CBuffer<T> : iCBuffer, IDisposable
        where T : struct
    {
        #region Private Variables
        bool _isDirty;
        string _name;
        Shaders _shadersImpacted;
        Buffer _CBuffer;
        D3DEngine _engine;
        DataStream _dataStream;
        int[] _slot = new int[ShaderIDs.NbrShaders];
        int _size;
        #endregion

        #region public Properties
        public T Values = default(T);
        public bool MarshalUpdate = false;

        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public Shaders ShadersImpacted
        {
            get { return _shadersImpacted; }
            set { _shadersImpacted = value; }
        }
        public int[] Slot
        {
            get { return _slot; }
            set { _slot = value; }
        }
        #endregion

        public CBuffer(D3DEngine engine, string Name)
        {
            _name = Name;
            _engine = engine;

            _size = Marshal.SizeOf(typeof(T));
            _CBuffer = new Buffer(_engine.Device,
                                    new BufferDescription
                                    {
                                        Usage = ResourceUsage.Default,
                                        BindFlags = BindFlags.ConstantBuffer,
                                        SizeInBytes = _size,
                                        CpuAccessFlags = CpuAccessFlags.None,
                                        OptionFlags = ResourceOptionFlags.None,
                                        StructureByteStride = 0
                                    }
            );

            Tools.Resource.SetName(_CBuffer, "CBuffer : " + Name);

            _dataStream = new DataStream(_size, true, true);
        }

        public void Update()
        {
            //If the T struct use Marshaling fct, then the only way to do an update is to use the StructureToPtr, but it is slower than the Write<T> !!!
            if (MarshalUpdate)
            {
                Marshal.StructureToPtr(Values, _dataStream.DataPointer, false);
            }
            else
            {
                _dataStream.Write<T>(Values);
                _dataStream.Position = 0;
            }

            DataBox dataBox = new DataBox(_size, _size, _dataStream);
            _engine.Context.UpdateSubresource(dataBox, _CBuffer, 0);
        }

        public void Set2Device(bool forced = false)
        {
            if (_isDirty || forced)
            {
                Update();
                if ((_shadersImpacted & Shaders.VS) == Shaders.VS) _engine.Context.VertexShader.SetConstantBuffer(_slot[ShaderIDs.VS], _CBuffer);
                if ((_shadersImpacted & Shaders.GS) == Shaders.GS) _engine.Context.GeometryShader.SetConstantBuffer(_slot[ShaderIDs.GS], _CBuffer);
                if ((_shadersImpacted & Shaders.PS) == Shaders.PS) _engine.Context.PixelShader.SetConstantBuffer(_slot[ShaderIDs.PS], _CBuffer);

                _isDirty = false;
            }
        }

        public void Dispose()
        {
            _dataStream.Dispose();
            _CBuffer.Dispose();
        }
    }
}
