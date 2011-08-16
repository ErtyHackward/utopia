﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3Engines.D3D;
using S33M3Engines.Sprites;
using S33M3Engines.StatesManager;
using SharpDX.Direct3D11;
using S33M3Engines.Struct.Vertex;
using SharpDX;
using S33M3Engines.D3D.Effects.Basics;
using S33M3Engines.Struct;
using CeGui;
//using CeGui.Demo.DirectX;
using S33M3Engines.Sprites.GUI;

namespace Utopia.GUI.D3D
{
    public class GUI : GameComponent
    {
        SpriteRenderer _spriteRender;
        SpriteTexture _crosshair;
        SpriteFont _font;

        SpriteGuiRenderer _ceguiRenderer;
        GuiSheet _rootGuiSheet;

        public GUI(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
        }

        public override void LoadContent()
        {
            _crosshair = new SpriteTexture(Game, @"Textures\Gui\Crosshair.png");

            _spriteRender = new SpriteRenderer();
            _spriteRender.Initialize(Game);

            _font = new SpriteFont();
            _font.Initialize("Segoe UI Mono", 13f, System.Drawing.FontStyle.Regular, true, Game.GraphicDevice);

            InitCegui();
        
        }

        private void InitCegui()
        {
         /*   _ceguiRenderer = new SpriteGuiRenderer(Game, _spriteRender);

            CeGui.GuiSystem.Initialize(_ceguiRenderer);
            _ceguiRenderer.loadCeGuiResources();
            _ceguiRenderer.setupDefaults();
            WindowManager winMgr = WindowManager.Instance;
            _rootGuiSheet = winMgr.CreateWindow("DefaultWindow", "Root") as GuiSheet;

            GuiSystem.Instance.GuiSheet = _rootGuiSheet;

            VideoModeSelectionForm videoModeSelector = new VideoModeSelectionForm(
                new CeGui.WidgetSets.Suave.SuaveGuiBuilder());
            ((CeGui.Window)videoModeSelector).SetFont("WindowTitle");
            _rootGuiSheet.AddChild(videoModeSelector);*/
        }

        public override void UnloadContent()
        {
            _spriteRender.Dispose();
            _crosshair.Dispose();
            _font.Dispose();
        }

        public override void Update(ref GameTime TimeSpent)
        {
            
        }

        public override void Interpolation(ref double interpolation_hd, ref float interpolation_ld)
        {
        }

        //Draw at 2d level ! (Last draw called)
        public override void DrawDepth2()
        {
            _spriteRender.Begin(SpriteRenderer.FilterMode.Linear);
            _spriteRender.Render(_crosshair, ref _crosshair.ScreenPosition, new Color4(1, 0, 0, 1));
            
            //InjectInputToCegui();

           // RenderGui();

            _spriteRender.End();
        }

        private static void RenderGui()
        {
            CeGui.GuiSystem.Instance.RenderGui();
        }

        private static void InjectInputToCegui()
        {
            //_spriteRender.RenderText(_font, "That's Bumbas baby !\nDeuxième ligne !", Matrix.Translation(0, 0, 0), new Color4(1, 1, 0, 1));

            // Here is what cygon ( author of ceguisharp branch we use , that later became nuclex ui ) : 
            // "
            // We do input processing here instead of in Update() because it makes no sense to
            // handle input on another basis than per frame. The Update() calls in XNA are done
            // batch-wise and not regularly in a background thread, so there's nothing to gain
            // from moving this into update, not even better responsiveness (user still would
            // have to hold the mouse button down for an entire frame to achieve any effect).
            //"
            InputInjector.processMouseInput();

            //TODO pass gameTime to draw methods ? autorepeat feature is disabled cause there s no gametime in draw methods  
            InputInjector.processKeyboardInput();
        }
    }
}
