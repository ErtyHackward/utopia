using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.Maths;
using S33M3Engines.D3D;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using S33M3Engines.Buffers;
using S33M3Engines.Struct;
using Utopia.GameClock;
using UtopiaContent.Effects.Skydome;
using SharpDX.Direct3D11;
using S33M3Engines.StatesManager;
using Utopia.Shared.Structs;
using SharpDX.Direct3D;
using S33M3Engines.Shared.Math;

namespace Utopia.Planets.Skybox
{
    public class Stars : GameComponent
    {
        #region private variables
        private int _nbrStars = 1000;
        private VertexBuffer<VertexPositionColor> _vb;

        private HLSLStars _effectStars;
        private Matrix _world = Matrix.Identity;
        private Clock _gameClock;
        private float _visibility;
        #endregion

        #region Public Properties

        #endregion 

        public Stars(Game game, Clock gameClock)
            :base(game)
        {
            _gameClock = gameClock;
        }

        #region Private methods
        //http://www.cgafaq.info/wiki/Random_Points_On_Sphere

        private VertexPositionColor[] GenerateSpherePoints(int n, float scale)
        {
            Random rnd = new Random();
            VertexPositionColor[] result = new VertexPositionColor[n];
            int size;
            int i;
            double x, y, z, w, t;

            for( i=0; i< n; i++ ) 
            {
                z = 2.0 * rnd.NextDouble() - 1.0;
                t = 2.0 * Math.PI * rnd.NextDouble();
                w = Math.Sqrt( 1 - z*z );
                x = w * Math.Cos( t );
                y = w * Math.Sin( t );
                size = rnd.Next(0,256);
                result[i] = new VertexPositionColor(new Vector3((float)x * scale, (float)y * scale, (float)z * scale), new Color(size, 0, 0));
            }

            return result;
        }
        #endregion

        #region public methods

        public override void  Initialize()
        {
        }

        public override void LoadContent()
        {
            _effectStars = new HLSLStars(Game, @"Effects\SkyDome\Stars.hlsl", VertexPositionColor.VertexDeclaration);
            CreateBuffer();
        }

        public override void UnloadContent()
        {
            _effectStars.Dispose();
            _vb.Dispose();
        }

        public override void  DrawDepth0()
        {
            StatesRepository.ApplyStates(GameDXStates.DXStates.Rasters.Default,GameDXStates.DXStates.Blenders.Enabled);

            _effectStars.Begin();
            _effectStars.CBPerDraw.Values.World = Matrix.Transpose(_world);
            _effectStars.CBPerDraw.Values.ViewProjection = Matrix.Transpose(Game.ActivCamera.ViewProjection3D);

            //Compute Vibility !
            if (_gameClock.ClockTimeNormalized < 0.25) _visibility = 1;     // Before 06:00 am
            else
                if (_gameClock.ClockTimeNormalized >= 0.25 && _gameClock.ClockTimeNormalized < 0.30) _visibility = MathHelper.FullLerp(1, 0, 0.25, 0.30, _gameClock.ClockTimeNormalized); //Between 04:00am and 06:00am
                else
                    if (_gameClock.ClockTimeNormalized >= 0.30 && _gameClock.ClockTimeNormalized < 0.75) _visibility = 0;     //Between 06:00am and 6:00pm
                    else
                        if (_gameClock.ClockTimeNormalized >= 0.75 && _gameClock.ClockTimeNormalized < 0.85) _visibility = MathHelper.FullLerp(0, 1, 0.75, 0.85, _gameClock.ClockTimeNormalized); //Between 06:00pm and 08:00pm
                        else
                            _visibility = 1;

            _visibility = Math.Max(_visibility, Math.Min(Math.Max((float)Game.ActivCamera.WorldPosition.Y - 127, 0), 173) / 173.0f);

            _effectStars.CBPerDraw.Values.Visibility = _visibility;
            _effectStars.CBPerDraw.IsDirty = true;

            _effectStars.Apply();

            _vb.SetToDevice(0);
            Game.D3dEngine.Context.Draw(_vb.VertexCount, 0);
        }
        #endregion

        #region Private Methods
        private void CreateBuffer()
        {
            VertexPositionColor[] vertices = GenerateSpherePoints(_nbrStars, 1500);
            _vb = new VertexBuffer<VertexPositionColor>(Game, vertices.Length, VertexPositionColor.VertexDeclaration, PrimitiveTopology.PointList);
            _vb.SetData(vertices);
        }
        #endregion
    }
}
