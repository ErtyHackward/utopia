using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Invisible entity to link god-mode camera to
    /// </summary>
    public class PlayerFocusEntity : DynamicEntity
    {
        public override ushort ClassId
        {
            get { return EntityClassId.FocusEntity; }
        }
    }
}
