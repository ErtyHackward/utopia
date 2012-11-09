using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utopia.Shared.Entities;
using Utopia.Shared.Settings;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Configuration
{
    //World Configuration subtyped with a processor Type
    public class WorldConfiguration<T> : WorldConfiguration, IBinaryStorable where T : IBinaryStorable, IProcessorParams, new()
    {
        #region Private Variables
        private int _worldHeight;
        #endregion

        #region Public Properties
        /// <summary>
        /// Holds parameters for Utopia processor
        /// </summary>
        [Browsable(false)]
        public T ProcessorParam { get; set; }

        /// <summary>
        /// World Height
        /// </summary>
        public override int WorldHeight
        {
            get { return _worldHeight; }
            set
            {
                _worldHeight = value;

                if (value >= 128 && value <= 256)
                {
                    UtopiaProcessorParams utopiaParam = ProcessorParam as UtopiaProcessorParams;
                    if(utopiaParam != null)
                    {
                        if (utopiaParam.WorldGeneratedHeight <= value) return;
                    }
                    _worldHeight = value;
                }
            }
        }
        #endregion

        public WorldConfiguration()
            :this(null, false)
        {
        }

        public WorldConfiguration(EntityFactory factory = null, bool withHelperAssignation = false)
            : base(factory, withHelperAssignation)
        {
            ProcessorParam = new T();
            ProcessorParam.Config = this;
        }

        #region Public Methods
        protected override void CreateDefaultCubeProfiles()
        {
            int id = 0;
            //Air Block
            CubeProfiles[id] = (new CubeProfile()
            {
                Name = "Air",
                Description = "A cube",
                Id = 0,
                Tex_Top = 255,
                Tex_Bottom = 255,
                Tex_Back = 255,
                Tex_Front = 255,
                Tex_Left = 255,
                Tex_Right = 255,
                IsSeeThrough = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = true
            });

            foreach (var processorInjectedCube in ProcessorParam.InjectDefaultCubeProfiles())
            {
                id++;
                CubeProfiles[id] = processorInjectedCube;
            }

            base.CreateDefaultCubeProfiles();
        }

        protected override void CreateDefaultEntities()
        {
            foreach (var processorInjectedEntities in ProcessorParam.InjectDefaultEntities())
            {
                AddNewEntity(processorInjectedEntities);
            }

            base.CreateDefaultEntities();
        }
        #endregion

        #region Private Methods
        #endregion

        public override void Load(BinaryReader reader)
        {
            base.Load(reader);
            ProcessorParam.Load(reader);
        }

        public override void Save(BinaryWriter writer)
        {
            base.Save(writer);
            ProcessorParam.Save(writer);
        }

    }
}
