using System;
using System.Linq;
using Ninject;
using SharpDX;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.Main;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Resources.VertexFormats;
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

namespace Utopia.Worlds.Shadows
{
    public class WorldShadowMap : DrawableGameComponent
    {
#if DEBUG
        private int _smDrawID;
#endif

        private Color4 _whiteColor = new Color4(255, 255, 255, 255);
        private bool _debugSMTextureNeedToBeSaved = false;

        private HLSLTerranShadow _shadowMapEffect;

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
        public IWorldChunks WorldChunks { get; set; }

        public WorldShadowMap(
                                CameraManager<ICameraFocused> camManager,
                                D3DEngine d3dEngine,
                                WorldFocusManager worldFocusManager,
                                IClock clock
                             )
        {
            DrawOrders.UpdateIndex(0, 99, "SM_CREATION");
#if DEBUG
            _smDrawID = DrawOrders.AddIndex(10000, "SM_DRAW");
#endif

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

            _shadowMapEffect = new HLSLTerranShadow(context.Device, ClientSettings.EffectPack + @"Terran/ShadowMap.hlsl", VertexCubeSolid.VertexDeclaration);

            _shadowMapEffect.TerraTexture.Value = WorldChunks.Terra_View;
            _shadowMapEffect.SamplerDiffuse.Value = RenderStatesRepo.GetSamplerState(Utopia.Shared.GameDXStates.DXStates.Samplers.UVClamp_MinMagMipPoint);
        }

        private Vector2 depthBufferDrawSize = new Vector2(128, 128);
        public override void Draw(DeviceContext context, int index)
        {
            if (index == _smDrawID)
            {
                //Draw the DephtBuffer texture
#if DEBUG
                _shadowMap.DrawDepthBuffer(context, ref depthBufferDrawSize);
#endif
            }
            else
            {
                CreateLightViewProjectionMatrix(out LightViewProjection);

                _shadowMap.Begin();

                // draw players
                DynamicEntityManager.VoxelDraw(context, LightViewProjection);

                // draw chunks
                Matrix worldFocus = Matrix.Identity;
                foreach (var chunk in WorldChunks.Chunks.Where(x => x.Graphics.IsExistingMesh4Drawing))
                {
                    _worldFocusManager.CenterTranslationMatrixOnFocus(ref chunk.World, ref worldFocus);
                    _shadowMapEffect.Begin(context);
                    _shadowMapEffect.CBPerDraw.Values.LightWVP = Matrix.Transpose(worldFocus * LightViewProjection);
                    _shadowMapEffect.CBPerDraw.IsDirty = true;
                    _shadowMapEffect.Apply(context);

                    chunk.Graphics.DrawSolidFaces(context);

                    WorldChunks.PrepareVoxelDraw(context, LightViewProjection);
                    WorldChunks.DrawStaticEntities(context, chunk);
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
