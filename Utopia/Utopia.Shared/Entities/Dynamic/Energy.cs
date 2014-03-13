using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Role-playing players and NPC attributes. All parameters can be in range [1; 10]
    /// </summary>
    [TypeConverter(typeof(EnergyTypeConverter))]
    [ProtoContract]
    public partial class Energy
    {
        private int _maxValue;
        private int _minValue;
        private int _currentValue;

        [ProtoMember(1)]
        public int MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }

        [ProtoMember(2)]
        public int MinValue
        {
            get { return _minValue; }
            set { _minValue = value; }
        }

        [ProtoMember(3)]
        [Browsable(false)]
        public int CurrentValue
        {
            get { return _currentValue; }
            set
            {
                if (value < _minValue) { _currentValue = _minValue; return; }
                if (value > _maxValue) { _currentValue = _maxValue; return; }
                _currentValue = value;
            }
        }

        [Browsable(false)]
        public float CurrentAsPercent
        {
            get
            {
                if (_maxValue - _minValue == 0) return 0;
                return (_currentValue - _minValue) / (_maxValue - _minValue);
            }
        }


    }
}
