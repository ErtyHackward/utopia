using ProtoBuf;
using System;
using System.ComponentModel;
using Utopia.Shared.Entities.Events;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Universal character variable like health, stamina, oxygen etc
    /// </summary>
    [TypeConverter(typeof(EnergyTypeConverter))]
    [ProtoContract]
    public partial class Energy
    {
        public event EventHandler<EnergyChangedEventArgs> ValueChanged;

        private float _maxValue;
        private float _currentValue;

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

        [ProtoMember(4)]
        [Browsable(false)]
        public uint EntityOwnerId { get; set; }

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
        }

        //Will raise a change Notification Event to subscriber
        private void RaiseChangeNotification(float changedAmount)
        {
            var handler = ValueChanged;
            if (handler != null)
                handler(this, new EnergyChangedEventArgs { 
                    EntityOwner = EntityOwnerId,
                    EnergyChanged = this, 
                    ValueChangedAmount = changedAmount 
                });
        }

    }
}
