using System.Collections.Generic;
using System.IO;
using Utopia.Shared;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Entities
{
    /// <summary>
    /// Common sqlite storage
    /// </summary>
    public class ModelSQLiteStorage : SQLiteStorage, IVoxelModelStorage
    {
        public ModelSQLiteStorage(string fileName) : base(fileName, false)
        {

        }

        /// <summary>
        /// Returns database creation query 
        /// </summary>
        /// <returns></returns>
        protected override string CreateDataBase()
        {
            return @"CREATE TABLE [models] ([id] integer PRIMARY KEY NOT NULL, [data] blob NOT NULL);";
        }

        /// <summary>
        /// Indicates if the storage contains a model with hash specified
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        bool IVoxelModelStorage.Contains(Md5Hash hash)
        {
            using (var reader = Query(string.Format("SELECT id FROM models WHERE id = {0}", hash.GetHashCode())))
            {
                return reader.HasRows;
            }
        }

        /// <summary>
        /// Loads a model form the storage
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        VoxelModel IVoxelModelStorage.Load(Md5Hash hash)
        {
            using (var reader = Query(string.Format("SELECT data FROM models WHERE id={0}", hash.GetHashCode())))
            {
                reader.Read();
                var bytes = (byte[])reader.GetValue(0);
                return bytes.Deserialize<VoxelModel>();
            }
        }

        /// <summary>
        /// Saves a new model to the storage
        /// </summary>
        /// <param name="model"></param>
        void IVoxelModelStorage.Save(VoxelModel model)
        {
            var bytes = model.Serialize();

            InsertBlob(string.Format("INSERT INTO models (id,data) VALUES ({0}, @blob)", model.Hash.GetHashCode()), bytes);
        }

        /// <summary>
        /// Removes model from the storage
        /// </summary>
        /// <param name="hash"></param>
        void IVoxelModelStorage.Delete(Md5Hash hash)
        {
            Execute(string.Format("DELETE FROM models WHERE id = {0}", hash.GetHashCode()));
        }

        /// <summary>
        /// Allows to fetch all models
        /// </summary>
        /// <returns></returns>
        IEnumerable<VoxelModel> IVoxelModelStorage.Enumerate()
        {
            using (var reader = Query("SELECT data FROM models"))
            {
                while (reader.Read())
                {
                    yield return ((byte[])reader.GetValue(0)).Deserialize<VoxelModel>();
                }
            }
        }
    }
}
