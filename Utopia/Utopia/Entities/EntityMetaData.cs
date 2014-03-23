using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Interfaces;

namespace Utopia.Entities
{
    public class EntityMetaData
    {
        public IEntity Entity;
        
        //Particule MetaData Information.
        public StaticEntityParticule Particule;
        public DateTime EntityLastEmitTime;
    }
}
