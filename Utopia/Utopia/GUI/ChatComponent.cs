using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using S33M3Engines;
using S33M3Engines.D3D;
using S33M3Engines.D3D.DebugTools;
using S33M3Engines.Sprites;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Action;
using Utopia.InputManager;
using Utopia.Net.Messages;
using Utopia.Network;
using Utopia.Shared.Structs;

namespace Utopia.GUI
{
    /// <summary>
    /// Represents game chat component
    /// </summary>
    public class ChatComponent : DrawableGameComponent
    {
        SpriteFont _font;
        SpriteRenderer _spriteRender;
        Matrix _textPosition = Matrix.Translation(5, 200, 0);

        private Color4 _fontColor = new Color4(Color.White.A, Color.White.R, Color.White.G, Color.White.B);
        private D3DEngine _d3dEngine;
        private readonly ActionsManager _actionManager;
        private readonly InputsManager _imanager;
        private readonly Server _server;
        private Queue<string> _messages = new Queue<string>();
        private bool _showCaret = false;
        private DateTime _caretSwitch;
        private float windowHeight;

        public int ChatLineLimit { get; set; }
        
        public bool Activated { get; private set; }

        /// <summary>
        /// Current line 
        /// </summary>
        public string Input { get; set; }

        public ChatComponent(D3DEngine engine, ActionsManager actionManager, InputsManager imanager, Server server)
        {

            _d3dEngine = engine;
            _actionManager = actionManager;
            _imanager = imanager;
            _server = server;

            _server.ServerConnection.MessageChat += ServerConnection_MessageChat;

            ChatLineLimit = 30;

            _imanager.OnKeyPressed += _imanager_OnKeyPressed;
            _d3dEngine.ViewPort_Updated += LocateChat;

            LocateChat(_d3dEngine.ViewPort);

            // make it drawn on top
            DrawOrders.UpdateIndex(0, 1001);
        }

        void ServerConnection_MessageChat(object sender, Net.Connections.ProtocolMessageEventArgs<ChatMessage> e)
        {
            AddMessage(string.Format("<{0}> {1}", e.Message.Login, e.Message.Message));
        }

        public void AddMessage(string message)
        {
            _messages.Enqueue(string.Format("[{1}] {0}", message, DateTime.Now.TimeOfDay.ToString(@"h\:m\:s")));
            if (_messages.Count > ChatLineLimit)
            {
                _messages.Dequeue();
            }
        }

        void _imanager_OnKeyPressed(object sender, KeyPressEventArgs e)
        {
            if (Activated)
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    _actionManager.KeyboardActionsProcessing = true;
                    _imanager.KeyBoardListening = false;
                    if (!string.IsNullOrWhiteSpace(Input))
                    {
                        _server.ServerConnection.SendAsync(new ChatMessage { Login = _server.Player.DisplayName, Message = Input });
                    }

                    Input = string.Empty;
                    return;
                }
                if (e.KeyChar == (char)Keys.Back)
                {
                    if (Input.Length > 0)
                    {
                        Input = Input.Remove(Input.Length - 1);
                    }
                    return;
                }

                Input += e.KeyChar;
            }
        }

        private void LocateChat(Viewport viewport)
        {
            windowHeight = viewport.Height;
        }
        
        public override void LoadContent()
        {
            _font = new SpriteFont();
            _font.Initialize("Segoe UI Mono", 11.5f, System.Drawing.FontStyle.Regular, true, _d3dEngine.Device);
            _spriteRender = new SpriteRenderer();
            _spriteRender.Initialize(_d3dEngine);
        }

        public override void UnloadContent()
        {
            _font.Dispose();
            _spriteRender.Dispose();
        }

        public override void Update(ref GameTime timeSpent)
        {
            if (_actionManager.isTriggered(Actions.Toggle_Chat))
            {
                if (Activated)
                {
                    Activated = false;
                    _imanager.KeyBoardListening = false;
                    _actionManager.KeyboardActionsProcessing = true;
                }
                else
                {
                    Activated = true;
                    _actionManager.KeyboardActionsProcessing = false;
                    _imanager.KeyBoardListening = true;
                }
            }

            if (Activated)
            {
                // swap caret
                if ((DateTime.Now - _caretSwitch).TotalSeconds > 0.5)
                {
                    _showCaret = !_showCaret;
                    _caretSwitch = DateTime.Now;
                }

            }

        }

        public override void Draw(int Index)
        {
            _spriteRender.Begin(SpriteRenderer.FilterMode.Point);

            var builder = new StringBuilder();

            foreach (var message in _messages)
            {
                builder.AppendLine(message);
            }
            if (Activated)
            {
                builder.AppendFormat(">{0}{1}\n", Input, _showCaret ? "|" : "");
            }
            else builder.AppendLine();

            _textPosition = Matrix.Translation(5, windowHeight - (100 + _messages.Count * _font.CharHeight), 0);

            _spriteRender.RenderText(_font, builder.ToString(), _textPosition, _fontColor);
            _spriteRender.End();
        }
    }
}
