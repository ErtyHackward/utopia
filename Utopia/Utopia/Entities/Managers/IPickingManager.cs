using SharpDX;

namespace Utopia.Entities.Managers
{
    public interface IPickingManager
    {
        /// <summary>
        /// Checks nearby entities intersection with the pickingRay
        /// </summary>
        /// <param name="pickingRay">Ray to check intersection</param>
        /// <returns></returns>
        EntityPickResult CheckEntityPicking(Ray pickingRay);
    }
}