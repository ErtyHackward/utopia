using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;

namespace S33M3DXEngine.Effects.HLSLFramework
{
    public class ShaderSampler
    {
        #region Private variables
        private int[] _slot = new int[ShaderIDs.NbrShaders];
        private bool _isDirty;
        private SamplerState _sampler;
        Shaders _shadersImpacted;
        string _name;
        #endregion

        #region Public properties and Variables
        public SamplerState Value
        {
            get { return _sampler; }
            set { _sampler = value; _isDirty = true; }
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
        #endregion

        public ShaderSampler(string Name)
        {
            _name = Name;
        }

        public void Set2Device(DeviceContext context, bool forced = false)
        {
            if (_isDirty || forced)
            {
                if ((_shadersImpacted & Shaders.VS) == Shaders.VS) context.VertexShader.SetSampler(_slot[ShaderIDs.VS], _sampler);
                if ((_shadersImpacted & Shaders.GS) == Shaders.GS) context.GeometryShader.SetSampler(_slot[ShaderIDs.GS], _sampler);
                if ((_shadersImpacted & Shaders.PS) == Shaders.PS) context.PixelShader.SetSampler(_slot[ShaderIDs.PS], _sampler);

                _isDirty = false;
            }
        }
    }
}
