using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Worlds.GameClocks;
using SharpDX;
using SharpDX.Direct3D;
using Utopia.Shared.Structs;
using Utopia.Settings;
using Utopia.Resources.Effects.Skydome;
using S33M3Resources.VertexFormats;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Cameras;
using S33M3DXEngine;
using S33M3Resources.Structs;
using SharpDX.Direct3D11;
using S33M3DXEngine.RenderStates;
using S33M3CoreComponents.Cameras.Interfaces;
using S33M3CoreComponents.Maths;

namespace Utopia.Worlds.SkyDomes.SharedComp
{
    public class SkyStars : DrawableGameComponent
    {
        #region private variables
        private int _nbrStars = 1000;
        private VertexBuffer<VertexPosition3Color> _skyStarVB;

        private HLSLStars _effectStars;
        private Matrix _world = Matrix.Identity;
        private IClock _gameClock;
        private float _visibility;
        private D3DEngine _d3dEngine;
        private CameraManager<ICameraFocused> _camManager;
        #endregion

        #region Public Properties
        #endregion

        public SkyStars(D3DEngine d3dEngine, CameraManager<ICameraFocused> camManager , IClock gameClock)
        {
            _d3dEngine = d3dEngine;
            _gameClock = gameClock;
            _camManager = camManager;
        }

        #region public methods
        public override void Initialize()
        {
            _effectStars = new HLSLStars(_d3dEngine.Device, ClientSettings.EffectPack + @"SkyDome\Stars.hlsl", VertexPosition3Color.VertexDeclaration);
            CreateBuffer();
        }

        public override void Dispose()
        {
            _effectStars.Dispose();
            _skyStarVB.Dispose();
        }

        public override void Update( GameTime timeSpend)
        {
        }


        public override void Interpolation(double interpolation_hd, float interpolation_ld, long timePassed)
        {
        }

        public override void Draw(DeviceContext context, int index)
        {
            RenderStatesRepo.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Enabled);

            _effectStars.Begin(context);
            _effectStars.CBPerDraw.Values.ViewProjection = Matrix.Transpose(_camManager.ActiveCamera.ViewProjection3D_focused);

            //Compute Vibility !
            if (_gameClock.ClockTime.ClockTimeNormalized < 0.25) _visibility = 1;     // Before 06:00 am
            else
                if (_gameClock.ClockTime.ClockTimeNormalized >= 0.25 && _gameClock.ClockTime.ClockTimeNormalized < 0.30) _visibility = MathHelper.FullLerp(1, 0, 0.25, 0.30, _gameClock.ClockTime.ClockTimeNormalized); //Between 04:00am and 06:00am
                else
                    if (_gameClock.ClockTime.ClockTimeNormalized >= 0.30 && _gameClock.ClockTime.ClockTimeNormalized < 0.75) _visibility = 0;     //Between 06:00am and 6:00pm
                    else
                        if (_gameClock.ClockTime.ClockTimeNormalized >= 0.75 && _gameClock.ClockTime.ClockTimeNormalized < 0.85) _visibility = MathHelper.FullLerp(0, 1, 0.75, 0.85, _gameClock.ClockTime.ClockTimeNormalized); //Between 06:00pm and 08:00pm
                        else
                            _visibility = 1;

            _visibility = Math.Max(_visibility, Math.Min(Math.Max((float)_camManager.ActiveCamera.WorldPosition.Y - 127, 0), 173) / 173.0f);

            _effectStars.CBPerDraw.Values.Visibility = _visibility;
            _effectStars.CBPerDraw.IsDirty = true;

            _effectStars.Apply(context);

            _skyStarVB.SetToDevice(context, 0);
            context.Draw(_skyStarVB.VertexCount, 0);
        }
        #endregion

        #region Private Methods
        private void CreateBuffer()
        {
            VertexPosition3Color[] vertices = GenerateSpherePoints(_nbrStars, 1500);
            _skyStarVB = new VertexBuffer<VertexPosition3Color>(_d3dEngine.Device, vertices.Length, VertexPosition3Color.VertexDeclaration, PrimitiveTopology.PointList, "_skyStarVB");
            _skyStarVB.SetData(_d3dEngine.ImmediateContext, vertices);
        }

        //http://www.cgafaq.info/wiki/Random_Points_On_Sphere
        private VertexPosition3Color[] GenerateSpherePoints(int n, float scale)
        {
            Random rnd = new Random();
            VertexPosition3Color[] result = new VertexPosition3Color[n];
            int size;
            int i;
            double x, y, z, w, t;

            for (i = 0; i < n; i++)
            {
                z = 2.0 * rnd.NextDouble() - 1.0;
                t = 2.0 * Math.PI * rnd.NextDouble();
                w = Math.Sqrt(1 - z * z);
                x = w * Math.Cos(t);
                y = w * Math.Sin(t);
                size = rnd.Next(0, 256);
                result[i] = new VertexPosition3Color(new Vector3((float)x * scale, (float)y * scale, (float)z * scale), new ByteColor(size, 0, 0, 0));
            }

            return result;
        }
        #endregion
    }
}
