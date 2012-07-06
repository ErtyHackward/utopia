using System.Collections.Generic;
using Utopia.Shared.Entities.Models;

namespace Utopia.Shared.Interfaces
{
    /// <summary>
    /// Describes a storage voxel model storage
    /// </summary>
    public interface IVoxelModelStorage
    {
        /// <summary>
        /// Indicates if the storage contains a model with name specified
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool Contains(string name);

        /// <summary>
        /// Loads a model form the storage
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        VoxelModel Load(string name);

        /// <summary>
        /// Saves a new model to the storage
        /// </summary>
        /// <param name="model"></param>
        void Save(VoxelModel model);

        /// <summary>
        /// Removes model from the storage
        /// </summary>
        /// <param name="name"></param>
        void Delete(string name);

        /// <summary>
        /// Allows to fetch all models
        /// </summary>
        /// <returns></returns>
        IEnumerable<VoxelModel> Enumerate();
    }
}
