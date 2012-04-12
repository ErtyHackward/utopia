using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX.Direct3D11;

namespace S33M3DXEngine.Main.Interfaces
{
    public interface IGameComponent
    {
        string Name { get; }
        bool IsSystemComponent { get; }
        bool IsDefferedLoadContent { get; }
        bool CatchExclusiveActions { get; set; }
        void EnableComponent();
        void DisableComponent();
        void Initialize();
        void LoadContent(DeviceContext context);
        void UnloadContent();
    }
}
