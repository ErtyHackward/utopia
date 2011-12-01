using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Utopia;

namespace LostIsland.Client.States
{
    /// <summary>
    /// Main gameplay stuff. Displaying the chunks, an entities, handling an input
    /// </summary>
    public class GamePlayState : GameState
    {
        public override string Name
        {
            get { return "Gameplay"; }
        }

        public GamePlayState(IKernel iocContainer)
        {
            //EnabledComponents.Add(


        }
    }
}
