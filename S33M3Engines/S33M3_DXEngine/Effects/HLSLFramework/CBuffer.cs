using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using SharpDX;
using S33M3_DXEngine.Debug;

namespace S33M3_DXEngine.Effects.HLSLFramework
{
    public interface iCBuffer : IDisposable
    {
        bool IsDirty { get; set; }
        string Name { get; set; }
        Shaders ShadersImpacted { get; set; }
        int[] Slot { get; set; }
        void Set2Device(DeviceContext context, bool forced = false);
        void Update(DeviceContext context);
        iCBuffer Clone();
    }

    public class CBuffer<T> : iCBuffer, IDisposable
        where T : struct
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private bool _marshalUpdate;
        bool _isDirty;
        string _name;
        Shaders _shadersImpacted;
        Buffer _CBuffer;
        Device _device;
        DataStream _dataStream;
        int[] _slot = new int[ShaderIDs.NbrShaders];
        int _size;
        bool _isCloned = false;
        #endregion

        #region public Properties
        public T Values = default(T);
        
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
        public CBuffer(Device device, string Name, bool marshalUpdate = false, Buffer CBufferValue = null)
        {
            _marshalUpdate = marshalUpdate;
            _name = Name;
            _device = device;

            if (CBufferValue == null)
            {

                _size = Marshal.SizeOf(typeof(T));
                _CBuffer = new Buffer(_device,
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

                if (_marshalUpdate) _dataStream = new DataStream(_size, true, true);
                _isCloned = false;
            }
            else
            {
                _CBuffer = CBufferValue;
                _isCloned = true;
            }

#if DEBUG
            //Set resource Name, will only be done at debug time.
            _CBuffer.DebugName = "CBuffer : " + Name;
#endif
        }

        /// <summary>
        /// Send the value of the CBuffer stored in the "CPU" memory to the GPU memory.
        /// </summary>
        /// <param name="context"></param>
        public void Update(DeviceContext context)
        {
            if (_isCloned) 
            { 
#if DEBUG 
                logger.Warn("Cloned CBuffer should not by called for update, its his parent that is responsible to update the buffer !!");
#endif
                return;                     
            }

            ////If the T struct use Marshaling fct, then the only way to do an update is to use the StructureToPtr, but it is slower than the Write<T> !!!
            if (_marshalUpdate)
            {
                Marshal.StructureToPtr(Values, _dataStream.DataPointer, false);
                DataBox dataBox = new DataBox(_dataStream.DataPointer, _size, _size);
                context.UpdateSubresource(dataBox, _CBuffer, 0);
            }
            else
            {
                context.UpdateSubresource(ref Values, _CBuffer);
            }

            _isDirty = false;
        }

         //Create a CBuffer copy, linked to the same Buffer
        public iCBuffer Clone()
        {
            return new CBuffer<T>(_device, this.Name, _marshalUpdate, _CBuffer);
        }

        /// <summary>
        /// 1) Start an update of the CBuffer value on the CPU if needed (Dirty or forced)
        /// 2) Set the CBuffer to be used by the effect.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="withForcedUpdate"></param>
        public void Set2Device(DeviceContext context, bool withForcedUpdate = false)
        {
            if (_isDirty || withForcedUpdate)
            {
                Update(context); //Update constant buffer with new values (From CPU to GPU)
            }

            if ((_shadersImpacted & Shaders.VS) == Shaders.VS) context.VertexShader.SetConstantBuffer(_slot[ShaderIDs.VS], _CBuffer);
            if ((_shadersImpacted & Shaders.GS) == Shaders.GS) context.GeometryShader.SetConstantBuffer(_slot[ShaderIDs.GS], _CBuffer);
            if ((_shadersImpacted & Shaders.PS) == Shaders.PS) context.PixelShader.SetConstantBuffer(_slot[ShaderIDs.PS], _CBuffer);
        }

        public void Dispose()
        {
            if (_isCloned == false)
            {
                if (_dataStream != null) _dataStream.Dispose();
                _CBuffer.Dispose();
            }
        }
    }
}
