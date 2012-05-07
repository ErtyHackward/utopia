using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.Generator
{
    public class Gradient : INoise
    {
        #region Private variable
        private double m_gx1, m_gy1, m_gz1, m_gw1;
        private double m_gx2, m_gy2, m_gz2, m_gw2;
        private double m_x, m_y, m_z, m_w;
        private double m_vlen;
        private double _adjustX, _adjustY, _adjustZ, _adjustW;
        #endregion

        #region Public variables/properties
        public double AdjustX { get { return _adjustX; } }
        public double AdjustY { get { return _adjustY; } }
        public double AdjustZ { get { return _adjustZ; } }
        public double AdjustW { get { return _adjustW; } }
        #endregion

        /// <summary>
        /// Will create gradient Value in the range of [0;1]
        /// </summary>
        /// <param name="x1">The X value that will result an output of 0 for the X axis</param>
        /// <param name="x2">The X value that will result an output of 1 for the X axis</param>
        /// <param name="y1">The Y value that will result an output of 0 for the Y axis</param>
        /// <param name="y2">The Y value that will result an output of 1 for the Y axis</param>
        /// <param name="z1">The Z value that will result an output of 0 for the Z axis</param>
        /// <param name="z2">The Z value that will result an output of 1 for the Z axis</param>
        /// <param name="w1">The W value that will result an output of 0 for the W axis</param>
        /// <param name="w2">The W value that will result an output of 1 for the W axis</param>
        public Gradient(double x1, double x2,
                        double y1, double y2,
                        double z1 = 0, double z2 = 0,
                        double w1 = 0, double w2 = 0)
        {
            setGradient(x1, x2, y1, y2, z1, z2, w1, w2);
        }

        #region Public methods
        public double Get(double x, double y)
        {
            double dx = x - m_gx1;
            double dy = y - m_gy1;
            double dp = dx * m_x + dy * m_y;
            dp /= m_vlen;
            return dp;
        }

        public double Get(double x, double y, double z)
        {
            double dx = x - m_gx1;
            double dy = y - m_gy1;
            double dz = z - m_gz1;
            double dp = dx * m_x + dy * m_y + dz * m_z;
            dp /= m_vlen;
            return dp;
        }

        public double Get(double x, double y, double z, double w)
        {
            double dx = x - m_gx1;
            double dy = y - m_gy1;
            double dz = z - m_gz1;
            double dw = w - m_gw1;
            double dp = dx * m_x + dy * m_y + dz * m_z + dw * m_w;
            dp /= m_vlen;
            return dp;
        }
        #endregion

        #region Private methods
        private void setGradient(double x1, double x2, 
                                double y1, double y2, 
                                double z1 = 0, double z2 = 0,  
                                double w1 = 0, double w2 = 0)
        {
            m_gx1 = x1; m_gx2 = x2;
            m_gy1 = y1; m_gy2 = y2;
            m_gz1 = z1; m_gz2 = z2;
            m_gw1 = w1; m_gw2 = w2;

            m_x = x2 - x1;
            m_y = y2 - y1;
            m_z = z2 - z1;
            m_w = w2 - w1;

            _adjustX = Math.Abs(m_x);
            _adjustY = Math.Abs(m_y);
            _adjustZ = Math.Abs(m_z);
            _adjustW = Math.Abs(m_w);

            m_vlen = (m_x * m_x + m_y * m_y + m_z * m_z + m_w * m_w);
        }
        #endregion
    }
}
