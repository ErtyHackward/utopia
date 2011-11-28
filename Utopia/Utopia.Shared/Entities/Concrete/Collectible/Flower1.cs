using System.IO;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;
using System;
using Utopia.Shared.Interfaces;
using S33M3Engines.Shared.Math;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Flower1 : SpriteItem, ITool, IBlockLinkedEntity
    {
        #region Private properties
        private ILandscapeManager2D _landscapeManager;
        #endregion

        #region Public properties/variables
        public override bool IsPickable { get { return true; } }
        public override bool IsPlayerCollidable { get { return false; } }
        public Vector3I LinkedCube { get; set; }

        public override ushort ClassId
        {
            get { return EntityClassId.Flower1; }
        }

        public override string DisplayName
        {
            get { return "Flower"; }
        }

        public override string Description
        {
            get { return "Flower description"; }
        }

        public override int MaxStackSize
        {
            get
            {
                return 20;
            }
        }

        #endregion
        public Flower1()
        {
            Type = EntityType.Static;
            UniqueName = DisplayName;
            Format = SpriteFormat.Billboard;
            Size = new Vector3(0.3f, 0.3f, 0.3f);
        }

        public Flower1(ILandscapeManager2D landscapeManager)
            :this()
        {
            _landscapeManager = landscapeManager;
        }

        #region Public methods
        // we need to override save and load!
        public override void Load(BinaryReader reader)
        {
            // first we need to load base information
            base.Load(reader);
            Vector3I linkedCube = new Vector3I();
            linkedCube.X = reader.ReadInt32();
            linkedCube.Y = reader.ReadInt32();
            linkedCube.Z = reader.ReadInt32();
            LinkedCube = linkedCube;
        }

        public override void Save(BinaryWriter writer)
        {
            // first we need to save base information
            base.Save(writer);
            writer.Write(LinkedCube.X);
            writer.Write(LinkedCube.Y);
            writer.Write(LinkedCube.Z);
        }

        public IToolImpact Use(IDynamicEntity owner, byte useMode, bool runOnServer)
        {
            var impact = new ToolImpact { Success = false };

            if (useMode == 1)
            {

                if (owner.EntityState.IsBlockPicked == true)
                {
                    IChunkLayout2D chunk = _landscapeManager.GetChunk(owner.EntityState.PickedBlockPosition);

                    //Create a new version of the Grass, and put it into the world
                    var cubeEntity = (IItem)EntityFactory.Instance.CreateEntity(this.ClassId);
                    cubeEntity.Position = new Vector3D(owner.EntityState.PickedBlockPosition.X + 0.5f, owner.EntityState.PickedBlockPosition.Y + 1f, owner.EntityState.PickedBlockPosition.Z + 0.5f);

                    chunk.Entities.Add(cubeEntity);

                    impact.Success = true;
                }
            }
            return impact;
        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private methods
        #endregion

    }
}
