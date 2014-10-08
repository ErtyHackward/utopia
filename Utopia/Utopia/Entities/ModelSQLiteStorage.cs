using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using Utopia.Shared;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Interfaces;

namespace Utopia.Entities
{
    /// <summary>
    /// Common sqlite storage
    /// </summary>
    public class ModelSQLiteStorage : SQLiteStorage, IVoxelModelStorage
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

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
                    var name = Path.GetFileNameWithoutExtension(file);
                    if (name == null) continue;
                    name = name.Replace('\'', ' ');
                    using (var reader = Query("SELECT updated FROM models WHERE id ='{0}'", name ))
                    {
                        if (reader.Read() && reader.GetDateTime(0) >= File.GetLastWriteTimeUtc(file))
                            continue;
                    }

                    // import the model
                    Delete(name);
                    Save(VoxelModel.LoadFromFile(file));
                }
            }
            catch (Exception x)
            {
                Logger.Error("Models import failed. {0}", x.Message);
            }
        }

        /// <summary>
        /// Returns database creation query 
        /// </summary>
        /// <returns></returns>
        protected override string CreateDataBase()
        {
            return @"CREATE TABLE [models] ([id] varchar(120) PRIMARY KEY NOT NULL, [updated] datetime NULL, [data] blob NOT NULL);";
        }

        /// <summary>
        /// Indicates if the storage contains a model with hash specified
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Contains(string name)
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
        public VoxelModel Load(string name)
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
        public void Save(VoxelModel model)
        {
            CheckName(model.Name);
            using (var ms= new MemoryStream() )
            {
                Serializer.Serialize(ms, model);
                var bytes = ms.ToArray();

                if (Contains(model.Name))
                    Delete(model.Name);

                InsertBlob(string.Format("INSERT INTO models (id, updated, data) VALUES ('{0}', datetime('now'), @blob)", model.Name), bytes);
            }
        }

        /// <summary>
        /// Removes model from the storage
        /// </summary>
        /// <param name="name"></param>
        public void Delete(string name)
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
        public IEnumerable<VoxelModel> Enumerate()
        {
            using (var reader = Query("SELECT data FROM models"))
            {
                while (reader.Read())
                {
                    VoxelModel model = null;

                    try
                    {
                        model = ((byte[])reader.GetValue(0)).Deserialize<VoxelModel>();
                    }
                    catch (InvalidDataException x)
                    {
                        Logger.Error("Unable to load model: " + x.Message);
                    }

                    if (model != null)
                        yield return model;
                }
            }
        }
    }
}
