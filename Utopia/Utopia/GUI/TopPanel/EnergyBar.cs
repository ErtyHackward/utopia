using S33M3CoreComponents.GUI.Nuclex.Controls;
using S33M3CoreComponents.GUI.Nuclex.Controls.Arcade;
using S33M3CoreComponents.Maths;
using S33M3DXEngine.Buffers;
using S33M3DXEngine.Main;
using S33M3Resources.Structs;
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
        private FTSValue<float> _value = new FTSValue<float>();
        private float _previousOldValue = -1;
        private Stopwatch _startTime = new Stopwatch();

        //Graphics properties
        public long TimeFromOldToNewInMS { get; set; }

        /// <summary>
        /// The current value of the Bar, in [0 to 1] range. 1 = Full, 0 = Empty
        /// </summary>
        public float Value
        {
            get { return _value.Value; }
            set
            {
                if (_value.Value == value) return;
                _value.Value = value;
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
                if (Math.Abs(_newValue - Value) < 0.01 && !_startTime.IsRunning) //Less than 1 percent of difference, assign directly !
                {
                    Value = _newValue;
                }
                else
                {
                    if (_previousOldValue == -1)
                    {
                        _previousOldValue = Value;
                        _startTime.Restart();
                    }
                }
            }
        }

        public void Update(GameTime timeSpend)
        {
            _value.BackUpValue();

            if (_startTime.IsRunning)
            {
                var factor = (_startTime.ElapsedMilliseconds / (float)TimeFromOldToNewInMS);
                if (factor > 1.0f)
                {
                    _previousOldValue = -1;
                    factor = 1.0f;
                    _startTime.Stop();
                }

                Value = MathHelper.Lerp(_previousOldValue, _newValue, factor);
            }
        }

        public void VTSUpdate(double interpolationHd, float interpolationLd, float elapsedTime)
        {
            //Interpolating bar movements changes
           _value.ValueInterp = MathHelper.Lerp(_value.ValuePrev, _value.Value, interpolationLd);

           this.Bounds.Size.X.Fraction = _value.ValueInterp;
        }

    }
}
