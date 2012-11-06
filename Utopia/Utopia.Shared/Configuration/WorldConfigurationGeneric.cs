using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utopia.Shared.Entities;
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

        public WorldConfiguration(EntityFactory factory = null, bool withDefaultValueCreation = false, bool withHelperAssignation = false)
            : base(factory, withDefaultValueCreation, withHelperAssignation)
        {
            ProcessorParam = new T();
            ProcessorParam.Config = this;

            if (withDefaultValueCreation) CreateDefaultUtopiaProcessorParam();
        }

        #region Public Methods
        #endregion

        #region Private Methods
        //Definition of all default Utopia processor params
        private void CreateDefaultUtopiaProcessorParam()
        {
            ProcessorParam.CreateDefaultConfiguration();
        }
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
