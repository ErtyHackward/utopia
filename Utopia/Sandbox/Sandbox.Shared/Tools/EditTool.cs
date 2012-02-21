using System;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.Entities.Interfaces;
using Utopia.Shared.Entities.Inventory;
using Utopia.Shared.Interfaces;

namespace LostIsland.Shared.Tools
{
    public class EditTool : BlockRemover
    {
        public override ushort ClassId
        {
            get { return LostIslandEntityClassId.EditTool; }
        }

        public override string DisplayName
        {
            get { return "Edit tool (or spell or scroll, not decided)"; }
        }


        public override string Description
        {
            get { return "Edit whatever is selected"; }
        }

        public EditTool(ILandscapeManager2D landscapeManager) : base(landscapeManager)
        {
        }

        public override IToolImpact Use(IDynamicEntity owner, byte useMode, bool runOnServer = false)
        {
            var impact = new ToolImpact {Success = false};
            if (! (owner is PlayerCharacter))
            {
                impact.Message = "Only plaer can edit";
                return impact;
            }

            if (owner.EntityState.IsPickingActive)
            {
                if (owner.EntityState.IsEntityPicked)
                {
                    throw new NotImplementedException("Edit entity");
                    //return EntityImpact(owner);
                }
                else
                {
                    IToolImpact removal = BlockImpact(owner);
                    if (removal.Success)
                    {
                        (owner as PlayerCharacter).EnterEditMode=true;
                    }
                }
            }
            else
            {
                impact.Message = "No target selected for use";
            }

            return impact;
        }
    }
}