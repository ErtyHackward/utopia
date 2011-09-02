﻿using System;

namespace Utopia.Shared.Chunks.Entities.Concrete
{

    //dont implement this one on server for now 
    public class EditableVoxelEntity : VoxelEntity
    {
        public override EntityClassId ClassId
        {
            get { return EntityClassId.EditableEntity; }
        }

        public override string DisplayName
        {
            get { return "Editable"; }
        }
    }
}