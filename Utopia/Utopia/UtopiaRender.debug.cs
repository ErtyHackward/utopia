using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.InputHandler;
using S33M3Engines.InputHandler.KeyboardHelper;
using System.Windows.Forms;
using SharpDX.Direct3D11;
using S33M3Engines.Cameras;
using UtopiaContent.ModelComp;
using SharpDX;
using S33M3Engines.Struct;
using S33M3Engines.Maths;
using S33M3Engines.D3D.Effects.Basics;
using Utopia.GameDXStates;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;
using Utopia.Shared;
using Utopia.Settings;
using Utopia.Shared.Config;
using Utopia.Worlds;
using Utopia.Worlds.GameClocks;
using Utopia.Worlds.SkyDomes;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Weather;
using Utopia.Worlds.SkyDomes.SharedComp;
using Ninject;
using Ninject.Parameters;
using S33M3Engines.WorldFocus;
using S33M3Engines.GameStates;
using S33M3Engines;
using Size = System.Drawing.Size;
using S33M3Engines.Threading;
using Utopia.Entities.Living;
using Utopia.Shared.Interfaces;
using Utopia.Shared.World;
using Utopia.Worlds.Cubes;
using Utopia.Entities;
using Utopia.Worlds.Chunks.ChunkLighting;

namespace Utopia
{
    public partial class UtopiaRender : Game
    {
        //Init phase used for testing purpose
        private void DebugInit(IKernel IoCContainer)
        {
           
        }
    }
}
