using System;
using System.Linq;
using Ninject;
using SharpDX;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.Main;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Resources.VertexFormats;
using Utopia.Shared.GameDXStates;
using Utopia.Worlds.SkyDomes;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3DXEngine.Textures;
using SharpDX.Direct3D11;
using S33M3DXEngine;
using Utopia.Resources.Effects.Terran;
using Utopia.Shared.Settings;
using S33M3DXEngine.RenderStates;
using S33M3CoreComponents.WorldFocus;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.GameClocks;
using Utopia.Resources.Effects.Entities;
using Utopia.Entities.Voxel;

namespace Utopia.Worlds.Shadows
{
    public class WorldShadowMap : DrawableGameComponent
    {
        private int _smDrawID;


        private Color4 _whiteColor = new Color4(255, 255, 255, 255);
        private bool _debugSMTextureNeedToBeSaved = false;

        private HLSLTerranShadow _landScapeShadowMapEffect;
        private HLSLVoxelModelInstancedShadow _entitiesShadowMapEffect;

        private CameraManager<ICameraFocused> _camManager;
        private DrawableTex2D _shadowMap;
        private D3DEngine _d3dEngine;
        private WorldFocusManager _worldFocusManager;
        private IClock _clock;
        
        public Vector3 BackUpLightDirection;
        public Matrix LightViewProjection;
        private const int ShadowMapSize = 4096;

        public DrawableTex2D ShadowMap
        {
            get { return _shadowMap; }
        }

        [Inject]
        public IVisualDynamicEntityManager DynamicEntityManager { get; set; }

        [Inject]
        public ISkyDome SkyDome { get; set; }

        [Inject]
        public IWorldChunks2D WorldChunks { get; set; }

        public WorldShadowMap(
                                CameraManager<ICameraFocused> camManager,
                                D3DEngine d3dEngine,
                                WorldFocusManager worldFocusManager,
                                IClock clock
                             )
        {
            DrawOrders.UpdateIndex(0, 99, "SM_CREATION");

            _smDrawID = DrawOrders.AddIndex(10000, "SM_DRAW");


            _d3dEngine = d3dEngine;
            _camManager = camManager;
            _worldFocusManager = worldFocusManager;
            _clock = clock;
        }

        #region Public Methods
        public override void Initialize()
        {
        }

        public override void LoadContent(DeviceContext context)
        {
            _shadowMap = ToDispose(new DrawableTex2D(_d3dEngine));
            _shadowMap.Init(ShadowMapSize, ShadowMapSize, false, SharpDX.DXGI.Format.R32_Float);

            _landScapeShadowMapEffect = ToDispose(new HLSLTerranShadow(context.Device, ClientSettings.EffectPack + @"Terran/Terran.hlsl", VertexCubeSolid.VertexDeclaration));
            _entitiesShadowMapEffect = ToDispose(new HLSLVoxelModelInstancedShadow(_d3dEngine.Device, ClientSettings.EffectPack + @"Entities\VoxelModelInstanced.hlsl", VoxelInstanceData.VertexDeclaration));
        }

        private Vector2 depthBufferDrawSize = new Vector2(128, 128);
        public override void Draw(DeviceContext context, int index)
        {
            if (index == _smDrawID)
            {
                //Draw the DephtBuffer texture

                _shadowMap.DrawDepthBuffer(context, ref depthBufferDrawSize);

            }
            else
            {
                CreateLightViewProjectionMatrix(out LightViewProjection);

                var m = Matrix.Translation(_camManager.ActiveCamera.WorldPosition.ValueInterp.AsVector3());
                m.Invert();

                _shadowMap.Begin();

                // draw players
                DynamicEntityManager.VoxelDraw(context, m * LightViewProjection);

                // draw chunks
                Matrix worldFocus = Matrix.Identity;
                foreach (var chunk in WorldChunks.Chunks.Where(x => x.Graphics.IsExistingMesh4Drawing))
                {
                    _worldFocusManager.CenterTranslationMatrixOnFocus(ref chunk.World, ref worldFocus);
                    _landScapeShadowMapEffect.Begin(context);
                    _landScapeShadowMapEffect.CBPerDraw.Values.LightWVP = Matrix.Transpose(worldFocus * LightViewProjection);
                    _landScapeShadowMapEffect.CBPerDraw.IsDirty = true;
                    _landScapeShadowMapEffect.Apply(context);

                    chunk.Graphics.DrawSolidFaces(context);
                }

                //WorldChunks.PrepareVoxelDraw(context, m * LightViewProjection);
                _entitiesShadowMapEffect.Begin(context);
                _entitiesShadowMapEffect.CBPerDraw.Values.LightWVP = Matrix.Transpose(m * LightViewProjection);
                _entitiesShadowMapEffect.CBPerDraw.IsDirty = true;
                _entitiesShadowMapEffect.Apply(context);
                foreach (var chunk in WorldChunks.Chunks.Where(x => x.Graphics.IsExistingMesh4Drawing && x.DistanceFromPlayer <= WorldChunks.StaticEntityViewRange))
                {
                    WorldChunks.DrawStaticEntitiesShadow(context, chunk);
                }

                _shadowMap.End();

                _d3dEngine.SetRenderTargetsAndViewPort(context);

#if DEBUG
                if (!_debugSMTextureNeedToBeSaved)
                {
                    //Texture2D.ToFile(context, _shadowMap.DepthMap.Resource, ImageFileFormat.Dds, @"E:\1.dds");
                    _debugSMTextureNeedToBeSaved = true;
                }
#endif
            }
        }
        #endregion

        #region Private Methods
        private float lastLightUpdate = -100.0f;

        private void CreateLightViewProjectionMatrix(out Matrix lightProjection)
        {
            if (Math.Abs(lastLightUpdate - _clock.ClockTime.ClockTimeNormalized2) > 0.005f)
            {
                BackUpLightDirection = SkyDome.LightDirection;
                lastLightUpdate = _clock.ClockTime.ClockTimeNormalized2;
            }
            
            BoundingSphere sphere = new BoundingSphere(Vector3.Zero, 100);

            const float ExtraBackup = 20.0f;
            const float NearClip = 1.0f;

            Vector3 lightDirection = BackUpLightDirection * -1;
            //lightDirection.Z += 0.3f;
            lightDirection.Normalize();

            float backupDist = ExtraBackup + NearClip + sphere.Radius;
            Vector3 shadowCamPos = sphere.Center + (lightDirection * backupDist);
            Matrix shadowViewMatrix = Matrix.LookAtLH(shadowCamPos, sphere.Center, Vector3.UnitY);

            float bounds = sphere.Radius * 2.0f;
            float farClip = backupDist + sphere.Radius;
            Matrix shadowProjMatrix = Matrix.OrthoLH(bounds, bounds, NearClip, farClip);
            Matrix shadowMatrix = shadowViewMatrix * shadowProjMatrix;

            Matrix roundMatrix = ComputeRoundingValue();
            shadowMatrix *= roundMatrix;

            lightProjection = shadowMatrix;
        }


        private Matrix ComputeRoundingValue()
        {
            BoundingSphere sphere = new BoundingSphere(_camManager.ActiveCamera.WorldPosition.ValueInterp.AsVector3(), 100);

            const float ExtraBackup = 20.0f;
            const float NearClip = 1.0f;

            Vector3 lightDirection = BackUpLightDirection * -1;
            lightDirection.Normalize();

            float backupDist = ExtraBackup + NearClip + sphere.Radius;
            Vector3 shadowCamPos = sphere.Center + (lightDirection * backupDist);
            Matrix shadowViewMatrix = Matrix.LookAtLH(shadowCamPos, sphere.Center, Vector3.UnitY);

            float bounds = sphere.Radius * 2.0f;
            float farClip = backupDist + sphere.Radius;
            Matrix shadowProjMatrix = Matrix.OrthoLH(bounds, bounds, NearClip, farClip);

            Matrix shadowMatrix = shadowViewMatrix * shadowProjMatrix;

            Vector4 shadowOrigin = Vector3.Transform(Vector3.Zero, shadowMatrix);
            shadowOrigin *= (ShadowMapSize / 2.0f);
            Vector2 roundedOrigin = new Vector2((float)Math.Round(shadowOrigin.X), (float)Math.Round(shadowOrigin.Y));
            Vector2 rounding = roundedOrigin - new Vector2(shadowOrigin.X, shadowOrigin.Y);
            rounding /= (ShadowMapSize / 2.0f);

            return Matrix.Translation(rounding.X, rounding.Y, 0.0f);
        }
        #endregion




    }
}
