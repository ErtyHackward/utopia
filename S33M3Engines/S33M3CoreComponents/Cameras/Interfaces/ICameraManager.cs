using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Cameras.Interfaces;

namespace S33M3_CoreComponents.Cameras.Interfaces
{
    public delegate void CameraChange(ICamera newCamera);
    public interface ICameraManager
    {
        ICamera ActiveBaseCamera { get; }
        event CameraChange ActiveCamera_Changed;
    }
}
