using System;
using System.Collections.Generic;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Network;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using System.Diagnostics;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Sprites;
using S33M3Resources.Structs;
using S33M3DXEngine;
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
        Vector2 _textPosition = new Vector2(5, 200);

        private TextInput _textInput;
        private bool _refreshDisplay;
        private long _hideChatInTick;
        private long _lastUpdateTime;
        private ByteColor _fontColor = new ByteColor((byte)Color.White.R * 255, (byte)Color.White.G * 255, (byte)Color.White.B * 255, (byte)128);
        private readonly D3DEngine _d3dEngine;
        private readonly InputsManager _imanager;
        private readonly ServerComponent _server;
        private readonly Queue<string> _messages = new Queue<string>();
        private float _windowHeight;
        private bool _activated;

        /// <summary>
        /// Occurs when the chat message is ready to be sent to the server, allows to supress this operation
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> MessageOut;

        protected void OnMessageOut(ChatMessageEventArgs e)
        {
            var handler = MessageOut;
            if (handler != null) handler(this, e);
        }

        public int ChatLineLimit { get; set; }
        
        public bool Activated
        {
            get { return _activated; }
            private set
            {
                _activated = value;
                _imanager.ActionsManager.IsFullExclusiveMode = value;
            }
        }

        public ChatComponent(D3DEngine engine, InputsManager imanager, ServerComponent server)
        {
            IsDefferedLoadContent = true;

            _d3dEngine = engine;
            _imanager = imanager;
            _server = server;

            _server.MessageChat += ServerConnectionMessageChat;

            ChatLineLimit = 30;
            //For 5 seconds =
            _hideChatInTick = 15 * Stopwatch.Frequency;

            _d3dEngine.ViewPort_Updated += LocateChat;

            LocateChat(_d3dEngine.ViewPort, _d3dEngine.BackBufferTex.Description);

            // make it drawn on top
            DrawOrders.UpdateIndex(0, 10000);
        }

        public override void BeforeDispose()
        {
            _server.MessageChat -= ServerConnectionMessageChat;
            _d3dEngine.ViewPort_Updated -= LocateChat;            
        }

        public override void Initialize()
        {
            _textInput = new TextInput(_imanager.KeyboardManager);
        }

        public override void LoadContent(DeviceContext context)
        {
            _font = ToDispose(new SpriteFont());
            _font.Initialize("Lucida Console", 12f, System.Drawing.FontStyle.Regular, true, _d3dEngine.Device);
            _spriteRender = ToDispose(new SpriteRenderer(_d3dEngine));
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

        private void LocateChat(Viewport viewport, Texture2DDescription newBackBufferDescr)
        {
            _windowHeight = viewport.Height;
            _refreshDisplay = true;
        }

        private void ServerConnectionMessageChat(object sender, ProtocolMessageEventArgs<ChatMessage> e)
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


            if (Activated && _imanager.ActionsManager.isTriggered(UtopiaActions.Exit_Chat, CatchExclusiveActions))
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
                    var input = _textInput.GetText();
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        var ea = new ChatMessageEventArgs { Message = input };

                        OnMessageOut(ea);

                        if (!ea.DoNotSend)
                        {
                            var msg = new ChatMessage { DisplayName = _server.DisplayName, Message = input };
                            if (input.StartsWith("/me ", StringComparison.CurrentCultureIgnoreCase))
                            {
                                msg.Action = true;
                                msg.Message = input.Remove(0, 4);
                            }
                            _server.ServerConnection.SendAsync(msg);
                        }
                    }
                    _textInput.isListening = false;
                    _textInput.Clear();
                    _refreshDisplay = true;
                }
            }
            
            _textInput.Refresh();
        }

        public override void Draw(DeviceContext context, int index)
        {
            if (!Activated && _refreshDisplay == false)
            {
                _spriteRender.ReplayLast(context);
                return;
            }

            var showCaret = -1;
            var builder = new StringBuilder();

            foreach (var message in _messages)
            {
                builder.AppendLine(message); //Display the "OLD" test.
            }
            if (Activated)
            {
                string input;
                if (_textInput.GetText(out input, out showCaret))
                {
                    _lastUpdateTime = Stopwatch.GetTimestamp();
                    _refreshDisplay = true;
                }

                if(showCaret != -1) showCaret += builder.Length + 2;
                builder.Append(string.Format("{0} {1}", ">", input));
            }

            _spriteRender.Begin(false);
            _textPosition = new Vector2(5, _windowHeight - (100 + _messages.Count * _font.CharHeight));
            _spriteRender.DrawText(_font, builder.ToString(), ref _textPosition, ref _fontColor, -1, showCaret);
            _spriteRender.End(context);
            _refreshDisplay = false;
        }
    }

    public class ChatMessageEventArgs : EventArgs
    {
        /// <summary>
        /// Allows to prevent this message to be sent to the server
        /// </summary>
        public bool DoNotSend { get; set; }

        /// <summary>
        /// Gets the message
        /// </summary>
        public string Message { get; set; }

    }
}
