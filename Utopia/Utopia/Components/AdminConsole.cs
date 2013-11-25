﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.GUI;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Settings;
using S33M3DXEngine;
using S33M3DXEngine.Main;
using Utopia.Network;
using Utopia.Shared.Net.Messages;
using System.Diagnostics;

namespace Utopia.Components
{
    /// <summary>
    /// Class that will be able to do "Admin stuff" on the game
    /// </summary>
    public class AdminConsole : BaseComponent
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Private Variables
        private ChatComponent _chatComp;
        private IWorldChunks2D _worldChunk;
        private D3DEngine _engine;
        private Game _mainGameLoop;
        private ServerComponent _server;
        private Stopwatch _pingTimer = new Stopwatch();

        #endregion

        #region Public Properties
        public Stopwatch PingTimer { get { return _pingTimer; } }
        #endregion

        public AdminConsole(ChatComponent chatComp,
                            IWorldChunks2D worldChunk,
                            D3DEngine engine,
                            Game mainGameLoop,
                            ServerComponent server)
        {
            _chatComp = chatComp;
            _chatComp.Console = this;
            _worldChunk = worldChunk;
            _engine = engine;
            _mainGameLoop = mainGameLoop;
            _server = server;
            _chatComp.MessageOut += _chatComp_MessageOut;
        }

        public override void BeforeDispose()
        {

            _chatComp.MessageOut -= _chatComp_MessageOut;
        }

        #region Public Methods
        #endregion

        #region Private Methods
        //react to Admin command send via the Chat box
        private void _chatComp_MessageOut(object sender, ChatMessageEventArgs e)
        {
            e.DoNotSend = AnalyseAction(e.Message);
        }

        private bool AnalyseAction(string command)
        {
            bool commandProcessed = false;
            try
            {

                if (string.IsNullOrEmpty(command) == false && command[0] == '/')
                {
                    string[] splittedCmd = command.Split(' ');

                    switch (splittedCmd[0].ToLower())
                    {
                        case "/reloadtex":
                            //Refresh the texture pack values
                            TexturePackConfig.Current.Load();
                            _worldChunk.InitDrawComponents(_engine.ImmediateContext);
                            commandProcessed = true;
                            break;
                        case "/staticinstanced": //Swith from/to static instanced drawing for static entities
                            _worldChunk.DrawStaticInstanced = !_worldChunk.DrawStaticInstanced;
                            commandProcessed = true;
                            break;
                        case "/fpslimit":
                            _mainGameLoop.FramelimiterTime = (long)(1.0 / long.Parse(splittedCmd[1]) * 1000.0);
                            commandProcessed = true;
                            break;
                        case "/ping": //Send a ping to the server without using the chat system
                            _pingTimer.Restart();
                            _server.ServerConnection.Send(new PingMessage() { Request = true });
                            commandProcessed = true;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Error processing Admin command : {0}, error raised : {1}", command, e.Message); 
            }

            return commandProcessed;
        }
        #endregion

    }
}
