using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using Utopia.GUI;
using S33M3_DXEngine.Main;
using Utopia.Worlds.Chunks;
using Utopia.Shared.Settings;
using S33M3DXEngine;

namespace Utopia.Components
{
    /// <summary>
    /// Class that will be able to do "Admin stuff" on the game
    /// </summary>
    public class AdminConsole : BaseComponent
    {
        #region Private Variables
        private ChatComponent _chatComp;
        private IWorldChunks _worldChunk;
        private D3DEngine _engine;
        #endregion

        #region Public Properties
        #endregion

        public AdminConsole(ChatComponent chatComp,
                            IWorldChunks worldChunk,
                            D3DEngine engine)
        {
            _chatComp = chatComp;
            _worldChunk = worldChunk;
            _engine = engine;
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

            switch (command.ToLower())
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
                default:
                    break;
            }

            return commandProcessed;
        }
        #endregion

    }
}
