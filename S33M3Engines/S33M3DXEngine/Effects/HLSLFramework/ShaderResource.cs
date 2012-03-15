﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;

namespace S33M3DXEngine.Effects.HLSLFramework
{
    public class ShaderResource
    {
        #region Private variables
        private int[] _slot = new int[ShaderIDs.NbrShaders];
        private bool _isDirty;
        private ShaderResourceView _resourceView;
        Shaders _shadersImpacted;
        string _name;
        bool _isStaticResource;
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

        public bool IsStaticResource
        {
            get { return _isStaticResource; }
            set { _isStaticResource = value; }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Name">Resource Name</param>
        /// <param name="isStaticResource">Will this resource be considered as static, if yes then it will automatically by pushed to the device when the effect Begin()</param>
        public ShaderResource(string Name, bool isStaticResource = true)
        {
            _name = Name;
            _isStaticResource = isStaticResource;
        }

        public void Set2Device(DeviceContext context, bool forceStaticResourcesOnly)
        {
            if (forceStaticResourcesOnly && _isStaticResource == false) return;

            if (_isDirty || forceStaticResourcesOnly)
            {
                if ((_shadersImpacted & Shaders.VS) == Shaders.VS) context.VertexShader.SetShaderResource(_slot[ShaderIDs.VS], _resourceView);
                if ((_shadersImpacted & Shaders.GS) == Shaders.GS) context.GeometryShader.SetShaderResource(_slot[ShaderIDs.GS], _resourceView);
                if ((_shadersImpacted & Shaders.PS) == Shaders.PS) context.PixelShader.SetShaderResource(_slot[ShaderIDs.PS], _resourceView);

                _isDirty = false;
            }
        }
    }
}
