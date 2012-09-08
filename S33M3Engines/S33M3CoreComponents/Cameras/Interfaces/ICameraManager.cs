using System;

namespace S33M3CoreComponents.Cameras.Interfaces
{
    /// <summary>
    /// Provides possibility to catch camera change event or get active camera
    /// </summary>
    public interface ICameraManager
    {
        /// <summary>
        /// Gets currently active camera
        /// </summary>
        ICamera ActiveBaseCamera { get; }

        /// <summary>
        /// Occurs when current active camera was changed
        /// </summary>
        event EventHandler<CameraChangedEventArgs> ActiveCameraChanged;
    }
}
