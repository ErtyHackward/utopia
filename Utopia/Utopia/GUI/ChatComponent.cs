using System;
using System.Collections.Generic;
using System.Text;
using SharpDX;
using SharpDX.Direct3D11;
using Utopia.Network;
using Utopia.Shared.Entities.Events;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using System.Diagnostics;
using S33M3DXEngine.Main;
using S33M3CoreComponents.Sprites2D;
using S33M3Resources.Structs;
using S33M3DXEngine;
using S33M3CoreComponents.Inputs;
using Utopia.Action;
using S33M3CoreComponents.Inputs.KeyboardHandler;
using Utopia.Components;

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
        private ByteColor _fontColor = new ByteColor(Color.White.R, Color.White.G, Color.White.B ,(byte)128);
        private readonly D3DEngine _d3dEngine;
        private readonly InputsManager _imanager;
        private readonly ServerComponent _server;
        private readonly Queue<string> _messages = new Queue<string>();
        private float _windowHeight;
        private bool _activated;

        public AdminConsole Console { get; set; }


        /// <summary>
        /// Occurs when the chat message is ready to be sent to the server, allows to supress this operation
        /// </summary>
        public event EventHandler<ChatMessageEventArgs> MessageOut;

        protected void OnMessageOut(ChatMessageEventArgs e)
        {
            var handler = MessageOut;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// Occurs when chat component is activated or deactivated
        /// </summary>
        public event EventHandler ActivatedChanged;

        protected virtual void OnActivatedChanged()
        {
            var handler = ActivatedChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public int ChatLineLimit { get; set; }
        public bool IsHided { get; set; }
        public bool Activated
        {
            get { return _activated; }
            private set
            {
                if (_activated == value)
                    return;

                _activated = value;
                _imanager.ActionsManager.IsFullExclusiveMode = value;
                OnActivatedChanged();
            }
        }

        public ChatComponent(D3DEngine engine, InputsManager imanager, ServerComponent server)
        {
            IsDefferedLoadContent = true;

            _d3dEngine = engine;
            _imanager = imanager;
            _server = server;

            _server.MessageChat += ServerConnectionMessageChat;
            _server.MessagePing += _server_MessagePing;
            _server.MessageUseFeedback += _server_MessageUseFeedback;

            ChatLineLimit = 30;
            //For 5 seconds =
            _hideChatInTick = 15 * Stopwatch.Frequency;

            _d3dEngine.ScreenSize_Updated += LocateChat;

            LocateChat(_d3dEngine.ViewPort, _d3dEngine.BackBufferTex.Description);
            IsHided = false;

            // make it drawn on top
            DrawOrders.UpdateIndex(0, 10000);
        }

        public override void BeforeDispose()
        {
            _server.MessagePing -= _server_MessagePing;
            _server.MessageChat -= ServerConnectionMessageChat;
            _d3dEngine.ScreenSize_Updated -= LocateChat;            
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

            _lastUpdateTime = Stopwatch.GetTimestamp();
            _refreshDisplay = true;
        }

        private void LocateChat(ViewportF viewport, Texture2DDescription newBackBufferDescr)
        {
            _windowHeight = viewport.Height;
            _refreshDisplay = true;
        }

        private void ServerConnectionMessageChat(object sender, ProtocolMessageEventArgs<ChatMessage> e)
        {
            //Cut the received message by line feed
            foreach (var msgText in e.Message.Message.Split('\n'))
            {
                if (e.Message.IsServerMessage)
                {
                    AddMessage(string.Format("-{0}- {1}", e.Message.DisplayName, msgText));
                }
                else
                {
                    if (e.Message.Action)
                    {
                        AddMessage(string.Format("* {0} {1}", e.Message.DisplayName, msgText));
                    }
                    else AddMessage(string.Format("<{0}> {1}", e.Message.DisplayName, msgText));
                }
            }
        }

        private void _server_MessagePing(object sender, ProtocolMessageEventArgs<PingMessage> e)
        {
            AddMessage(string.Format("<Pong> {0} ms", Console.PingTimer.ElapsedMilliseconds));
            Console.PingTimer.Stop();
        }

        private void _server_MessageUseFeedback(object sender, ProtocolMessageEventArgs<UseFeedbackMessage> e)
        {
            if (e.Message.OwnerDynamicId == _server.Player.DynamicId && !e.Message.Impact.Success && !string.IsNullOrEmpty(e.Message.Impact.Message))
            {
                AddMessage(string.Format(" -- {0}", e.Message.Impact.Message));
            }
        }

        private void SetFontAlphaColor(byte color)
        {
            if (_fontColor.A != color)
            {
                _fontColor.A = color;
                _refreshDisplay = true;
            }
        }

        public override void FTSUpdate(GameTime timeSpend)
        {

            if (_imanager.ActionsManager.isTriggered(UtopiaActions.ToggleInterface))
            {
                IsHided = !IsHided;
            }

            if (Stopwatch.GetTimestamp() > _lastUpdateTime + _hideChatInTick)
            {
                SetFontAlphaColor(0);
            }
            else
            {
                SetFontAlphaColor(200);
            }


            if (Activated && _imanager.ActionsManager.isTriggered(UtopiaActions.ExitChat, CatchExclusiveActions))
            {
                Activated = false;
                _textInput.Clear();
                _lastUpdateTime = Stopwatch.GetTimestamp();
                _refreshDisplay = true;
            }

            if (_imanager.ActionsManager.isTriggered(UtopiaActions.ToggleChat, CatchExclusiveActions))
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

                            if (input.StartsWith("/"))
                                _server.ServerConnection.Send(new EntityUseMessage() { 
                                    DynamicEntityId = _server.Player.DynamicId, 
                                    UseType  = UseType.Command, 
                                    State = _server.Player.EntityState
                                });

                            _server.ServerConnection.Send(msg);
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
            if (IsHided) return;
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

            _spriteRender.Begin(false, context);
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
