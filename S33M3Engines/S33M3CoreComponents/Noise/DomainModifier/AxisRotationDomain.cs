using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Noise.Generator;
using S33M3CoreComponents.Maths;

namespace S33M3CoreComponents.Noise.DomainModifier
{
    public class AxisRotationDomain : INoise
    {
        #region Private variable
        private INoise _source;
        private double[,] _rotmatrix = new double[3,3];
        private double _cos2d, _sin2d;
        #endregion

        #region Public variables/properties
        #endregion

        public AxisRotationDomain(INoise noise)
        {
            _source = noise;
            CreateRotationAngle();
        }

        #region Public methods

        public double Get(double x)
        {
            throw new NotImplementedException();
        }

        public double Get(double x, double y)
        {
            double nx, ny;
            nx = x * _cos2d - y * _sin2d;
            ny = y * _cos2d + x * _sin2d;
            return _source.Get(nx, ny);
        }

        public double Get(double x, double y, double z)
        {
            double nx, ny, nz;
            nx = (_rotmatrix[0,0] * x) + (_rotmatrix[1,0] * y) + (_rotmatrix[2,0] * z);
            ny = (_rotmatrix[0,1] * x) + (_rotmatrix[1,1] * y) + (_rotmatrix[2,1] * z);
            nz = (_rotmatrix[0,2] * x) + (_rotmatrix[1,2] * y) + (_rotmatrix[2,2] * z);
            return _source.Get(nx, ny, nz);
        }

        public double Get(double x, double y, double z, double w)
        {
            double nx, ny, nz;
            nx = (_rotmatrix[0, 0] * x) + (_rotmatrix[1, 0] * y) + (_rotmatrix[2, 0] * z);
            ny = (_rotmatrix[0, 1] * x) + (_rotmatrix[1, 1] * y) + (_rotmatrix[2, 1] * z);
            nz = (_rotmatrix[0, 2] * x) + (_rotmatrix[1, 2] * y) + (_rotmatrix[2, 2] * z);
            return _source.Get(nx, ny, nz, w);
        }
        #endregion

        #region Private methods
        private void CreateRotationAngle()
        {
            FastRandom rnd;
            int seed;
            if (_source is ISeedable)
            {
                seed = ((ISeedable)_source).Seed;
                rnd = new FastRandom(seed);
            }
            else rnd = new FastRandom();

            double ax, ay, az;
            double len;

            ax = rnd.NextDouble();
            ay = rnd.NextDouble();
            az = rnd.NextDouble();
            len = Math.Sqrt(ax * ax + ay * ay + az * az);
            ax /= len;
            ay /= len;
            az /= len;
            setRotationAngle(ax, ay, az, rnd.NextDouble() * Math.PI * 2.0);
            double angle = rnd.NextDouble() * Math.PI * 2.0;
            _cos2d = Math.Cos(angle);
            _sin2d = Math.Sin(angle);
        }

        private void setRotationAngle(double x, double y, double z, double angle)
        {
            _rotmatrix[0, 0] = 1 + (1 - Math.Cos(angle)) * (x * x - 1);
            _rotmatrix[1, 0] = -z * Math.Sin(angle) + (1 - Math.Cos(angle)) * x * y;
            _rotmatrix[2, 0] = y * Math.Sin(angle) + (1 - Math.Cos(angle)) * x * z;

            _rotmatrix[0, 1] = z * Math.Sin(angle) + (1 - Math.Cos(angle)) * x * y;
            _rotmatrix[1, 1] = 1 + (1 - Math.Cos(angle)) * (y * y - 1);
            _rotmatrix[2, 1] = -x * Math.Sin(angle) + (1 - Math.Cos(angle)) * y * z;

            _rotmatrix[0, 2] = -y * Math.Sin(angle) + (1 - Math.Cos(angle)) * x * z;
            _rotmatrix[1, 2] = x * Math.Sin(angle) + (1 - Math.Cos(angle)) * y * z;
            _rotmatrix[2, 2] = 1 + (1 - Math.Cos(angle)) * (z * z - 1);
        }
        #endregion


    }
}
