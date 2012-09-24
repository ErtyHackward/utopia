using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Shared
{
    /// <summary>
    /// Contains all gameplay parameters of the realm
    /// Holds possible entities types, their names, world generator settings, defines everything
    /// Allows to save and load the realm configuration
    /// </summary>
    public class RealmConfiguration : IBinaryStorable
    {
        /// <summary>
        /// Realm format version
        /// </summary>
        private const int RealmFormat = 1;

        /// <summary>
        /// General realm display name
        /// </summary>
        public string RealmName { get; set; }

        /// <summary>
        /// Author name of the realm
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Datetime of the moment of creation
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Datetime of the last update
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Allows to comapre the realms equality
        /// </summary>
        public Md5Hash IntegrityHash { get; set; }

        /// <summary>
        /// Defines realm world processor
        /// </summary>
        public WorldProcessor WorldProcessor { get; set; }

        /// <summary>
        /// Holds examples of entities of all types in the realm
        /// </summary>
        public List<IEntity> EntityExamples { get; set; }
        
        public void Save(BinaryWriter writer)
        {
            writer.Write(RealmFormat);
            writer.Write(RealmName);
            writer.Write(Author);
            writer.Write(CreatedAt.ToBinary());
            writer.Write(UpdatedAt.ToBinary());
            writer.Write((byte)WorldProcessor);

            // todo: complete save and load

        }

        public void Load(BinaryReader reader)
        {
            
        }
    }

    /// <summary>
    /// Defines world processor possible types
    /// </summary>
    public enum WorldProcessor : byte
    {
        Flat,
        Utopia,
        Plan
    }
}
