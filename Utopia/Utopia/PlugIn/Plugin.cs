using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using Utopia.Planets.Terran;
using S33M3Engines.Struct;
using Utopia.Planets.Terran.Cube;
using System.ComponentModel.Composition;
using System.IO;
using System.ComponentModel.Composition.Hosting;
using System.Reflection;
using Utopia.Univers;
using Utopia.Shared.Structs;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.PlugIn
{
    public class Plugins
    {
        [ImportMany(typeof(IUniversePlugin))]
        public IUniversePlugin[] WorldPlugins;

        public void LoadPlugins()
        {
            if(!Directory.Exists("PlugIns")) Directory.CreateDirectory("PlugIns");

            AggregateCatalog catalog = new AggregateCatalog();
            catalog.Catalogs.Add(new DirectoryCatalog(@".\PlugIns"));
            CompositionContainer container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }

    public static class WorldPlugins
    {
        public static Plugins Plugins;

        public static void Initialize(Plugins plugins)
        {
            Plugins = plugins;
        }

        public static void Initialize(Universe universe)
        {
            for (int i = 0; i < WorldPlugins.Plugins.WorldPlugins.Length; i++)
            {
                WorldPlugins.Plugins.WorldPlugins[i].Initialize(universe);
            }
        }

        public static void LoadContent()
        {
            for (int i = 0; i < WorldPlugins.Plugins.WorldPlugins.Length; i++)
            {
                WorldPlugins.Plugins.WorldPlugins[i].LoadContent();
            }
        }

        public static void UnloadContent()
        {
            for (int i = 0; i < WorldPlugins.Plugins.WorldPlugins.Length; i++)
            {
                WorldPlugins.Plugins.WorldPlugins[i].UnloadContent();
            }
        }

        public static void Update(ref GameTime TimeSpend)
        {
            for (int i = 0; i < WorldPlugins.Plugins.WorldPlugins.Length; i++)
            {
                WorldPlugins.Plugins.WorldPlugins[i].Update(ref TimeSpend);
            }
        }

        public static void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
            for (int i = 0; i < WorldPlugins.Plugins.WorldPlugins.Length; i++)
            {
                WorldPlugins.Plugins.WorldPlugins[i].Interpolation(ref interpolation_hd, ref interpolation_ld);
            }
        }

        public static void Draw()
        {
            for (int i = 0; i < WorldPlugins.Plugins.WorldPlugins.Length; i++)
            {
                WorldPlugins.Plugins.WorldPlugins[i].Draw();
            }
        }

    }

    public interface IUniversePlugin
    {
        string PluginName { get; }
        string PluginVersion { get; }

        void Initialize(Universe universe);
        void Update(ref GameTime TimeSpend);
        void Draw();
        void Interpolation(ref double interpolation_hd, ref float interpolation_ld);
        void LoadContent();
        void UnloadContent();

        bool EntityBlockReplaced(ref Location3<int> cubeCoordinates, ref TerraCube newCube);
    }
}
