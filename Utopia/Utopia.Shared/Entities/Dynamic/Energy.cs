using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Events;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Role-playing players and NPC attributes. All parameters can be in range [1; 10]
    /// </summary>
    [TypeConverter(typeof(EnergyTypeConverter))]
    [ProtoContract]
    public partial class Energy
    {
        public event EventHandler<EnergyChangedEventArgs> ValueChanged;

        private int _maxValue;
        private int _minValue;
        private int _currentValue;

        [ProtoMember(1)]
        public int MaxValue
        {
            get { return _maxValue; }
            set { if(_maxValue != value) _maxValue = value;  }
        }

        [ProtoMember(2)]
        public int MinValue
        {
            get { return _minValue; }
            set { if (_minValue != value) _minValue = value; }
        }

        [ProtoMember(3)]
        [Browsable(false)]
        public int CurrentValue
        {
            get { return _currentValue; }
            set
            {
                if (value < _minValue) value = _minValue; 
                if (value > _maxValue) value = _maxValue;
                if (value != _currentValue)
                {
                    _currentValue = value;
                }
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

        public Energy(Energy energy)
        {
            this.MaxValue = energy.MaxValue;
            this.MinValue = energy.MinValue;
            this.CurrentValue = energy.CurrentValue;
        }

        public Energy()
        {
        }

        //Will raise a change Notification Event to subscriber
        public void RaiseChangeNotification()
        {
            if (ValueChanged != null) ValueChanged(this, new EnergyChangedEventArgs() { EnergyChanged = this });
        }

    }
}
