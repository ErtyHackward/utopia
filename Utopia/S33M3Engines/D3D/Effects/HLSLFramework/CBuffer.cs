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
        bool GlobalCB { get; set; }
        iCBuffer Clone();
    }

    public class CBuffer<T> : iCBuffer, IDisposable
        where T : struct
    {
        #region Private Variables
        bool _isDirty;
        string _name;
        Shaders _shadersImpacted;
        Buffer _CBufferValue;
        D3DEngine _engine;
        DataStream _dataStream;
        int[] _slot;
        int _size;
        bool _isCloned = false;
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

        public bool GlobalCB { get; set; }
        #endregion

        public CBuffer(D3DEngine engine, string Name, Buffer CBufferValue)
        {
            _name = Name;
            _engine = engine;

            _slot = new int[ShaderIDs.NbrShaders];

            //Set Default slot value to -1 => Not used by the shader
            for (int iSlot = 0; iSlot <= ShaderIDs.NbrShaders - 1; iSlot++)
            {
                _slot[iSlot] = -1;
            }

            if (CBufferValue == null)
            {
                _size = Marshal.SizeOf(typeof(T));
                _CBufferValue = new Buffer(_engine.Device,
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
                _dataStream = new DataStream(_size, true, true);
                _isCloned = false;
                GlobalCB = false;
            }
            else
            {
                _CBufferValue = CBufferValue;
                _isCloned = true;
                GlobalCB = true;
            }

            Tools.Resource.SetName(_CBufferValue, "CBuffer : " + Name);
        }

        public CBuffer(D3DEngine engine, string Name)
            : this(engine, Name, null)
        {
        }

        //Create a CBuffer copy, linked to the same Buffer
        public iCBuffer Clone()
        {
            return new CBuffer<T>(_engine, this.Name, _CBufferValue);
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

            DataBox dataBox = new DataBox(_dataStream.DataPointer,_size, _size);
            _engine.Context.UpdateSubresource(dataBox, _CBufferValue, 0);
        }

        public void Set2Device(bool forced = false)
        {
            if (_isDirty || forced || GlobalCB)
            {
                if (!GlobalCB) Update();
                if ((_shadersImpacted & Shaders.VS) == Shaders.VS) _engine.Context.VertexShader.SetConstantBuffer(_slot[ShaderIDs.VS], _CBufferValue);
                if ((_shadersImpacted & Shaders.GS) == Shaders.GS) _engine.Context.GeometryShader.SetConstantBuffer(_slot[ShaderIDs.GS], _CBufferValue);
                if ((_shadersImpacted & Shaders.PS) == Shaders.PS) _engine.Context.PixelShader.SetConstantBuffer(_slot[ShaderIDs.PS], _CBufferValue);

                _isDirty = false;
            }
        }

        public void Dispose()
        {
            if (_isCloned == false)
            {
                _dataStream.Dispose();
                _CBufferValue.Dispose();
            }
        }
    }
}
