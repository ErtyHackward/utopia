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

        //protected byte SelectedCube; XXX implement later selectedCube in palette per tool
        //protected byte SelectedColor;
        public string Name { get; set; }
        public string Status { get; set; }

        protected EditorTool(EntityEditor editor)
        {
            Editor = editor;
        }

        public abstract void Use();    
    }

    public class EditorAdd : EditorTool
    {
        public EditorAdd(EntityEditor editor) : base(editor)
        {
            Name = "Add";
        }

        public override void Use()
        {
            Editor.UpdateNewPlace(Editor.SelectedIndex);           
        }
    }
    
    public class EditorRemove : EditorTool
    {
        public EditorRemove(EntityEditor editor) : base(editor) { Name = "Remove"; }

        public override void Use()
        {
            Editor.UpdatePickedCube(0);
        }
    }

    public class EditorSelect : EditorTool
    {
        public EditorSelect(EntityEditor editor) : base(editor) { Name = "MultiSelect"; }

        public override void Use()
        {
            if  (Editor.MultiSelectEnabled )
            {
                //disabling multi select cleans selection
                Editor.ClearSelected();
                
            }
            Editor.MultiSelectEnabled = !Editor.MultiSelectEnabled;
            Status = Editor.MultiSelectEnabled ? "ON" : "OFF";
       
        }
    }

    /// <summary>
    /// EditorCopy for copy paste. copied blocks are stored in the copy tool instance, Paste tool depends on copy tool.
    /// This simple design allows multiple copy paste pair of tools !
    /// </summary>
    public class EditorCopy : EditorTool
    {
        public EditorCopy(EntityEditor editor) : base(editor) { Name = "Copy"; }

        public List<Located<Byte>> Copied = new List<Located<byte>>();

        public override void Use()
        {
            if (Editor.Selected.Count == 0) return;

            Copied.Clear();
            foreach (var selectedPos in Editor.Selected)
            {
               Copied.Add(new Located<byte>() { Location = selectedPos, Value = Editor.BlockAt(selectedPos) });
            }
            
        }
    }

    /// <summary>
    /// EditorPaste for copy paste. copied blocks are stored in the copy tool instance, Paste tool depends on copy tool.
    /// This simple design allows multiple copy paste pair of tools !
    /// </summary>
    public class EditorPaste : EditorTool
    {
        private readonly EditorCopy _copyTool;

        public EditorPaste(EntityEditor editor, EditorCopy copyTool) : base(editor)
        {
            Name = "Paste";
            _copyTool = copyTool;
        }

        public override void Use()
        {
            if (!Editor.PickedCubeLoc.HasValue || _copyTool.Copied.Count==0) return;

            Vector3I origin = Editor.PickedCubeLoc.Value;

            foreach (var copiedCube in _copyTool.Copied)
            {
                int x = origin.X + copiedCube.Location.X;
                int y = origin.Y + copiedCube.Location.Y;
                int z = origin.Z + copiedCube.Location.Z;

                Editor.SafeSetBlock(x, y, z, copiedCube.Value);

            }

        }
    }

    public class EditorSymetry : EditorTool
    {
        public EditorSymetry(EntityEditor editor) : base(editor) { Name = "Y Symetry"; }

        public override void Use()
        {
            Editor.VerticalSymetryEnabled =!Editor.VerticalSymetryEnabled;
            Status = Editor.VerticalSymetryEnabled ? "ON" : "OFF";
        }
    }

    public class EditorSave : EditorTool
    {
        public EditorSave(EntityEditor editor) : base(editor) { Name = "Save"; }

        public override void Use()
        {
            Editor.SaveEntity();
        }
    }

    public class EditorLoad : EditorTool
    {
        public EditorLoad(EntityEditor editor) : base(editor) { Name = "Load"; }

        public override void Use()
        {
            Editor.LoadEntity();
        }
    }

    public class EditorEditSelected : EditorTool
    {
        public EditorEditSelected(EntityEditor editor) : base(editor) { Name = "Edit"; }

        public override void Use()
        {
            Editor.EditSelectedEntity();
        }
    }

    internal class EditorEditSelf : EditorTool
    {
        public EditorEditSelf(EntityEditor editor) : base(editor) { Name = "EditSelf"; }
       
        public override void Use()
        {
            Editor.EditYourself();
        }
    }
}
