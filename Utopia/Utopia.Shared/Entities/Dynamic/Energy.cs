using ProtoBuf;
using System;
using System.ComponentModel;
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

        [ProtoMember(3)]
        [Browsable(false)]
        public float CurrentValue
        {
            get { return _currentValue; }
            set
            {
                if (value < 0) value = 0; 
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
                if (_maxValue == 0) return 0;
                return _currentValue / _maxValue;
            }
        }

        public Energy(Energy energy): this()
        {
            MaxValue = energy.MaxValue;
            CurrentValue = energy.CurrentValue;
        }

        public Energy()
        {
            isNetworkPropagated = true;
        }

        //Will raise a change Notification Event to subscriber
        private void RaiseChangeNotification(float changedAmount)
        {
            var handler = ValueChanged;
            if (handler != null) 
                handler(this, new EnergyChangedEventArgs { 
                    EnergyChanged = this, 
                    ValueChangedAmount = changedAmount 
                });
        }

    }
}
