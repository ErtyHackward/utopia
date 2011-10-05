using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Cubes;

namespace Utopia.Shared.Chunks.Entities.Concrete
{
    public class Tree : Entity
    {
        public override EntityClassId ClassId
        {
            get { return EntityClassId.Tree; }
        }

        public override string DisplayName
        {
            get { return "Tree"; }
        }
    }
}
