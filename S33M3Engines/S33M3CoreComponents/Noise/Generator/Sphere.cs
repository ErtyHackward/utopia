using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.Generator
{
    public class Sphere : INoise
    {
        #region Private Variables
        private double _centerX;
        private double _centerY;
        private double _centerZ;
        private double _centerW;
        private double _radius;
        #endregion

        #region Public Properties
        #endregion

        public Sphere(double radius, double centerX, double centerY, double centerZ = 0.0, double centerW = 0.0)
        {
            _radius = radius;
            _centerX = centerX;
            _centerY = centerY;
            _centerZ = centerZ;
            _centerW = centerW;
        }

        #region Public Methods
        public double Get(double x, double y)
        {
            double dx = x - _centerX;
            double dy = y - _centerY;
            double len = Math.Sqrt(dx * dx + dy * dy);
            double i = (_radius - len) / _radius;
            if (i < 0) i = 0;
            if (i > 1) i = 1;

            return i;
        }

        public double Get(double x, double y, double z)
        {
            double dx = x - _centerX;
            double dy = y - _centerY;
            double dz = z - _centerZ;
            double len = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            double i = (_radius - len) / _radius;
            if (i < 0) i = 0;
            if (i > 1) i = 1;

            return i;
        }

        public double Get(double x, double y, double z, double w)
        {
            double dx = x - _centerX;
            double dy = y - _centerY;
            double dz = z - _centerZ;
            double dw = w - _centerW;
            double len = Math.Sqrt(dx * dx + dy * dy + dz * dz + dw * dw);
            double i = (_radius - len) / _radius;
            if (i < 0) i = 0;
            if (i > 1) i = 1;

            return i;
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
