using System;
using Utopia.Shared.Chunks.Entities;
using Utopia.Shared.Chunks.Entities.Concrete;

namespace Utopia.Editor
{
    public class Spawn : EditorTool
    {
        protected readonly VoxelEntity Entity;
        protected readonly int Xmax;
        protected readonly int Ymax;
        protected readonly int Zmax;

        public Spawn(EntityEditor editor) : base(editor)
        {
            Name = "Spawn";
            Entity = new EditableVoxelEntity();
            Entity.Model.Blocks = new byte[16,16,16];
            Xmax = Entity.Model.Blocks.GetLength(0) - 1;
            Ymax = Entity.Model.Blocks.GetLength(1) - 1;
            Zmax = Entity.Model.Blocks.GetLength(2) - 1;
        }


        protected virtual void FillFunction(int x, int y, int z)
        {
            if (x == 0)
                Entity.Model.Blocks[x, y, z] = 1;
            else if (y == 0)
                Entity.Model.Blocks[x, y, z] = 4;
            else if (z == 0)
                Entity.Model.Blocks[x, y, z] = 7;
            else if (x == Xmax)
                Entity.Model.Blocks[x, y, z] = 9;
            else if (y == Ymax)
                Entity.Model.Blocks[x, y, z] = 13;
            else if (z == Zmax)
                Entity.Model.Blocks[x, y, z] = 7;
            else
                Entity.Model.Blocks[x, y, z] = 8;

            Entity.Model.Blocks[0, 0, 0] = 2;
            Entity.Model.Blocks[1, 0, 0] = 3;
        }


        public override void Use()
        {
            Fill(Entity, FillFunction);

            Editor.SpawnEntity(Entity);
        }

        private static void Fill(VoxelEntity entity, Action<int, int, int> fillFunction)
        {
            for (int x = 0; x < entity.Model.Blocks.GetLength(0); x++)
            {
                for (int y = 0; y < entity.Model.Blocks.GetLength(1); y++)
                {
                    for (int z = 0; z < entity.Model.Blocks.GetLength(2); z++)
                    {
                        fillFunction(x, y, z);
                    }
                }
            }
        }
    }

    public class SpawnPlain : Spawn
    {
        public SpawnPlain(EntityEditor editor) : base(editor)
        {
            Name = "S-Plain";
        }

        protected override void FillFunction(int x, int y, int z)
        {
            Entity.Model.Blocks[x, y, z] = Editor.SelectedIndex;
        }
    }

    public class SpawnBorder : Spawn
    {
        public SpawnBorder(EntityEditor editor)
            : base(editor)
        {
            Name = "S-Border";
        }

        protected override void FillFunction(int x, int y, int z)
        {
            int n = 0;
            if (x == 0) n++;
            if (y == 0) n++;
            if (z == 0) n++;
            if (x == Xmax) n++;
            if (y == Ymax) n++;
            if (z == Zmax) n++;
            if (n > 1)
            {
                Entity.Model.Blocks[x, y, z] = Editor.SelectedIndex;
            }
            else
                Entity.Model.Blocks[x, y, z] = 0;
        }
    }
}