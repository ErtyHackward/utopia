using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace S33M3CoreComponents.Noise.Generator
{
    public class Constant : INoise
    {
        #region Private Variables 
        private double _cstValue;
	    #endregion
	
	    #region Public Properties
	    #endregion
        public Constant(double value)
        {
            _cstValue = value;
        }
	
	    #region Public Methods
        public double Get(double x)
        {
            return _cstValue;
        }

        public double Get(double x, double y)
        {
            return _cstValue;
        }

        public double Get(double x, double y, double z)
        {
            return _cstValue;
        }

        public double Get(double x, double y, double z, double w)
        {
            return _cstValue;
        }
	    #endregion
	
	    #region Private Methods
	    #endregion	        

    }
}
