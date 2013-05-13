using System.Collections.Generic;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Interfaces
{
    /// <summary>
    /// Represents dynamic entity manager base, should be used both in client and server
    /// </summary>
    public interface IDynamicEntityManager
    {
        /// <summary>
        /// Allows to enumerate all existing entities around some point
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        IEnumerable<IDynamicEntity> EnumerateAround(Vector3 pos);

        /// <summary>
        /// Returns entity by a link or null
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        IDynamicEntity FindEntity(EntityLink link);
    }
}
