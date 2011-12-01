using System;
using S33M3Engines.D3D;

namespace Utopia.Components
{
    /// <summary>
    /// Describes a component used to switch between the states
    /// </summary>
    public interface ISwitchComponent : IDrawableComponent
    {
        /// <summary>
        /// Occurs when screen is completely opaque. It is a time to change active components
        /// </summary>
        event EventHandler SwitchMoment;

        /// <summary>
        /// Occurs when effect is completed and can be removed
        /// </summary>
        event EventHandler EffectComplete;

        /// <summary>
        /// Begins the switch effect
        /// </summary>
        void BeginSwitch();

        /// <summary>
        /// Begins the second stage of the switch effect
        /// </summary>
        void FinishSwitch();
    }
}