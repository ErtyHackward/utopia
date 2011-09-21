using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using UtopiaContent.Effects.Skydome;
using S33M3Engines.Buffers;
using Utopia.Worlds.GameClocks;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using SharpDX.Direct3D;
using Utopia.Shared.Structs;
using S33M3Engines.StatesManager;
using S33M3Engines.Shared.Math;
using S33M3Engines;
using S33M3Engines.Cameras;

namespace Utopia.Worlds.SkyDomes.SharedComp
{
    public class SkyStars : DrawableGameComponent
    {
        #region private variables
        private int _nbrStars = 1000;
        private VertexBuffer<VertexPositionColor> _skyStarVB;

        private HLSLStars _effectStars;
        private Matrix _world = Matrix.Identity;
        private IClock _gameClock;
        private float _visibility;
        private D3DEngine _d3dEngine;
        private CameraManager _camManager;
        #endregion

        #region Public Properties
        #endregion

        public SkyStars(D3DEngine d3dEngine, CameraManager camManager , IClock gameClock)
        {
            _d3dEngine = d3dEngine;
            _gameClock = gameClock;
            _camManager = camManager;
        }

        #region public methods
        public override void Initialize()
        {
            _effectStars = new HLSLStars(_d3dEngine, @"Effects\SkyDome\Stars.hlsl", VertexPositionColor.VertexDeclaration);
            CreateBuffer();
        }

        public override void Dispose()
        {
            _effectStars.Dispose();
            _skyStarVB.Dispose();
        }

        public override void Update(ref GameTime TimeSpend)
        {
        }


        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
        }

        public override void Draw(int Index)
        {
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default, GameDXStates.DXStates.Blenders.Enabled);

            _effectStars.Begin();
            _effectStars.CBPerDraw.Values.World = Matrix.Transpose(_world);
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

            _effectStars.Apply();

            _skyStarVB.SetToDevice(0);
            _d3dEngine.Context.Draw(_skyStarVB.VertexCount, 0);
        }
        #endregion

        #region Private Methods
        private void CreateBuffer()
        {
            VertexPositionColor[] vertices = GenerateSpherePoints(_nbrStars, 1500);
            _skyStarVB = new VertexBuffer<VertexPositionColor>(_d3dEngine, vertices.Length, VertexPositionColor.VertexDeclaration, PrimitiveTopology.PointList, "_skyStarVB");
            _skyStarVB.SetData(vertices);
        }

        //http://www.cgafaq.info/wiki/Random_Points_On_Sphere
        private VertexPositionColor[] GenerateSpherePoints(int n, float scale)
        {
            Random rnd = new Random();
            VertexPositionColor[] result = new VertexPositionColor[n];
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
                result[i] = new VertexPositionColor(new Vector3((float)x * scale, (float)y * scale, (float)z * scale), new Color(size, 0, 0));
            }

            return result;
        }
        #endregion
    }
}
