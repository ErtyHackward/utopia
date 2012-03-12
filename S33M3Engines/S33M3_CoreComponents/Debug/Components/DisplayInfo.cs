using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3_DXEngine.Main;
using SharpDX;
using S33M3_DXEngine;
using System.Drawing;
using S33M3_CoreComponents.Sprites;
using S33M3_Resources.Structs;
using SharpDX.Direct3D11;
using S33M3_DXEngine.Debug.Interfaces;

namespace S33M3_CoreComponents.Components.Debug
{
    public class DisplayInfo : DrawableGameComponent
    {
        SpriteFont _font;
        SpriteRenderer _spriteRender;
        Matrix _textPosition = Matrix.Translation(5, 0, 0);
        private StringBuilder _sb = new StringBuilder();
        private Game _game;

        List<IDebugInfo> _components = new List<IDebugInfo>();

        public List<IDebugInfo> Components
        {
            get { return _components; }
        }

        private ByteColor _fontColor = new ByteColor(Color.Yellow.R, Color.Yellow.G, Color.Yellow.B, (byte)255);
        private D3DEngine _d3dEngine;

        public DisplayInfo(D3DEngine d3dEngine, Game game)
        {
            _game = game;

            _game.GameComponents.ComponentAdded += GameComponents_ComponentAdded;
            _game.GameComponents.ComponentRemoved += GameComponents_ComponentRemoved;

            _d3dEngine = d3dEngine;
            DrawOrders.UpdateIndex(0, 10000);

            //Add all exiting components
            foreach (IDebugInfo comp in _game.GameComponents)
            {
                AddComponants(comp);
            }
        }

        public override void Dispose()
        {
            _game.GameComponents.ComponentAdded -= GameComponents_ComponentAdded;
            _game.GameComponents.ComponentRemoved -= GameComponents_ComponentRemoved;
            base.Dispose();
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

        public override void LoadContent(DeviceContext Context)
        {
            _font = ToDispose(new SpriteFont());
            _font.Initialize("Lucida Console", 10f, System.Drawing.FontStyle.Regular, true, _d3dEngine.Device);
            _spriteRender = ToDispose(new SpriteRenderer());
            _spriteRender.Initialize(_d3dEngine);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        public override void Update(GameTime timeSpend)
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
            _spriteRender.Begin(context ,false, SpriteRenderer.FilterMode.Point);
            _spriteRender.DrawText(_font, _sb.ToString(), _textPosition, _fontColor);
            _spriteRender.End();
        }

    }
}
