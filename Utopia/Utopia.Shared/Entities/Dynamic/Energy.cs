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

        private float _maxValue;
        private float _minValue;
        private float _currentValue;

        [Browsable(false)]
        public bool isNetworkPropagated { get; set; }

        [ProtoMember(1)]
        public float MaxValue
        {
            get { return _maxValue; }
            set
            {
                if (_maxValue != value)
                {
                    _maxValue = value;
                    RaiseChangeNotification(0.0f); 
                }
            }
        }

        [ProtoMember(2)]
        public float MinValue
        {
            get { return _minValue; }
            set
            {
                if (_minValue != value)
                {
                    _minValue = value;
                    RaiseChangeNotification(0.0f); 
                }
            }
        }

        [ProtoMember(3)]
        [Browsable(false)]
        public float CurrentValue
        {
            get { return _currentValue; }
            set
            {
                if (value < _minValue) value = _minValue; 
                if (value > _maxValue) value = _maxValue;
                if (value != _currentValue)
                {
                    var amount = value - _currentValue;
                    _currentValue = value;
                    RaiseChangeNotification(amount);
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

        public Energy(Energy energy): this()
        {
            this.MaxValue = energy.MaxValue;
            this.MinValue = energy.MinValue;
            this.CurrentValue = energy.CurrentValue;
        }

        public Energy()
        {
            isNetworkPropagated = true;
        }

        //Will raise a change Notification Event to subscriber
        private void RaiseChangeNotification(float changedAmount)
        {
            if (ValueChanged != null) ValueChanged(this, new EnergyChangedEventArgs() { EnergyChanged = this, ValueChangedAmount = changedAmount });
        }

    }
}
