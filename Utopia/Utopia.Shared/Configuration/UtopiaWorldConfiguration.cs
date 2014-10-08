using System.ComponentModel;
using ProtoBuf;
using Utopia.Shared.Entities;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Configuration
{
    /// <summary>
    /// World configuration for utopia world generator
    /// </summary>
    [ProtoContract]
    public class UtopiaWorldConfiguration : WorldConfiguration
    {
        private int _worldHeight;
        private UtopiaProcessorParams _processorParam;

        public override WorldProcessors ConfigType
        {
            get { return WorldProcessors.Utopia; }
        }

        /// <summary>
        /// Holds parameters for Utopia processor
        /// </summary>
        [Browsable(false)]
        [ProtoMember(1)]
        public UtopiaProcessorParams ProcessorParam
        {
            get { return _processorParam; }
            set { 
                _processorParam = value;
                _processorParam.Config = this;
            }
        }

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
                    if (ProcessorParam != null && ProcessorParam.WorldGeneratedHeight <= value) 
                        return;
                    
                    _worldHeight = value;
                }
            }
        }

        public UtopiaWorldConfiguration()
            :this(null, false)
        {
        }

        public UtopiaWorldConfiguration(EntityFactory factory = null, bool withHelperAssignation = false)
            : base(factory, withHelperAssignation)
        {
            ProcessorParam = new UtopiaProcessorParams();
        }

        protected override void CreateDefaultCubeProfiles()
        {
            BlockProfiles = new BlockProfile[255];
            int id = 0;
            //Air Block
            BlockProfiles[id] = (new BlockProfile()
            {
                Name = "Air",
                Description = "A cube",
                Id = 0,
                Tex_Top = new TextureData(),
                Tex_Bottom  = new TextureData(),
                Tex_Back =  new TextureData(),
                Tex_Front = new TextureData(),
                Tex_Left =  new TextureData(),
                Tex_Right = new TextureData(),
                IsSeeThrough = true,
                CubeFamilly = Enums.enuCubeFamilly.Solid,
                Friction = 0.25f,
                IsSystemCube = true
            });

            foreach (var processorInjectedCube in ProcessorParam.InjectDefaultCubeProfiles())
            {
                id++;
                BlockProfiles[id] = processorInjectedCube;
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
    }
}
