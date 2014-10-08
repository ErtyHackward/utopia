using System;
using System.Collections.Generic;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Particules;
using S33M3DXEngine;
using S33M3DXEngine.Effects.HLSLFramework;
using S33M3DXEngine.Main;
using S33M3DXEngine.Textures;
using S33M3Resources.Structs;
using SharpDX.Direct3D11;
using Utopia.Components;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.GameDXStates;
using Utopia.Shared.Settings;
using Utopia.Worlds.Weather;

namespace Utopia.Particules
{
    /// <summary>
    /// Allows to display particles on model in the editor
    /// </summary>
    public class EditorParticuleEngine : ParticuleEngine
    {
        private readonly IWeather _weather;
        private ShaderResourceView _particulesSpritesResource;
        private SpriteStaticEmitter _staticEntityEmitter;
        private VoxelModelInstance _instance;

        public EditorParticuleEngine(D3DEngine d3dEngine, CameraManager<ICameraFocused> cameraManager, SharedFrameCB sharedFrameBuffer, IWeather weather) : base(d3dEngine, sharedFrameBuffer.CBPerFrame)
        {
            _weather = weather;
        }

        public override void LoadContent(DeviceContext context)
        {
            ArrayTexture.CreateTexture2DFromFiles(_d3dEngine.Device, context, ClientSettings.TexturePack + @"Particules/", @"*.png", FilterFlags.Point, "ArrayTexture_Particules", out _particulesSpritesResource);
            ToDispose(_particulesSpritesResource);

            _staticEntityEmitter = new SpriteStaticEmitter(this, DXStates.Samplers.UVWrap_MinMagMipLinear, _particulesSpritesResource, DXStates.Rasters.Default, DXStates.Blenders.Enabled, DXStates.DepthStencils.DepthReadEnabled, _weather);
            AddEmitter(context, _staticEntityEmitter);

            base.LoadContent(context);
        }

        public void SetInstance(VoxelModelInstance instance)
        {
            _instance = instance;
        }

        public override void FTSUpdate(GameTime timeSpent)
        {
            if (_instance == null)
                return;

            if (_instance.ParticuleLastEmit == null)
                _instance.ParticuleLastEmit = new List<DateTime>(_instance.State.PartsStates.Count);

            for (int i = 0; i < _instance.State.PartsStates.Count; i++)
            {
                var partState = _instance.State.PartsStates[i];

                if (partState.Particlules != null && (DateTime.Now - _instance.ParticuleLastEmit[i]).TotalSeconds > partState.Particlules.EmittedParticuleRate)
                {
                    _staticEntityEmitter.EmitParticule(
                        partState.Particlules,
                        new Vector3D(_instance.World.TranslationVector)
                        );
                    _instance.ParticuleLastEmit[i] = DateTime.Now;
                }
            }

            base.FTSUpdate(timeSpent);
        }

        public override Vector3D CameraPosition
        {
            get { return new Vector3D();}
        }
    }
}