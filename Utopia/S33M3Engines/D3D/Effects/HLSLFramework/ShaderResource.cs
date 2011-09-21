using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;

namespace S33M3Engines.D3D.Effects
{
    public class ShaderResource
    {
        #region Private variables
        private int[] _slot = new int[ShaderIDs.NbrShaders];
        private bool _isDirty;
        private ShaderResourceView _resourceView;
        Shaders _shadersImpacted;
        string _name;
        D3DEngine _engine;
        #endregion

        #region Public properties and Variables
        public ShaderResourceView Value
        {
            get { return _resourceView; }
            set
            {
                _resourceView = value; _isDirty = true;
            }
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

        public ShaderResource(D3DEngine engine, string Name)
        {
            _name = Name;
            _engine = engine;
        }

        public void Set2Device(bool forced = false)
        {
            if (_isDirty || forced)
            {
                if ((_shadersImpacted & Shaders.VS) == Shaders.VS) _engine.Context.VertexShader.SetShaderResource(_slot[ShaderIDs.VS], _resourceView);
                if ((_shadersImpacted & Shaders.GS) == Shaders.GS) _engine.Context.GeometryShader.SetShaderResource(_slot[ShaderIDs.GS], _resourceView);
                if ((_shadersImpacted & Shaders.PS) == Shaders.PS) _engine.Context.PixelShader.SetShaderResource(_slot[ShaderIDs.PS], _resourceView);

                _isDirty = false;
            }
        }
    }
}
