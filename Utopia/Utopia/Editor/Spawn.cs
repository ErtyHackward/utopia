using System;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;

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
            Name = "Test";
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
            Name = "Filled";
        }

        protected override void FillFunction(int x, int y, int z)
        {
            Entity.Model.Blocks[x, y, z] = Editor.SelectedCubeId;
        }
    }

    public class SpawnBorder : Spawn
    {
        public SpawnBorder(EntityEditor editor)
            : base(editor)
        {
            Name = "Borders";
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
                Entity.Model.Blocks[x, y, z] = Editor.SelectedCubeId;
            }
            else
                Entity.Model.Blocks[x, y, z] = 0;
        }
    }

    public class SpawnCenter : Spawn
    {
        public SpawnCenter(EntityEditor editor)
            : base(editor)
        {
            Name = "Center";
        }
        public override void Use()
        {
            int xc = Entity.Model.Blocks.GetLength(0)/2;
            int yc = Entity.Model.Blocks.GetLength(1)/2;
            int zc = Entity.Model.Blocks.GetLength(2)/2;

            for (int x = xc-1; x < xc+1; x++)
            {
                for (int y = yc-1; y < yc+1; y++)
                {
                    for (int z = zc-1; z < zc+1; z++)
                    {
                        Entity.Model.Blocks[x, y, z] = Editor.SelectedCubeId;
                    }
                }
            }

            Editor.SpawnEntity(Entity);
        }
        protected override void FillFunction(int x, int y, int z)
        {
            throw new InvalidOperationException("no need to call fillFunctiuon with spawnOne");
        }
    }

    public class SpawnAxis : Spawn
    {
        public SpawnAxis(EntityEditor editor)
            : base(editor)
        {
            Name = "Axis";
        }

        protected override void FillFunction(int x, int y, int z)
        {

            int xc = Entity.Model.Blocks.GetLength(0) / 2;
            int yc = Entity.Model.Blocks.GetLength(1) / 2;
            int zc = Entity.Model.Blocks.GetLength(2) / 2;

            int n = 0;//if someone wants to replace this hack by real boolean logic, please do ! im too tired atm !
            if (x == xc || x == xc-1) n++;
            if (y == yc || y == yc - 1) n++;
            if (z == zc || z == zc - 1)n++;
                
            if (n>1)
            {
                Entity.Model.Blocks[x, y, z] = Editor.SelectedCubeId;                
            }
            else
                Entity.Model.Blocks[x, y, z] = 0;
        }
    }
}