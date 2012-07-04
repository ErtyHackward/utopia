using System;
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
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public ModelSQLiteStorage(string fileName) : base(fileName, false)
        {

        }

        /// <summary>
        /// Load new models from folder
        /// </summary>
        /// <param name="folderPath"></param>
        public void ImportFromPath(string folderPath)
        {
            try
            {
                foreach (var file in Directory.GetFiles(folderPath, "*.uvm"))
                {
                    using (var reader = Query("SELECT id FROM models WHERE id ='{0}'", Path.GetFileNameWithoutExtension(file)))
                    {
                        if (reader.HasRows)
                            continue;
                    }

                    // import the model
                    ((IVoxelModelStorage)this).Save(VoxelModel.LoadFromFile(file));
                }
            }
            catch (Exception x)
            {
                _logger.Error("Models import failed. {0}", x.Message);
            }
        }

        /// <summary>
        /// Returns database creation query 
        /// </summary>
        /// <returns></returns>
        protected override string CreateDataBase()
        {
            return @"CREATE TABLE [models] ([id] varchar(120) PRIMARY KEY NOT NULL, [data] blob NOT NULL);";
        }

        /// <summary>
        /// Indicates if the storage contains a model with hash specified
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        bool IVoxelModelStorage.Contains(string name)
        {
            CheckName(name);
            using (var reader = Query(string.Format("SELECT id FROM models WHERE id = '{0}'", name)))
            {
                return reader.HasRows;
            }
        }

        /// <summary>
        /// Loads a model form the storage
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        VoxelModel IVoxelModelStorage.Load(string name)
        {
            CheckName(name);
            using (var reader = Query(string.Format("SELECT data FROM models WHERE id = '{0}'", name)))
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
            CheckName(model.Name);
            var bytes = model.Serialize();

            InsertBlob(string.Format("INSERT INTO models (id,data) VALUES ('{0}', @blob)", model.Name), bytes);
        }

        /// <summary>
        /// Removes model from the storage
        /// </summary>
        /// <param name="name"></param>
        void IVoxelModelStorage.Delete(string name)
        {
            CheckName(name);
            Execute(string.Format("DELETE FROM models WHERE id = '{0}'", name));
        }

        private void CheckName(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name", "Null or empty names are not allowed");

            if (name.Contains("'"))
                throw new FormatException("Model name could not contan a \"'\" symbol");
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
