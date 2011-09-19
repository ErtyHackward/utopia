using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Shared.Structs;

namespace Utopia.Editor
{
    public abstract class EditorTool
    {
        protected EntityEditor Editor;

        protected byte SelectedCube;
        protected byte SelectedColor;


        protected EditorTool(EntityEditor editor)
        {
            Editor = editor;
        }

        public abstract void Use();    
    }

    public class EditorAdd : EditorTool
    {
        public EditorAdd(EntityEditor editor) : base(editor){}

        public override void Use()
        {
            Editor.UpdatePickedCube(Editor.SelectedIndex);           
        }
    }
    
    public class EditorRemove : EditorTool
    {
        public EditorRemove(EntityEditor editor): base(editor){}

        public override void Use()
        {
            Editor.UpdatePickedCube(0);
        }
    }

    public class EditorPaste : EditorTool
    {
        public EditorPaste(EntityEditor editor) : base(editor) { }

        public override void Use()
        {
            List<Located<Byte>> copied = new List<Located<byte>>();
            
            if (!Editor.PickedCube.HasValue) return;

            Location3<int> origin = Editor.PickedCube.Value;

            foreach (var copiedCube in copied)
            {
                int x = origin.X + copiedCube.Location.X;
                int y = origin.Y + copiedCube.Location.Y;
                int z = origin.Z + copiedCube.Location.Z;
               
                Editor.SafeSetBlock(x, y, z,copiedCube.Value);
                
            }
            
        }
    }

    
}
