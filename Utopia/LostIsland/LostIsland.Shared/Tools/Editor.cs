using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;

namespace LostIsland.Shared.Tools
{

    /// <summary>
    /// Editor is a SpriteItem for showing an icon in the UI, it has no physic representation in the world
    /// </summary>
    public class Editor : SpriteItem, IGameStateTool
    {
        public override ushort ClassId
        {
            get { return LostIslandEntityClassId.Editor; }
        }

        public override string DisplayName
        {
            get { return "Editor"; }
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }

        public override string Description
        {
            get { return "Editor tool edits any entity"; }
        }


        public IToolImpact Use(IDynamicEntity owner, byte useMode, bool runOnServer)
        {
            throw new NotImplementedException();
        }

        public void Rollback(IToolImpact impact)
        {
            throw new NotImplementedException();
        }
    }
}
