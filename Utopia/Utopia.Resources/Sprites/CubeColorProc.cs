﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Sprites2D;
using S33M3CoreComponents.Sprites3D.Interfaces;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine.Main;
using S33M3Resources.Effects.Sprites;
using S33M3Resources.Primitives;
using S33M3Resources.Structs;
using S33M3Resources.Structs.Vertex;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Utopia.Resources.Effects.Particules;
using Utopia.Shared.Settings;

namespace Utopia.Resources.Sprites
{
    public class CubeColorProc : BaseComponent, ISprite3DProcessor
    {
        #region Private Variables
        private List<VertexCubeColor> _spritesvertexCollection;
        private List<ushort> _spritesIndexCollection;
        private VertexBuffer<VertexCubeColor> _vb;
        private IndexBuffer<ushort> _ib;
        private bool _isCollectionDirty;
        private HLSLCubeColorParticule _effect;
        private Include _sharedCBIncludeHandler;
        private iCBuffer _frameSharedCB;

        private Vector3[] vbCube;
        private short[] ibCube;
        #endregion

        #region Public Properties
        #endregion
        public CubeColorProc(Include sharedCBIncludeHandler, iCBuffer frameSharedCB)
        {
            _sharedCBIncludeHandler = sharedCBIncludeHandler;
            _frameSharedCB = frameSharedCB;
        }

        #region Public Methods
        public void Init(DeviceContext context, ResourceUsage usage = ResourceUsage.Dynamic)
        {
            //Get primitive data
            Generator.Cube(1.0f, out vbCube, out ibCube);

            _effect = ToDispose(new HLSLCubeColorParticule(context.Device, ClientSettings.EffectPack + @"Particules/CubeColorParticule.hlsl", VertexCubeColor.VertexDeclaration, _frameSharedCB, _sharedCBIncludeHandler));

            _spritesvertexCollection = new List<VertexCubeColor>();
            _vb = ToDispose(new VertexBuffer<VertexCubeColor>(context.Device, 16, PrimitiveTopology.TriangleList, "VB Sprite3DColorBillBoardProcessor", usage, 10));

            _spritesIndexCollection = new List<ushort>();
            _ib = ToDispose(new IndexBuffer<ushort>(context.Device, 6, "CubeColorProc_iBuffer"));

            _isCollectionDirty = false;
        }

        public void Begin()
        {
            _spritesvertexCollection.Clear(); //Free buffer;
            _spritesIndexCollection.Clear();
        }

        public void SetData(DeviceContext context)
        {
            if (_isCollectionDirty)
            {
                _vb.SetData(context, _spritesvertexCollection.ToArray());
                _ib.SetData(context, _spritesIndexCollection.ToArray());
                _isCollectionDirty = false;
            }
        }

        public void Set2DeviceAndDraw(DeviceContext context)
        {
            if (_vb.VertexCount == 0) return;

            //Set Effect Constant Buffer
            _effect.Begin(context);
            _effect.Apply(context);

            _vb.SetToDevice(context, 0);
            _ib.SetToDevice(context, 0);

            context.DrawIndexed(_ib.IndicesCount, 0, 0);
        }

        //Add a new Cube
        public void Draw(ref ByteColor color, ref ByteColor ambiantColor, ref Matrix tranform)
        {
            int vertexOffset = _spritesvertexCollection.Count;

            //Create the 24 vertex + 36 Index data par cube !
            for (int i = 0; i < vbCube.Length; i++)
            {
                _spritesvertexCollection.Add(new VertexCubeColor(ref vbCube[i], ref color, ref ambiantColor, ref tranform));
            }

            foreach (var index in ibCube)
            {
                _spritesIndexCollection.Add((ushort)(index + vertexOffset));
            }

            _isCollectionDirty = true;
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
