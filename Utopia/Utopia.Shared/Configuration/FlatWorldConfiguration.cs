﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using ProtoBuf;
using Utopia.Shared.Entities;
using Utopia.Shared.Settings;

namespace Utopia.Shared.Configuration
{
    [ProtoContract]
    public class FlatWorldConfiguration : WorldConfiguration
    {
        private FlatProcessorParams _processorParam;

        public override WorldProcessors ConfigType
        {
            get { return WorldProcessors.Flat; }
        }

        /// <summary>
        /// Holds parameters for Utopia processor
        /// </summary>
        [Browsable(false)]
        [ProtoMember(1)]
        public FlatProcessorParams ProcessorParam
        {
            get { return _processorParam; }
            set { 
                _processorParam = value;
                _processorParam.Config = this;
            }
        }


        public FlatWorldConfiguration()
            :this(null, false)
        {
        }

        public FlatWorldConfiguration(EntityFactory factory = null, bool withHelperAssignation = false)
            : base(factory, withHelperAssignation)
        {
            ProcessorParam = new FlatProcessorParams();
        }

        protected override void CreateDefaultCubeProfiles()
        {
            CubeProfiles = new CubeProfile[255];
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
    }
}