using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3Resources.VertexFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Utopia.GUI.TopPanel
{
    public class EnergyBar : PanelControl
    {
        private float _newValue;
        private float _value;
        private float _previousOldValue;
        private Stopwatch _startTime = new Stopwatch();

        //Graphics properties
        public long TimeFromOldToNewInMS { get; set; }

        /// <summary>
        /// The current value of the Bar, in [0 to 1] range. 1 = Full, 0 = Empty
        /// </summary>
        public float Value
        {
            get { return _value; }
            set
            {
                if (_value == value) return;
                _value = value;
                this.Bounds.Size.X.Fraction = _value;
            }
        }

        /// <summary>
        /// The new value we are aiming to
        /// </summary>
        public float NewValue
        {
            get { return _newValue; }
            set
            {
                if (value == _newValue) return;
                _newValue = value; 
                _previousOldValue = Value; _startTime.Restart();
            }
        }

        public void Update(GameTime timeSpend)
        {
            if (_startTime.IsRunning)
            {
                var factor = (_startTime.ElapsedMilliseconds / (float)TimeFromOldToNewInMS);
                if (factor > 1.0f)
                {
                    factor = 1.0f;
                    _startTime.Stop();
                }

                Value = MathHelper.Lerp(_previousOldValue, _newValue, factor);
            }
        }

    }
}
