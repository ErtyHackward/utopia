#region

using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Entities.Voxel;
using Utopia.Worlds.Cubes;
using Utopia.Shared.Settings;

#endregion

namespace Utopia.Editor
{
    //public class EntityEditorUi
    //{
    //    private readonly EntityEditor _editorComponent;
    //    private WindowControl _texPalette;
    //    private WindowControl _colorPalette;
    //    private WindowControl _toolBar;
    //    private WindowControl _spawnBar;
        
    //    public List<Control> Children = new List<Control>();

    //    public EntityEditorUi(EntityEditor editorComponent)
    //    {
    //        _editorComponent = editorComponent;

    //        _toolBar = InitToolBar();

    //        _spawnBar = InitSpawnBar();

    //        _colorPalette = InitColorPalette();
    //        _texPalette = InitTexturePalette();

    //        Children.Add(_toolBar);
    //        Children.Add(_spawnBar);
    //        Children.Add(_colorPalette);
    //        Children.Add(_texPalette);
    //    }

    //    private WindowControl InitSpawnBar()
    //    {
    //        const int buttonSize = 64;

    //        const int margin = 20;

    //        List<EditorTool> tools = new List<EditorTool>
    //                                     {
    //                                         new Spawn(_editorComponent),
    //                                         new SpawnPlain(_editorComponent),
    //                                         new SpawnBorder(_editorComponent),
    //                                         new SpawnCenter(_editorComponent),
    //                                         new SpawnAxis(_editorComponent),
    //                                         new EditorEditSelected(_editorComponent),
    //                                         new EditorEditSelf(_editorComponent)
    //                                     };

    //        int buttonsNbr = tools.Count;

    //        WindowControl toolBar = new WindowControl();
    //        toolBar.Bounds = new UniRectangle(0.0f, 500, buttonsNbr*buttonSize, buttonSize + margin);
    //        toolBar.Title = "Spawn";

    //        for (int x = 0; x < tools.Count; x++)
    //        {
    //            EditorTool tool = tools[x];
    //            ButtonControl btn = new ButtonControl();
    //            btn.Bounds = new UniRectangle(x*buttonSize, margin, buttonSize, buttonSize);
    //            btn.Text = tool.Name;
    //            btn.Pressed += delegate
    //                               {
    //                                   tool.Use();
    //                                   btn.Text += tool.Status;
    //                               };

    //            toolBar.Children.Add(btn);
    //        }
    //        return toolBar;
    //    }

    //    private WindowControl InitToolBar()
    //    {
    //        const int buttonSize = 64;

    //        const int margin = 20;

    //        EditorCopy editorCopy = new EditorCopy(_editorComponent);

    //        List<EditorTool> tools = new List<EditorTool>
    //                                     {
    //                                         new EditorSymetry(_editorComponent),
    //                                         //new EditorAdd(_editorComponent), TODO finish edit tools
    //                                         //new EditorRemove(_editorComponent),
    //                                         //new EditorSelect(_editorComponent),
    //                                         //editorCopy,
    //                                         //new EditorPaste(_editorComponent, editorCopy),
    //                                         new EditorLoad(_editorComponent),
    //                                         new EditorSave(_editorComponent),
                                          
    //                                     };
    //        int buttonsNbr = tools.Count;

    //        WindowControl toolBar = new WindowControl();
    //        toolBar.Bounds = new UniRectangle(0.0f, 0, buttonsNbr*buttonSize, buttonSize + margin);
    //        toolBar.Title = "Edit tools";

    //        for (int x = 0; x < tools.Count; x++)
    //        {
    //            EditorTool tool = tools[x];
    //            ButtonControl btn = new ButtonControl();
    //            btn.Bounds = new UniRectangle(x*buttonSize, margin, buttonSize, buttonSize);
    //            btn.Text = tool.Name;
    //            btn.Pressed += delegate
    //                               {
    //                                   tool.Use();
    //                                   btn.Text += tool.Status;
    //                               };

    //            toolBar.Children.Add(btn);
    //        }
    //        return toolBar;
    //    }

    //    private WindowControl InitColorPalette()
    //    {
    //        //XXX parametrize UI sizes
    //        const int rows = 16;
    //        const int cols = 4;
    //        const int btnSize = 20;

    //        const int y0 = 20;
    //        const int x0 = 0;

    //        WindowControl palette = new WindowControl();
    //        palette.Bounds = new UniRectangle(0, 0, (cols)*btnSize, (rows + 1)*btnSize);

    //        int index = 0;
    //        for (int x = 0; x < cols; x++)
    //        {
    //            for (int y = 0; y < rows; y++)
    //            {
    //                if (index == ColorLookup.Colours.Count()) break;

    //                var color = ColorLookup.Colours[index];

    //                PaletteButtonControl btn = new PaletteButtonControl();
    //                btn.Bounds = new UniRectangle(x0 + x*btnSize, y0 + y*btnSize, btnSize, btnSize);
    //                btn.Color = color;
    //                int associatedindex = index; //for access inside closure 
    //                btn.Pressed += (sender, e) =>
    //                                   {
    //                                       _editorComponent.SelectedCubeId = (byte)associatedindex;
    //                                       _editorComponent.IsColor = true;
    //                                   };

    //                palette.Children.Add(btn);
    //                index++;
    //            }
    //        }
    //        return palette;
    //    }


    //    private WindowControl InitTexturePalette()
    //    {
    //        ShaderResourceView arrayResourceView = _editorComponent.Texture;

    //        const int rows = 8;
    //        const int cols = 4;
    //        const int btnSize = 34;

    //        const int y0 = 23;
    //        const int x0 = 3;

    //        WindowControl palette = new WindowControl();
    //        palette.Bounds = new UniRectangle(100, 0, (cols)*btnSize, (rows + 1)*btnSize);

    //        List<CubeProfile> filtered =
    //            RealmConfiguration.CubeProfiles.ToList().FindAll(p => ! p.IsEmissiveColorLightSource);

    //        int cubeProfileIndex = 1;
    //        for (int x = 0; x < cols; x++)
    //        {
    //            for (int y = 0; y < rows; y++)
    //            {
    //                if (cubeProfileIndex == filtered.Count) break;
    //                blockProfile profile = filtered[cubeProfileIndex];

    //                PaletteButtonControl btn = new PaletteButtonControl();
    //                btn.Bounds = new UniRectangle(x0 + x*btnSize, y0 + y*btnSize, btnSize, btnSize);
    //                btn.Texture = new SpriteTexture(btnSize, btnSize, arrayResourceView, Vector2.Zero);

    //                btn.Texture.Index = profile.Tex_Front.TextureArrayId;
    //                int associatedindex = profile.Id; //new variable for access inside btn.pressed closure 
    //                btn.Pressed += (sender, e) =>
    //                                   {
    //                                       _editorComponent.SelectedCubeId = (byte) associatedindex;
    //                                       _editorComponent.IsTexture = true;
    //                                   };
    //                palette.Children.Add(btn);

    //                cubeProfileIndex++;
    //            }
    //        }
    //        return palette;
    //    }
    //}

    
}