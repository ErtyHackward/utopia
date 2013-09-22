using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.Main;
using Utopia.Worlds.SkyDomes;
using S33M3CoreComponents.Cameras;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3DXEngine.Textures;
using SharpDX.Direct3D11;
using S33M3DXEngine;
using Utopia.Resources.Effects.Terran;
using Utopia.Shared.Settings;
using S33M3Resources.Structs.Vertex;
using Utopia.Effects.Shared;
using S33M3DXEngine.RenderStates;
using S33M3CoreComponents.WorldFocus;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.GameClocks;

namespace Utopia.Worlds.Shadows
{
    public class WorldShadowMap : DrawableGameComponent
    {
        private int _smDrawID;

        private Color4 _whiteColor = new Color4(255, 255, 255, 255);
        private bool _debugSMTextureNeedToBeSaved = false;

        private HLSLTerranShadow _shadowMapEffect;

        private ISkyDome _skydome;
        private CameraManager<ICameraFocused> _camManager;
        private DrawableTex2D _shadowMap;
        private D3DEngine _d3dEngine;
        public IWorldChunks WorldChunks;
        private WorldFocusManager _worldFocusManager;
        private IClock _clock;

        public Vector3 BackUpLightDirection;
        public Matrix LightViewProjection;
        private const int ShadowMapSize = 4096;

        public DrawableTex2D ShadowMap
        {
            get { return _shadowMap; }
        }

        public WorldShadowMap(
                                ISkyDome skydome,
                                CameraManager<ICameraFocused> camManager,
                                D3DEngine d3dEngine,
                                WorldFocusManager worldFocusManager,
                                IClock clock
                             )
        {
            DrawOrders.UpdateIndex(0, 99, "SM_CREATION");
            _smDrawID = DrawOrders.AddIndex(10000, "SM_DRAW");

            _d3dEngine = d3dEngine;
            _skydome = skydome;
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
                //Draw the Dephbuffer texture
                _shadowMap.DrawDepthBuffer(context, ref depthBufferDrawSize);
            }
            else
            {
                CreateLightViewProjectionMatrix(out LightViewProjection);

                _shadowMap.Begin();

                Matrix worldFocus = Matrix.Identity;

                foreach (var chunk in WorldChunks.Chunks.Where(x => x.isExistingMesh4Drawing))
                {
                    _worldFocusManager.CenterTranslationMatrixOnFocus(ref chunk.World, ref worldFocus);
                    _shadowMapEffect.Begin(context);
                    _shadowMapEffect.CBPerDraw.Values.LightWVP = Matrix.Transpose(worldFocus * LightViewProjection);
                    _shadowMapEffect.CBPerDraw.IsDirty = true;
                    _shadowMapEffect.Apply(context);

                    chunk.DrawSolidFaces(context);
                }

                _shadowMap.End();

                _d3dEngine.SetRenderTargetsAndViewPort();

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
                BackUpLightDirection = _skydome.LightDirection;
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

        private void CreateLightViewProjectionMatrixOLD(out Matrix lightProjection)
        {
            Vector2 boxSize = new Vector2(160, 160);
            int texSize = 2048;
            Vector3 direction = _skydome.LightDirection;
            // SOURCE: http://www.gamedev.net/topic/591684-xna-40---shimmering-shadow-maps/         

            // CREATE A BOX CENTERED AROUND THE pCAMERA POSITION:                      
            Vector3 min = -new Vector3(boxSize.X / 2, 0, boxSize.Y / 2);
            min.Y = 0;
            Vector3 max = new Vector3(boxSize.X / 2, 0, boxSize.Y / 2);
            max.Y = 128;
            BoundingBox boxWS = new BoundingBox(min, max);

            // CREATE A VIEW MATRIX OF THE SHADOW CAMERA            
            Vector3 shadowCamPos = Vector3.Zero;
            shadowCamPos.Y = 128;

            Vector3 target = (shadowCamPos + (direction * 10));
            Matrix shadowViewMatrix = Matrix.LookAtLH(shadowCamPos, target, MVector3.Up);
            // TRANSFORM THE BOX INTO LIGHTSPACE COORDINATES:            
            Vector3[] cornersWS = boxWS.GetCorners();
            Vector4[] cornersLS = new Vector4[cornersWS.Length];
            Vector3.Transform(cornersWS, ref shadowViewMatrix, cornersLS);

            Vector3[] cornersLS3 = new Vector3[cornersLS.Length];
            for (int i = 0; i < cornersLS.Length; i++)
            {
                Vector4 c = cornersLS[i];
                cornersLS3[i] = new Vector3(c.X, c.Y, c.Z);
            }

            BoundingBox box = BoundingBox.FromPoints(cornersLS3);
            // CREATE PROJECTION MATRIX            
            Matrix shadowProjMatrix = Matrix.OrthoOffCenterLH(box.Minimum.X, box.Maximum.X, box.Minimum.Y, box.Maximum.Y, -box.Maximum.Z, -box.Minimum.Z);
            Matrix shadowViewProjMatrix = shadowViewMatrix * shadowProjMatrix;
            
            lightProjection = shadowViewProjMatrix;
            return;


            Vector4 shadowOrigin = Vector3.Transform(Vector3.Zero, shadowViewProjMatrix);
            shadowOrigin *= (texSize / 2.0f);
            Vector2 roundedOrigin = new Vector2((float)Math.Round(shadowOrigin.X), (float)Math.Round(shadowOrigin.Y));
            Vector2 rounding = roundedOrigin - new Vector2(shadowOrigin.X, shadowOrigin.Y);
            rounding /= (texSize / 2.0f);
            Matrix roundMatrix = Matrix.Translation(new Vector3(rounding.X, rounding.Y, 0.0f));
            shadowViewProjMatrix *= roundMatrix;
            lightProjection = shadowViewProjMatrix;
        }
        #endregion



        
    }
}
