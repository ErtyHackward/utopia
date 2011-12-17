using System.Collections.Generic;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Structs;

namespace Utopia.Shared.Interfaces
{
    /// <summary>
    /// Describes a storage voxel model storage
    /// </summary>
    public interface IVoxelModelStorage
    {
        /// <summary>
        /// Indicates if the storage contains a model with hash specified
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        bool Contains(Md5Hash hash);

        /// <summary>
        /// Loads a model form the storage
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        VoxelModel Load(Md5Hash hash);

        /// <summary>
        /// Saves a new model to the storage
        /// </summary>
        /// <param name="model"></param>
        void Save(VoxelModel model);

        /// <summary>
        /// Removes model from the storage
        /// </summary>
        /// <param name="hash"></param>
        void Delete(Md5Hash hash);

        /// <summary>
        /// Allows to fetch all models
        /// </summary>
        /// <returns></returns>
        IEnumerable<VoxelModel> Enumerate();
    }
}
