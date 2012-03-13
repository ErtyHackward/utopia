using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Network;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using System.Diagnostics;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Sprites;
using S33M3Resources.Structs;
using S33M3DXEngine;
using S33M3CoreComponents.Inputs.Actions;
using S33M3CoreComponents.Inputs;
using Utopia.Action;
using S33M3CoreComponents.Inputs.KeyboardHandler;

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

        private TextInput _textInput;
        private bool _refreshDisplay;
        private long _hideChatInTick;
        private long _lastUpdateTime;
        private ByteColor _fontColor = new ByteColor((byte)Colors.White.Red * 255, (byte)Colors.White.Green * 255, (byte)Colors.White.Blue * 255, (byte)128);
        private D3DEngine _d3dEngine;
        private readonly InputsManager _imanager;
        private readonly ServerComponent _server;
        private readonly Queue<string> _messages = new Queue<string>();
        private float windowHeight;

        public int ChatLineLimit { get; set; }

        private bool _activated;

        public bool Activated
        {
            get { return _activated; }
            private set
            {
                _activated = value;
                if (value)
                {
                    _imanager.ActionsManager.IsExclusiveMode = true;
                }
                else
                {
                    _imanager.ActionsManager.IsExclusiveMode = false;
                }
            }
        }

        public ChatComponent(D3DEngine engine, InputsManager imanager, ServerComponent server)
        {

            _d3dEngine = engine;
            _imanager = imanager;
            _server = server;

            _server.ServerConnection.MessageChat += ServerConnectionMessageChat;

            ChatLineLimit = 30;
            //For 5 seconds =
            _hideChatInTick = 15 * Stopwatch.Frequency;

            _d3dEngine.ViewPort_Updated += LocateChat;

            LocateChat(_d3dEngine.ViewPort);

            // make it drawn on top
            DrawOrders.UpdateIndex(0, 10001);
        }

        public override void Dispose()
        {
            _server.ServerConnection.MessageChat -= ServerConnectionMessageChat;
            _d3dEngine.ViewPort_Updated -= LocateChat;
            base.Dispose();
        }

        void ServerConnectionMessageChat(object sender, ProtocolMessageEventArgs<ChatMessage> e)
        {
            //Cut the received message by line feed
            foreach (var msgText in e.Message.Message.Split('\n'))
            {
                if (e.Message.Action)
                {
                    AddMessage(string.Format("* {0} {1}", e.Message.DisplayName, msgText));
                }
                else AddMessage(string.Format("<{0}> {1}", e.Message.DisplayName, msgText));
            }

            _lastUpdateTime = Stopwatch.GetTimestamp();
        }

        public void AddMessage(string message)
        {
            _messages.Enqueue(string.Format("[{1}] {0}", message, DateTime.Now.TimeOfDay.ToString(@"hh\:mm\:ss")));
            if (_messages.Count > ChatLineLimit)
            {
                _messages.Dequeue(); //Remove the Olds messages (FIFO collection)
            }

            _refreshDisplay = true;
        }

        private void LocateChat(Viewport viewport)
        {
            windowHeight = viewport.Height;
        }

        public override void Initialize()
        {
            _textInput = new TextInput(_imanager.KeyboardManager);
        }

        public override void LoadContent(DeviceContext Context)
        {
            _font = ToDispose( new SpriteFont());
            _font.Initialize("Lucida Console", 12f, System.Drawing.FontStyle.Regular, true, _d3dEngine.Device);
            _spriteRender = ToDispose(new SpriteRenderer());
            _spriteRender.Initialize(_d3dEngine);
        }

        private void SetFontAlphaColor(byte color)
        {
            if (_fontColor.A != color)
            {
                _fontColor.A = color;
                _refreshDisplay = true;
            }
        }

        public override void Update(GameTime timeSpend)
        {

            if (Stopwatch.GetTimestamp() > _lastUpdateTime + _hideChatInTick)
            {
                SetFontAlphaColor(50);
            }
            else
            {
                SetFontAlphaColor(200);
            }


            if (Activated == true && _imanager.ActionsManager.isTriggered(UtopiaActions.Exit_Chat, CatchExclusiveActions))
            {
                Activated = false;
                _textInput.Clear();
                _lastUpdateTime = Stopwatch.GetTimestamp();
                _refreshDisplay = true;
            }

            if (_imanager.ActionsManager.isTriggered(UtopiaActions.Toggle_Chat, CatchExclusiveActions))
            {
                if (Activated == false)
                {
                    CatchExclusiveActions = true;
                    _lastUpdateTime = Stopwatch.GetTimestamp();

                    Activated = true;
                    _refreshDisplay = true;
                    _textInput.Clear();
                    _textInput.isListening = true;
                }
                else
                {
                    //Send message to server !
                    Activated = false;
                    CatchExclusiveActions = false;
                    string Input = _textInput.GetText();
                    if (!string.IsNullOrWhiteSpace(Input))
                    {
                        var msg = new ChatMessage { DisplayName = _server.DisplayName, Message = Input };
                        if (Input.StartsWith("/me ", StringComparison.CurrentCultureIgnoreCase))
                        {
                            msg.Action = true;
                            msg.Message = Input.Remove(0, 4);
                        }
                        _server.ServerConnection.SendAsync(msg);
                    }
                    _textInput.isListening = false;
                }
            }

            
            _textInput.Refresh();

        }

        public override void Draw(DeviceContext context, int index)
        {
            if (!Activated && _refreshDisplay == false)
            {
                _spriteRender.ReplayLast();
                return;
            }

            int _showCaret = -1;
            string Input = string.Empty;
            var builder = new StringBuilder();

            foreach (var message in _messages)
            {
                builder.AppendLine(message); //Display the "OLD" test.
            }
            if (Activated)
            {
                if (_textInput.GetText(out Input, out _showCaret))
                {
                    _lastUpdateTime = Stopwatch.GetTimestamp();
                    _refreshDisplay = true;
                }

                if(_showCaret != -1) _showCaret += builder.Length + 2;
                builder.Append(string.Format("{0} {1}", ">", Input));
            }

            _spriteRender.Begin(context, false, SpriteRenderer.FilterMode.Point);
            _textPosition = Matrix.Translation(5, windowHeight - (100 + _messages.Count * _font.CharHeight), 0);
            _spriteRender.DrawText(_font, builder.ToString(), _textPosition, _fontColor, -1, -1,  _showCaret);
            _spriteRender.End();
            _refreshDisplay = false;
        }
    }
}
