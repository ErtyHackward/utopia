using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3DXEngine.Main;
using SharpDX;
using S33M3DXEngine;
using S33M3CoreComponents.Sprites2D;
using S33M3Resources.Structs;
using SharpDX.Direct3D11;
using S33M3DXEngine.Debug.Interfaces;
using S33M3CoreComponents.Cameras.Interfaces;

namespace S33M3CoreComponents.Components.Debug
{
    public class DisplayInfo : DrawableGameComponent
    {
        private SpriteFont _font;
        private SpriteRenderer _spriteRender;
        private Vector2 _textPosition = new Vector2(205, 5);
        private StringBuilder _sb = new StringBuilder();
        private Game _game;

        List<IDebugInfo> _components = new List<IDebugInfo>();

        public List<IDebugInfo> Components
        {
            get { return _components; }
        }

        private ByteColor _fontColor = new ByteColor(Color.Red.R, Color.Red.G, Color.Red.B, (byte)255);
        private D3DEngine _d3dEngine;

        public DisplayInfo(D3DEngine d3dEngine, Game game)
        {
            _game = game;
            _game.GameComponents.ComponentAdded += GameComponents_ComponentAdded;
            _game.GameComponents.ComponentRemoved += GameComponents_ComponentRemoved;

            _d3dEngine = d3dEngine;
            DrawOrders.UpdateIndex(0, 10000);

            //Add all exiting components
            foreach (var components in _game.GameComponents)
            {
                if (components is IDebugInfo) AddComponants((IDebugInfo)components);
            }
        }

        public override void BeforeDispose()
        {
            _game.GameComponents.ComponentAdded -= GameComponents_ComponentAdded;
            _game.GameComponents.ComponentRemoved -= GameComponents_ComponentRemoved;
        }

        void GameComponents_ComponentRemoved(object sender, GameComponentCollectionEventArgs e)
        {
            IDebugInfo debugInfoComp = e.GameComponent as IDebugInfo;
            if (debugInfoComp == null) return;
            RemoveComponants(debugInfoComp);
        }

        void GameComponents_ComponentAdded(object sender, GameComponentCollectionEventArgs e)
        {
            IDebugInfo debugInfoComp = e.GameComponent as IDebugInfo;
            if (debugInfoComp == null) return;
            AddComponants(debugInfoComp);
        }

        public void SetComponants(params IDebugInfo[] args)
        {
            _components = new List<IDebugInfo>(args);
        }

        public void AddComponants(IDebugInfo comp)
        {
            _components.Add(comp);
        }

        public void RemoveComponants(IDebugInfo comp)
        {
            _components.Remove(comp);
        }

        public override void LoadContent(DeviceContext context)
        {
            _font = ToDispose(new SpriteFont());
            _font.Initialize("Lucida Console", 10f, System.Drawing.FontStyle.Regular, true, _d3dEngine.Device);
            _spriteRender = ToDispose(new SpriteRenderer(_d3dEngine, true));
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        public override void FTSUpdate(GameTime timeSpend)
        {
            _sb.Clear();
            for (int CompIndex = 0; CompIndex < _components.Count; CompIndex++)
            {
                if (_components[CompIndex].ShowDebugInfo)
                {
                    _sb.Append(_components[CompIndex].GetDebugInfo() + "\n");
                }
            }
        }

        public override void Draw(DeviceContext context, int index)
        {
            //Afficher la console, ou bien les infos !
            _spriteRender.Begin(false, context);
            _spriteRender.DrawText(_font, _sb.ToString(), ref _textPosition, ref _fontColor, -1, -1, SpriteRenderer.TextFontPosition.RelativeToFontUp);
            _spriteRender.End(context);
        }

    }
}
