using System.IO;
using SharpDX;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Structs;
using System;
using Utopia.Shared.Interfaces;
using S33M3Engines.Shared.Math;
using Utopia.Shared.Chunks;

namespace Utopia.Shared.Entities.Concrete.Collectible
{
    public class Grass : SpriteItem, IGrowEntity, ITool, IBlockLinkedEntity
    {
        #region Private properties
        private ILandscapeManager2D _landscapeManager;
        private byte _growPhase;
        #endregion

        #region Public properties/variables
        public override bool IsPickable { get { return true; } }
        public override bool IsPlayerCollidable { get { return false; } }
        public Vector3I LinkedCube { get; set; }

        public byte GrowPhase
        {
            get { return _growPhase; }
            set { _growPhase = value; GrawPhaseChanged(); }
        }

        public override string StackType
        {
            get { return this.GetType().Name + GrowPhase; }
        }

        public override ushort ClassId
        {
            get { return EntityClassId.Grass; }
        }

        public override string DisplayName
        {
            get { return "Grass"; }
        }

        public override string Description
        {
            get { return "Juicy green grass. Collect, dry and smoke!"; }
        }

        public override int MaxStackSize
        {
            get
            {
                return 20;
            }
        }

        #endregion
        public Grass()
        {
            Type = EntityType.Static;
            UniqueName = DisplayName;
            GrowPhase = 0; //Set Default Grow Phase
        }

        public Grass(ILandscapeManager2D landscapeManager)
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
            GrowPhase = reader.ReadByte();
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
            writer.Write(GrowPhase);
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
                    ((IGrowEntity)cubeEntity).GrowPhase = ((IGrowEntity)this).GrowPhase;

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
        private void GrawPhaseChanged()
        {
            switch (GrowPhase)
            {
                default:
                        Size = new Vector3(0.7f, 0.7f, 0.7f);
                        Format = SpriteFormat.Triangle;
                    break;
            }
        }
        #endregion

    }
}
